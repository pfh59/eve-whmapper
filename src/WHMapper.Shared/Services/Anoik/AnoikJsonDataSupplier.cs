using System.Text.Json;

namespace WHMapper.Shared.Services.Anoik
{
    public class AnoikJsonDataSupplier : IAnoikDataSupplier
    {
        private readonly JsonDocument _json;

        public AnoikJsonDataSupplier(string jsonFilePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jsonFilePath);

            try
            {
                string jsonText = File.ReadAllText(jsonFilePath);
                _json = JsonDocument.Parse(jsonText);
            }
            catch (FileNotFoundException ex)
            {
                throw new ArgumentException("The specified JSON file was not found.", nameof(jsonFilePath), ex);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("The JSON file is invalid or malformed.", nameof(jsonFilePath), ex);
            }
        }

        public JsonElement GetSystems()
        {
            return _json.RootElement.GetProperty("systems");
        }

        public JsonElement GetEffects()
        {
            return _json.RootElement.GetProperty("effects");
        }

        public JsonElement GetWormHoles()
        {
            return _json.RootElement.GetProperty("wormholes");
        }
    }
}