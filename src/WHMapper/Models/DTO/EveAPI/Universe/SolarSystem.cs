using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
    public class SolarSystem
    {
        
        [JsonPropertyName("star_id")]
        public int StarId { get; private set; }

        [JsonPropertyName("system_id")]
        public int SystemId { get; private set; }

        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonPropertyName("position")]
        public Position Position { get; private set; }

        [JsonPropertyName("security_status")]
        public float SecurityStatus { get; private set; }

        [JsonPropertyName("security_class")]
        public string SecurityClass { get; private set; }

        [JsonPropertyName("constellation_id")]
        public int ConstellationId { get; private set; }

        
        //[JsonPropertyName("planets")]
        //public ICollection<Planet> Planets { get; private set; } = new List<Planet>();

        
        [JsonPropertyName("stargates")]
        public int[] Stargates { get; private set; }

        
        [JsonPropertyName("stations")]
        public int[] Stations { get; private set; }

        public SolarSystem(int starId, int systemId, string name, Position position, float securityStatus, string securityClass, int constellationId/*, ICollection<Planet> planets*/ ,int[] stargates, int[] stations)
            => (StarId, SystemId, Name, Position, SecurityStatus, SecurityClass, ConstellationId/*, Planets*/,Stargates, Stations) = (starId, systemId, name, position, securityStatus, securityClass, constellationId/*, planets*/,stargates, stations);

    }
}
