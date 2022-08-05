using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI
{
    public class Position
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("z")]
        public double Z { get; set; }

        [JsonConstructor]
        public Position(double x, double y, double z) => (X, Y, Z) = (x, y, z);
    }
    
}
