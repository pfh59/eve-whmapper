namespace WHMapper.Shared.Models.DTO.RoutePlanner;

public class RouteConnection
{
    public int FromSolarSystemId { get; set; }
    public float FromSecurity { get; set; }

    public int ToSolarSystemId { get; set; }
    public float ToSecurity { get; set; }

    public RouteConnection()
    {

    }

    public RouteConnection(int fromSolarSystemId, float fromSecurity, int toSolarSystemId, float toSecurity)
    {
        FromSolarSystemId = fromSolarSystemId;
        FromSecurity = fromSecurity;
        ToSolarSystemId = toSolarSystemId;
        ToSecurity = toSecurity;
    }
}
