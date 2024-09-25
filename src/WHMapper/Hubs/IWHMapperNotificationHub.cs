using WHMapper.Models.Db.Enums;

namespace WHMapper.Hubs
{
    public interface IWHMapperNotificationHub
    {
        Task NotifyUserConnected(string userName);
        Task NotifyUserDisconnected(string userName);
        Task NotifyUserPosition(string userName,int mapId,int wormholeId);
        Task NotifyWormoleAdded(string userName, int mapId,int wormholeId);
        Task NotifyWormholeRemoved(string userName, int mapId,int wormholeId);
        Task NotifyLinkAdded(string userName, int mapId, int linkId);
		Task NotifyLinkRemoved(string userName, int mapId, int linkId);
        Task NotifyWormoleMoved(string userName, int mapId, int wormholeId,double posx,double posy);
        Task NotifyLinkChanged(string username,int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass);
        Task NotifyWormholeNameExtensionChanged(string username, int mapId, int wormholeId,bool increment);
        Task NotifyWormholeSignaturesChanged(string username, int mapId, int wormholeId);
        Task NotifyWormholeLockChanged(string username, int mapId, int wormholeId, bool locked);
        Task NotifyWormholeSystemStatusChanged(string username, int mapId, int wormholeId, WHSystemStatus systemStatus);
        Task NotifyMapAdded(string userName, int mapId);
        Task NotifyMapRemoved(string userName, int mapId);
        Task NotifyMapNameChanged(string userName, int mapId, string newName);
        Task NotifyAllMapsRemoved(string userName);
        Task NotifyMapAccessesAdded(string userName, int mapId, IEnumerable<int> accessId);
        Task NotifyMapAccessRemoved(string userName, int mapId, int accessId);
        Task NotifyMapAllAccessesRemoved(string userName, int mapId); 
        Task NotifyUserOnMapConnected(string userName, int mapId); 
        Task NotifyUserOnMapDisconnected(string userName, int mapId); 
    }
}

