using System.Text.Json.Serialization;


namespace WHMapper.Shared.Models.DTO.EveAPI.Location
{
    public class EveLocation
    {
        [JsonPropertyName("solar_system_id")]
        public int SolarSystemId { get; private set; }

        [JsonPropertyName("station_id")]
        public int StationId { get; private set; }

        [JsonPropertyName("structure_id")]
        public long StructureId { get; private set; }

        [JsonConstructor]
        public EveLocation(int solarSystemId, int stationId, long structureId) =>
            (SolarSystemId, StationId, StructureId) = (solarSystemId, stationId, structureId);
    }
}
