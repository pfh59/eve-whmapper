using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WHMapper.Services.EveAPI
{
    public enum RequestSecurity
    {
        Public,
        Authenticated
    }

    public enum RequestMethod
    {
        Delete,
        Get,
        Post,
        Put
    }

    public abstract class AEveApiServices
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public AEveApiServices(HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Clear();

            _httpClient = httpClient;
            _token = token;
        }


        public async Task<T> Execute<T>(RequestSecurity security, RequestMethod method, string uri, object? body = null)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            //Add bearer token
            if (security == RequestSecurity.Authenticated)
            {
                if (_token == null)
                    throw new ArgumentException("SSO authentication requested");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            //Serialize post body data
            HttpContent postBody = null;
            if (body != null)
                postBody = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
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

            string result = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode != HttpStatusCode.NoContent && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Accepted))
            {
                return JsonSerializer.Deserialize<T>(result);
            }
            else
                return default(T);
        }
    }
}
