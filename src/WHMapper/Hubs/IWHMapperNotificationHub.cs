using WHMapper.Models.Db.Enums;

namespace WHMapper.Hubs
{
    public interface IWHMapperNotificationHub
    {
        Task NotifyUserConnected(string userName);
        Task NotifyUserDisconnected(string userName);
        Task NotifyUserPosition(string userName,string systemName);
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
    }
}

