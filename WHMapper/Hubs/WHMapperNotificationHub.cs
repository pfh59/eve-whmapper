using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WHMapper.Models.Db.Enums;
using WHMapper.Services.EveJwkExtensions;
using WHMapper.Services.EveOnlineUserInfosProvider;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Hubs
{
    [Authorize(AuthenticationSchemes = EveOnlineJwkDefaults.AuthenticationScheme)]
    public class WHMapperNotificationHub : Hub<IWHMapperNotificationHub>
    {
        private const string UNDEFINE_POSITION = "Undefine Position";
        private static ConcurrentDictionary<string, string> _connectedUserPosition = new ConcurrentDictionary<string, string>();


        private string CurrentUser()
        {
            if (Context != null && Context.User != null)
            {
                var nameRes = Context.User.FindFirst("name");
                if (nameRes != null)
                    return nameRes.Value;
            }
            return string.Empty;
        }

        public override async Task OnConnectedAsync()
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName) && !_connectedUserPosition.ContainsKey(userName))
            {
                while (!_connectedUserPosition.TryAdd(userName, UNDEFINE_POSITION))
                    await Task.Delay(1);
            }
            else//add log
            {

            }

            await Clients.AllExcept(Context.ConnectionId).NotifyUserConnected(userName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string res = string.Empty;
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName) && _connectedUserPosition.ContainsKey(userName))
            {
                while (!_connectedUserPosition.TryRemove(userName, out res))
                    await Task.Delay(1);
            }
            else//add log
            {

            }
            await Clients.AllExcept(Context.ConnectionId).NotifyUserDisconnected(userName);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendUserPosition(string systemName)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName) && _connectedUserPosition.ContainsKey(userName))
            {
                string res = string.Empty;
                while (!_connectedUserPosition.TryGetValue(userName, out res))
                    await Task.Delay(1);

                while (!_connectedUserPosition.TryUpdate(userName, systemName, res))
                    await Task.Delay(1);
            }
            else//add log
            {

            }

            await Clients.AllExcept(Context.ConnectionId).NotifyUserPosition(userName, systemName);
            await Clients.Caller.NotifyUsersPosition(_connectedUserPosition);
        }

        public async Task SendWormholeAdded(int mapId, int wowrmholeId)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyWormoleAdded(userName, mapId, wowrmholeId);
            }
            
        }

        public async Task SendWormholeRemoved(int mapId, int wowrmholeId)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyWormholeRemoved(userName, mapId, wowrmholeId);
            }
        }

        public async Task SendLinkAdded(int mapId, int linkId)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyLinkAdded(userName, mapId, linkId);
            }
        }

        public async Task SendLinkRemoved(int mapId, int linkId)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyLinkRemoved(userName, mapId, linkId);
            }
        }

        public async Task SendWormholeMoved(int mapId, int wowrmholeId, double posX, double posY)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyWormoleMoved(userName, mapId, wowrmholeId, posX, posY);
            }
        }


        public async Task SendLinkChanged(int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyLinkChanged(userName, mapId, linkId, eol, size, mass);
            }
        }

        public async Task SendWormholeNameExtensionChanged(int mapId, int wowrmholeId,bool increment)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyWormholeNameExtensionChanged(userName, mapId, wowrmholeId,increment);
            }

        }

        public async Task SendWormholeSignaturesChanged(int mapId, int wowrmholeId)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyWormholeSignaturesChanged(userName, mapId, wowrmholeId);
            }
        }

        public async Task SendWormholeLockChanged(int mapId, int wormholeId, bool locked)
        {
            string userName = CurrentUser();
            if (!string.IsNullOrEmpty(userName))
            {
                await Clients.AllExcept(Context.ConnectionId).NotifyWormholeLockChanged(userName, mapId, wormholeId, locked);
            }
        }
    
    }
}

