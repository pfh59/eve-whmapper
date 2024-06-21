namespace WHMapper.Services.EveAPI.UserInterface
{
    public interface IUserInterfaceServices
	{
		Task<string?> SetWaypoint(int destination_id, bool add_to_beginning = false, bool clear_other_waypoints = false);
	}
}

