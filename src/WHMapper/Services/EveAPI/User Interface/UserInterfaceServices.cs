using WHMapper.Models.DTO;

namespace WHMapper.Services.EveAPI.UserInterface
{
    public class UserInterfaceServices : EveApiServiceBase, IUserInterfaceServices
    {
        public UserInterfaceServices(HttpClient httpClient, UserToken? userToken = null)
            : base(httpClient, userToken)
        {
        }


        public async Task<string?> SetWaypoint(int destination_id, bool add_to_beginning = false, bool clear_other_waypoints = false)
        {
            return await base.Execute<string>(RequestSecurity.Authenticated, RequestMethod.Post, string.Format("v2/ui/autopilot/waypoint?datasource=tranquility&destination_id={0}&add_to_beginning={1}&clear_other_waypoints={2}", destination_id, add_to_beginning, clear_other_waypoints));
        }
    }
}