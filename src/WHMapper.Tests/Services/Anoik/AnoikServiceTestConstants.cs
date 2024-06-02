using System.Text.Json;

namespace WHMapper.Tests.Services.Anoik
{
    public static class AnoikServiceTestConstants
    {
        public static string ValidJson = @"
        {
            ""systems"": {
                ""J120450"": {
                    ""solarSystemID"": 31001554,
                    ""wormholeClass"": ""C4"",
                    ""effectName"": ""Red Giant"",
                    ""statics"": [""H900"", ""X877""]
                }
            },
            ""effects"": {
                ""Pulsar"": {
                    ""Shield Capacity"": [""+30%"", ""+44%"", ""+58%"", ""+72%"", ""+86%"", ""+100%""]
                }
            },
            ""wormholes"": {
                ""H900"": {
                    ""dest"": ""C5"",
                    ""src"": [""C4""]
                },
                ""X877"": {
                    ""dest"": ""C2"",
                    ""src"": [""C3"", ""C4""]
                }
            }
        }";

        public static JsonDocument GetJsonDocument()
        {
            return JsonDocument.Parse(ValidJson);
        }

        public static JsonElement GetElement(string name)
        {
            return GetJsonDocument().RootElement.GetProperty(name);
        }
    }
}
