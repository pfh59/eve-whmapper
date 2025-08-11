using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveScout;

namespace WHMapper.Services.EveScoutAPI;

public class EveScoutAPIServices : IEveScoutAPIServices
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public EveScoutAPIServices(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        _httpClient = httpClient;

        // Check if httpClient the base URL is set correctly
        if (!_httpClient.BaseAddress?.ToString().StartsWith(EveScoutAPIServiceConstants.EveScoutUrl) ?? true)
        {
            throw new ArgumentException("HttpClient base address must start with the EveScout API URL.", nameof(httpClient));
        }
    }

    public Task<IEnumerable<EveScoutSystemEntry>?> GetTheraSystemsAsync()
    {
        return Execute<IEnumerable<EveScoutSystemEntry>>("v2/public/signatures?system_name=thera");
    }

    public Task<IEnumerable<EveScoutSystemEntry>?> GetTurnurSystemsAsync()
    {
        return Execute<IEnumerable<EveScoutSystemEntry>>("v2/public/signatures?system_name=turnur");
    }
    
    private async Task<T?> Execute<T>(string uri)
    {
        HttpResponseMessage? response = await _httpClient.GetAsync(uri).ConfigureAwait(false);

        if (response != null && response.StatusCode != HttpStatusCode.NoContent && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Accepted))
        {
            string result = response.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(result))
                return default(T);
            else
                return JsonSerializer.Deserialize<T>(result, _jsonSerializerOptions);
        }
        else
            return default(T);
            //throw new Exception($"Request failed with status code: {response.StatusCode}");
    }
}
