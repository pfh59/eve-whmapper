using MudBlazor;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WHMapper.Models.DTO;

namespace WHMapper.Services.EveAPI
{
    public abstract class EveApiServiceBase
    {
        private readonly HttpClient _httpClient;
        protected UserToken? UserToken { get; private set; }

        public EveApiServiceBase(HttpClient httpClient, UserToken? userToken = null)
        {
            _httpClient = httpClient;
            UserToken = userToken;
        }


        /// <summary>
        /// Executes an HTTP request and returns a Result with success/error information
        /// </summary>
        public async Task<Result<T>> Execute<T>(RequestSecurity security, RequestMethod method, string uri, object? body = null)
        {
            try
            {
                // Add bearer token for authenticated requests
                if (security == RequestSecurity.Authenticated)
                {
                    if (UserToken == null)
                        return Result<T>.Failure("UserToken is required for authenticated requests", 401);

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserToken.AccessToken);
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                }

                // Serialize post body data
                HttpContent? postBody = null;
                if (body != null)
                    postBody = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                // Execute HTTP request based on method
                HttpResponseMessage? response = null;
                switch (method)
                {
                    case RequestMethod.Delete:
                        response = await _httpClient.DeleteAsync(uri).ConfigureAwait(false);
                        break;
                    case RequestMethod.Get:
                        response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
                        break;
                    case RequestMethod.Post:
                        response = await _httpClient.PostAsync(uri, postBody).ConfigureAwait(false);
                        break;
                    case RequestMethod.Put:
                        response = await _httpClient.PutAsync(uri, postBody).ConfigureAwait(false);
                        break;
                }

                if (response == null)
                    return Result<T>.Failure("No response received from server");

                // Handle rate limiting specifically
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = "unknown";
                    if (response.Headers.TryGetValues("Retry-After", out var values))
                    {
                        retryAfter = values.First();
                    }
                    return Result<T>.Failure($"Rate limit exceeded. Retry after {retryAfter} seconds.", (int)response.StatusCode, null,
                        TimeSpan.FromSeconds(double.TryParse(retryAfter, out var seconds) ? seconds : 0));
                }

                // Handle successful responses
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    // For NoContent responses, return success with default value
                    return Result<T>.Success(default(T)!);
                }

                if (response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Created ||
                    response.StatusCode == HttpStatusCode.Accepted)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(responseContent))
                        return Result<T>.Success(default(T)!);

                    try
                    {
                        var deserializedResult = JsonSerializer.Deserialize<T>(responseContent);
                        return Result<T>.Success(deserializedResult!);
                    }
                    catch (JsonException jsonEx)
                    {
                        return Result<T>.Failure($"Failed to deserialize response: {jsonEx.Message}", (int)response.StatusCode, jsonEx);
                    }
                }

                // Handle error responses
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = string.IsNullOrEmpty(errorContent)
                    ? $"Request failed with status: {response.StatusCode} ({response.ReasonPhrase})"
                    : $"Request failed ({response.StatusCode}): {errorContent}";

                return Result<T>.Failure(errorMessage, (int)response.StatusCode);
            }
            catch (HttpRequestException httpEx)
            {
                return Result<T>.Failure($"HTTP request failed: {httpEx.Message}", exception: httpEx);
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
            {
                return Result<T>.Failure("Request timed out", exception: tcEx);
            }
            catch (TaskCanceledException tcEx)
            {
                return Result<T>.Failure("Request was cancelled", exception: tcEx);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"Unexpected error occurred: {ex.Message}", exception: ex);
            }
        }
    }
}

