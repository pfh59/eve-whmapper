using WHMapper.Models.Db;

namespace WHMapper.Models.DTO.MapAdmin;

public class MapAdmin
    {
        private  WHMap? map = null;
        public int Id => map?.Id ?? -1;
        public string Name => map?.Name ?? string.Empty;
        public IEnumerable<WHMapAccess>? WHMapAccesses => map?.WHMapAccesses;
        public bool ShowAccessDetails { get; set; } = false;

        public MapAdmin(WHMapper.Models.Db.WHMap map)
        {
            this.map = map;
            this.ShowAccessDetails = false;
        }
    }

