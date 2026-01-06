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
                ConfigureHeaders(security);
                HttpContent? postBody = SerializeBody(body);
                HttpResponseMessage? response = await ExecuteHttpRequest(method, uri, postBody).ConfigureAwait(false);

                if (response == null)
                    return Result<T>.Failure("No response received from server");

                return await HandleResponse<T>(response);
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

        private void ConfigureHeaders(RequestSecurity security)
        {
            // Always add the X-Compatibility-Date header for ESI API versioning
            // This replaces the old per-route versioning (v1, v2, etc.)
            if (!_httpClient.DefaultRequestHeaders.Contains(EveAPIServiceConstants.CompatibilityDateHeader))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    EveAPIServiceConstants.CompatibilityDateHeader, 
                    EveAPIServiceConstants.CompatibilityDateValue);
            }

            if (security == RequestSecurity.Authenticated)
            {
                if (UserToken == null)
                    throw new InvalidOperationException("UserToken is required for authenticated requests");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserToken.AccessToken);
            }
            else
            {
                // Clear authorization but keep the compatibility date header
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private HttpContent? SerializeBody(object? body)
        {
            return body == null ? null : new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }

        private async Task<HttpResponseMessage?> ExecuteHttpRequest(RequestMethod method, string uri, HttpContent? postBody)
        {
            return method switch
            {
                RequestMethod.Delete => await _httpClient.DeleteAsync(uri).ConfigureAwait(false),
                RequestMethod.Get => await _httpClient.GetAsync(uri).ConfigureAwait(false),
                RequestMethod.Post => await _httpClient.PostAsync(uri, postBody).ConfigureAwait(false),
                RequestMethod.Put => await _httpClient.PutAsync(uri, postBody).ConfigureAwait(false),
                _ => null,
            };
        }

        private async Task<Result<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return HandleRateLimit<T>(response);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return Result<T>.Success(default(T)!);

            if (IsSuccessStatusCode(response.StatusCode))
                return await HandleSuccessResponse<T>(response);

            return await HandleErrorResponse<T>(response);
        }

        private Result<T> HandleRateLimit<T>(HttpResponseMessage response)
        {
            var retryAfter = "unknown";
            if (response.Headers.TryGetValues("Retry-After", out var values))
                retryAfter = values.First();

            var retrySeconds = double.TryParse(retryAfter, out var seconds) ? seconds : 0;
            return Result<T>.Failure($"Rate limit exceeded. Retry after {retryAfter} seconds.", (int)response.StatusCode, null, TimeSpan.FromSeconds(retrySeconds));
        }

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.Accepted;
        }

        private async Task<Result<T>> HandleSuccessResponse<T>(HttpResponseMessage response)
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

        private async Task<Result<T>> HandleErrorResponse<T>(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorMessage = string.IsNullOrEmpty(errorContent)
                ? $"Request failed with status: {response.StatusCode} ({response.ReasonPhrase})"
                : $"Request failed ({response.StatusCode}): {errorContent}";

            return Result<T>.Failure(errorMessage, (int)response.StatusCode);
        }
    }
}

