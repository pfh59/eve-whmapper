namespace WHMapper.Models.DTO.RoutePlanner;

public class RouteSystemDetail
{
    public int SystemId { get; set; }
    public string SystemName { get; set; }
    public string Color { get; set; }

    public RouteSystemDetail(int systemId, string systemName, string color)
    {
        SystemId = systemId;
        SystemName = systemName;
        Color = color;
    }
}
