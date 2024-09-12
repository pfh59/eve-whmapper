using System.ComponentModel.DataAnnotations;

namespace WHMapper;

public class WHRoute
{
        [Key]
        public int Id { get; set; }

        
        public int? EveEntityId { get; set; }

        [Required]
        public int MapId { get; set; }

        [Required]
        public int SolarSystemId { get; set; }

        [Obsolete("EF Requires it")]
        protected WHRoute() { }

        public WHRoute(int mapId,int solarSystemId)
        {
            MapId = mapId;
            SolarSystemId = solarSystemId;
        }

        public WHRoute(int mapid,int solarSystemId,int eveEntityId)
        {
            MapId = mapid;
            EveEntityId = eveEntityId;
            SolarSystemId = solarSystemId;
        }
}
