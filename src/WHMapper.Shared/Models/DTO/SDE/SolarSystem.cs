namespace WHMapper.Shared.Models.DTO.SDE;

public class SolarSystem
{
    public int SolarSystemId { get; set; }
    public float Security { get; set; }

    public SolarSystem()
    {

    }

    public SolarSystem(int solarSystemId, float security)
    {
        SolarSystemId = solarSystemId;
        Security = security;
    }
}
