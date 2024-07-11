using System.Text.Json.Serialization;

namespace WHMapper.Models.DTO.EveAPI.Universe
{
    public class Planet
    {
        [JsonPropertyName("planet_id")]
        public int PlanetId { get; private set; }

        [JsonPropertyName("name")]
        public string Name { get; private set; }

        [JsonPropertyName("type_id")]
        public int TypeId { get; private set; }

        [JsonPropertyName("position")]
        public Position Position { get; private set; }
 
        [JsonPropertyName("system_id")]
        public int SystemId { get; private set; }


        [JsonPropertyName("asteroid_belts")]
        public int[] AsteroidBelts { get; private set; }

        [JsonPropertyName("moons")]
        public int[] Moons { get; private set; }

        [JsonConstructor]
        public Planet(int planetId,string name, int typeId, Position pos,int systemId,int[] asteroidBelts,int[] moons) =>
            (PlanetId,Name, TypeId, Position, SystemId, AsteroidBelts, Moons) = (planetId, name, typeId, pos, systemId, asteroidBelts, moons);
    }
}
