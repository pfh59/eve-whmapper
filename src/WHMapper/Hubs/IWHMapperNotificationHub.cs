using WHMapper.Models.Db.Enums;

namespace WHMapper.Hubs
{
    public interface IWHMapperNotificationHub
    {
        Task NotifyUserConnected(int accountID);
        Task NotifyUserDisconnected(int accountID);
        Task NotifyUserPosition(int accountID,int mapId,int wormholeId);
        Task NotifyWormoleAdded(int accountID, int mapId,int wormholeId);
        Task NotifyWormholeRemoved(int accountID, int mapId,int wormholeId);
        Task NotifyLinkAdded(int accountID, int mapId, int linkId);
		Task NotifyLinkRemoved(int accountID, int mapId, int linkId);
        Task NotifyWormoleMoved(int accountID, int mapId, int wormholeId,double posx,double posy);
        Task NotifyLinkChanged(int accountID,int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass);
        Task NotifyWormholeNameExtensionChanged(int accountID, int mapId, int wormholeId,bool increment);
        Task NotifyWormholeSignaturesChanged(int accountID, int mapId, int wormholeId);
        Task NotifyWormholeLockChanged(int accountID, int mapId, int wormholeId, bool locked);
        Task NotifyWormholeSystemStatusChanged(int accountID, int mapId, int wormholeId, WHSystemStatus systemStatus);
        Task NotifyMapAdded(int accountID, int mapId);
        Task NotifyMapRemoved(int accountID, int mapId);
        Task NotifyMapNameChanged(int accountID, int mapId, string newName);
        Task NotifyAllMapsRemoved(int accountID);
        Task NotifyMapAccessesAdded(int accountID, int mapId, IEnumerable<int> accessId);
        Task NotifyMapAccessRemoved(int accountID, int mapId, int accessId);
        Task NotifyMapAllAccessesRemoved(int accountID, int mapId); 
        Task NotifyUserOnMapConnected(int accountID, int mapId); 
        Task NotifyUserOnMapDisconnected(int accountID, int mapId); 
    }
}

