using System.ComponentModel.DataAnnotations;

namespace WHMapper.Shared.Models.Db;

public class WHRoute
{
    [Key]
    public int Id { get; set; }


    public int? EveEntityId { get; set; }

    [Required]
    public int SolarSystemId { get; set; }

    [Obsolete("EF Requires it")]
    protected WHRoute() { }

    public WHRoute(int solarSystemId)
    {
        SolarSystemId = solarSystemId;
    }

    public WHRoute(int solarSystemId, int eveEntityId)
    {
        EveEntityId = eveEntityId;
        SolarSystemId = solarSystemId;
    }
}
