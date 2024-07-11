namespace WHMapper.Shared.Models.DTO.EveMapper;

public class EveRoute
{
    public int Id { get; private set; }
    public string DestinationName { get; private set; }
    public int[]? Route { get; private set; }

    public int RouteLength
    {
        get
        {
            if (Route == null)
                return 0;
            else
            {
                return Route.Length;
            }
        }
    }

    public bool IsAvailable
    {
        get
        {
            return Route != null && Route.Length > 0;
        }
    }

    public bool IsShowed { get; set; } = false;

    public EveRoute(int id, string destinationName, int[]? route)
    {
        Id = id;
        DestinationName = destinationName;
        Route = route;
    }

}
