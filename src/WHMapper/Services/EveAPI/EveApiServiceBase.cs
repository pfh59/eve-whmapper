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


        public async Task<T?> Execute<T>(RequestSecurity security, RequestMethod method, string uri, object? body = null)
        {
            //Add bearer token
            if (security == RequestSecurity.Authenticated)
            {
                if (UserToken == null)
                    throw new Exception("UserToken is required for authenticated requests");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserToken.AccessToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }

            //Serialize post body data
            HttpContent? postBody = null;

            if (body != null)
                postBody = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

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

            if (response != null && response.StatusCode != HttpStatusCode.NoContent && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Accepted))
            {
                string result = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(result))
                    return default(T);
                else
                    return JsonSerializer.Deserialize<T>(result);
            }
            else
                return default(T);
        }
    }
}
