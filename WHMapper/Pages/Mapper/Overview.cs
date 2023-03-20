using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Pages.Mapper.CustomNode;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using Blazor.Diagrams.Algorithms;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;
using System;
using MudBlazor;
using Blazor.Diagrams.Core.Options;
using Blazor.Diagrams.Options;
using Blazor.Diagrams;
using WHMapper.Models.Db.Enums;
using System.Threading.Tasks;
using WHMapper.Repositories.WHSystemLinks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SvgPathProperties.Base;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using WHMapper.Services.EveJwkExtensions;
using WHMapper.Services.EveOnlineUserInfosProvider;
using static MudBlazor.CategoryTypes;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;
using System.Collections.Concurrent;
using Blazor.Diagrams.Components;
using System.Data;

namespace WHMapper.Pages.Mapper
{
    public partial class Overview : ComponentBase, IAsyncDisposable
    {
        protected BlazorDiagram Diagram { get; private set; } = null!;
        private MudMenu ClickRightMenu { get; set; } = null!;


        private EveLocation? _currentLocation = null;
        private EveSystemNodeModel? _currentSystemNode = null;
        private int _currentWHSystemId = 0;
        private EveSystemNodeModel? _selectedSystemNode = null;
        private EveSystemLinkModel? _selectedSystemLink = null;
        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

        private HubConnection? _hubConnection;


        [Inject]
        AuthenticationStateProvider AuthState { get; set; } = null!;

        [Inject]
        TokenProvider TokenProvider { get; set; } = null!;

        [Inject]
        IEveUserInfosServices UserInfos { get; set; } = null!;


        [Inject]
        IWHMapRepository? DbWHMaps { get; set; }

        [Inject]
        IWHSystemRepository? DbWHSystems { get; set; }

        [Inject]
        IWHSystemLinkRepository? DbWHSystemLinks { get; set; }

        [Inject]
        IEveAPIServices? EveServices { get; set; }

        [Inject]
        IAnoikServices? AnoikServices { get; set; }

        [Inject]
        public ISnackbar? Snackbar { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; } = null!;

        [Inject]
        public ILogger<EveAPIServices> Logger { get; set; } = null!;


        private string _userName = string.Empty;

        private IEnumerable<WHMap>? WHMaps { get; set; } = new List<WHMap>();
        private WHMap? _selectedWHMap = null;

        private int _selectedWHMapIndex = 0;
        private int SelectedWHMapIndex
        {
            get
            {
                return _selectedWHMapIndex;
            }
            set
            {
                if (_selectedWHMapIndex != value)
                {
                    _selectedWHMapIndex = value;
                    _selectedWHMap = WHMaps?.ElementAtOrDefault(value);
                }
            }
        }

        private bool _loading = true;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            _userName = await UserInfos.GetUserName();
            if (await InitDiagram())
            {
                if (await Restore())
                {
                    if (await InitNotificationHub())
                    {
                        HandleTimerAsync();
                    }

                    _loading = false;
                    StateHasChanged();
                }
                else
                {
                    Snackbar?.Add("Mapper restore error", Severity.Error);
                }
            }
            else
            {
                Snackbar?.Add("Mapper Initialization error", Severity.Error);
            }


            await base.OnInitializedAsync();
        }

        private async Task<bool> InitNotificationHub()
        {
            if (TokenProvider != null && !string.IsNullOrEmpty(TokenProvider.AccessToken) && _hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                     .WithUrl(Navigation.ToAbsoluteUri("/whmappernotificationhub"), options =>
                     {
                         options.AccessTokenProvider = () => Task.FromResult(TokenProvider.AccessToken);
                     }).Build();

                _hubConnection.On<string>("NotifyUserConnected", (user) =>
                {
                    Snackbar?.Add($"{user} are connected", Severity.Info);
                });
                _hubConnection.On<string>("NotifyUserDisconnected", async (user) =>
                {
                    EveSystemNodeModel userSystem = (EveSystemNodeModel)Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).ConnectedUsers.Contains(user));
                    if (userSystem != null)
                    {
                        await userSystem.RemoveConnectedUser(user);
                        userSystem.Refresh();
                    }
                    Snackbar?.Add($"{user} are disconnected", Severity.Info);
                });
                _hubConnection.On<string, string>("NotifyUserPosition", async (user, systemName) =>
                {
                    EveSystemNodeModel previousSytem = (EveSystemNodeModel)Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).ConnectedUsers.Contains(user));
                    if (previousSytem != null)
                    {
                        await previousSytem.RemoveConnectedUser(user);
                        previousSytem.Refresh();
                    }
                    

                    EveSystemNodeModel systemToAddUser = (EveSystemNodeModel)Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).Title == systemName);
                    if (systemToAddUser != null)
                    {
                        await systemToAddUser.AddConnectedUser(user);
                        systemToAddUser.Refresh();
                    }
                });
                _hubConnection.On<IDictionary<string,string>>("NotifyUsersPosition", async (usersPosition) =>
                {

                    await Parallel.ForEachAsync(usersPosition, async (item, cancellationToken) =>
                    {
                        EveSystemNodeModel systemToAddUser = (EveSystemNodeModel)Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).Title == item.Value);
                        await systemToAddUser.AddConnectedUser(item.Key);
                        systemToAddUser.Refresh();
                    });
                });
                _hubConnection.On<string, int,int>("NotifyWormoleAdded", async (user, mapId,wormholeId) =>
                {
                    if (wormholeId > 0 && mapId == _selectedWHMap?.Id)
                    {
                        var newWHSystem = await DbWHSystems?.GetById(wormholeId);
                        var newSystemNode = await DefineEveSystemNodeModel(newWHSystem);
                        Diagram?.Nodes?.Add(newSystemNode);

                    }

                });
                _hubConnection.On<string, int,int>("NotifyWormholeRemoved", async (user, mapId,wormholeId) =>
                {
                    if (wormholeId > 0 && mapId == _selectedWHMap?.Id)
                    {
                        var sustemNodeToDelete=Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                        if (sustemNodeToDelete != null)
                        {
                            Diagram?.Nodes?.Remove(sustemNodeToDelete);

                            if (((EveSystemNodeModel)sustemNodeToDelete).IdWH == _currentWHSystemId)
                            {
                                _currentLocation = null;
                                _currentSystemNode = null;
                                _currentWHSystemId = -1;
                                if (_selectedSystemNode?.IdWH == _currentWHSystemId)
                                {
                                    _selectedSystemNode = null;
                                    StateHasChanged();
                                }
                            }
                        }
                    }
                    
                });
                _hubConnection.On<string, int, int>("NotifyLinkAdded", async (user, mapId, linKId) =>
                {
                    if (linKId > 0 && mapId == _selectedWHMap?.Id)
                    {

                        var link = await DbWHSystemLinks?.GetById(linKId);


                        var whFrom = await DbWHSystems.GetById(link.IdWHSystemFrom);
                        var whTo = await DbWHSystems.GetById(link.IdWHSystemTo);
                        if (whFrom != null && whTo != null)
                        {
                            EveSystemNodeModel newSystemNodeFrom = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whFrom.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;
                            EveSystemNodeModel newSystemNodeTo = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whTo.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;
                            Diagram?.Links?.Add(new EveSystemLinkModel(link, newSystemNodeFrom, newSystemNodeTo));
                        }
                    }
                });
                _hubConnection.On<string, int, int>("NotifyLinkRemoved", async (user, mapId, linKId) =>
                {
                    if (linKId > 0 && mapId == _selectedWHMap?.Id)
                    {
                        var linkToDel = Diagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linKId);
                        if (linkToDel != null)
                            Diagram?.Links?.Remove(linkToDel);

                    }
                });
                _hubConnection.On<string, int, int,double,double>("NotifyWormoleMoved", async (user, mapId, wormholeId,posX,posY) =>
                {
                    if (wormholeId > 0 && mapId == _selectedWHMap?.Id)
                    {
                        var whToMoved = Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                        if(whToMoved!=null)
                        {
                            whToMoved.SetPosition(posX, posY);
                        }
                    }
                });
                _hubConnection.On<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus>("NotifyLinkChanged", async (user, mapId, linkId, eol, size, mass) =>
                {
                    if (linkId > 0 && mapId == _selectedWHMap?.Id)
                    {
                        var linkToChanged = Diagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linkId);
                        if (linkToChanged != null)
                        {
                            ((EveSystemLinkModel)linkToChanged).IsEoL = eol;
                            ((EveSystemLinkModel)linkToChanged).Size = size;
                            ((EveSystemLinkModel)linkToChanged).MassStatus = mass;
                            ((EveSystemLinkModel)linkToChanged).Refresh();
                        }
                    }
                });


                await _hubConnection.StartAsync();

                return true;
            }
            return false;

        }

        private async Task NotifyUserPosition(string systemName)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendUserPosition", systemName);
            }
        }

        private async Task NotifyWormoleAdded(int mapId,int wormholeId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeAdded", mapId,wormholeId);
            }
        }

        private async Task NotifyWormholeRemoved(int mapId, int wormholeId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeRemoved", mapId,wormholeId);
            }
        }

        private async Task NotifyLinkAdded(int mapId,int linkId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendLinkAdded", mapId, linkId);
            }
        }

        private async Task NotifyLinkRemoved(int mapId, int linkId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendLinkRemoved", mapId, linkId);
            }
        }

        private async Task NotifyWormholeMoved(int mapId, int wormholeId,double posX,double posY)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeMoved", mapId, wormholeId, posX, posY);
            }
        }

        private async Task NotifyLinkChanged(int mapId,int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendLinkChanged", mapId, linkId, eol, size, mass);
            }
        }

        private async Task<bool> InitDiagram()
        {
            try
            {
                Logger.LogInformation("Start Init Diagram");
                var options = new BlazorDiagramOptions
                {
                    AllowMultiSelection = true, // Whether to allow multi selection using CTRL
                    /*Links = new DiagramLinkOptions
                    {
                        DefaultColor = "grey",
                        DefaultSelectedColor = "white"
                    },*/

                    /*Zoom = new DiagramZoomOptions
                    {
                        Minimum = 0.25, // Minimum zoom value
                        Inverse = false, // Whether to inverse the direction of the zoom when using the wheel
                    },*/

                };


                Diagram = new BlazorDiagram(options);
                Diagram.SelectionChanged += async (item) =>
                {
                    if (item.GetType() == typeof(EveSystemNodeModel))
                    {

                        if (((EveSystemNodeModel)item).Selected)
                            _selectedSystemNode = (EveSystemNodeModel)item;
                        else
                            _selectedSystemNode = null;

                        _selectedSystemLink = null;
                        StateHasChanged();
                            
                    }

                    if (item.GetType() == typeof(EveSystemLinkModel))
                    {

                        if (((EveSystemLinkModel)item).Selected)
                            _selectedSystemLink = (EveSystemLinkModel)item;
                        else
                            _selectedSystemLink = null;

                        _selectedSystemNode = null;
                        StateHasChanged();
                            
                    }
                };

                Diagram.KeyDown += async (kbevent) =>
                {
                    if (kbevent.Code=="Delete")
                    {
                        if(_selectedSystemNode!=null)
                        {
                            try
                            {
                                if (await DbWHMaps?.RemoveWHSystem(_selectedWHMap.Id, _selectedSystemNode.IdWH) != null)
                                {
                                    await NotifyWormholeRemoved(_selectedWHMap.Id, _selectedSystemNode.IdWH);
                                    if (_selectedSystemNode.IdWH == _currentWHSystemId)
                                    {
                                        _currentLocation = null;
                                        _currentSystemNode = null;
                                        _currentWHSystemId = -1;
                                    }
                                    _selectedSystemNode = null;
                                    StateHasChanged();
                                }
                                else
                                    Snackbar?.Add("Remove wormhole node db error", Severity.Error);
                            }
                            catch(Exception ex)
                            {
                                
                            }
                            return;
                        }

                        if(_selectedSystemLink!=null)
                        {

                            try
                            {                        
                                if (await DbWHMaps?.RemoveWHSystemLink(_selectedWHMap.Id, ((EveSystemNodeModel)_selectedSystemLink.Source.Model).IdWH, ((EveSystemNodeModel)_selectedSystemLink.Target.Model).IdWH) != null)
                                {
                                   await NotifyLinkRemoved(_selectedWHMap.Id, _selectedSystemLink.Id);

                                    _selectedSystemLink = null;
                                    StateHasChanged();

                                }
                                else
                                    Snackbar?.Add("Remove wormhole link db error", Severity.Error);
                            }
                            catch(Exception ex)
                            {
                               
                            }
                            return;
                        }
                    }


                };

                Diagram.PointerUp += async (item, pointerEvent) =>
                {
                    if (item != null)
                    {
                        if (item.GetType() == typeof(EveSystemNodeModel))
                        {
                            try
                            {

                                var wh = await DbWHSystems?.GetById(((EveSystemNodeModel)item).IdWH);
                                if (wh != null)
                                {
                                    if (wh.PosX != ((EveSystemNodeModel)item).Position.X || wh.PosY != ((EveSystemNodeModel)item).Position.Y)
                                    {
                                        wh.PosX = ((EveSystemNodeModel)item).Position.X;
                                        wh.PosY = ((EveSystemNodeModel)item).Position.Y;

                                        if (await DbWHSystems?.Update(((EveSystemNodeModel)item).IdWH, wh) == null)
                                        {
                                            Snackbar?.Add("Update wormhole node position db error", Severity.Error);
                                        }
                                        await NotifyWormholeMoved(_selectedWHMap.Id, wh.Id, wh.PosX, wh.PosY);
                                    }
                                }
                                else
                                {
                                    Snackbar?.Add("Unable to find moved wormhole node dd error", Severity.Error);
                                }

                            }
                            catch(Exception ex)
                            {

                            }
                           
                        }
                    }
                };

                Diagram.Options.Zoom.Enabled = false;
                Diagram.RegisterComponent<EveSystemNodeModel, EveSystemNode>();
                Diagram.RegisterComponent<EveSystemLinkModel, EveSystemLink>();

                return true;
            }            
            catch(Exception ex)
            {
                Logger.LogError(ex, "Init Diagram Error");
                return false;
            }
        }

        private async Task<bool> Restore()
        {
            try
            {

                Logger.LogInformation("Beginning Restore Mapper");
                WHMaps = await DbWHMaps?.GetAll();
                if (WHMaps == null || WHMaps.Count() == 0)
                {
                    _selectedWHMap = await DbWHMaps.Create(new WHMap("Default Maps"));
                    if (_selectedWHMap != null)
                        WHMaps = await DbWHMaps?.GetAll();

                }
                _selectedWHMap = WHMaps.FirstOrDefault();

                if (_selectedWHMap.WHSystems.Count > 0)
                {
                    foreach (WHSystem dbWHSys in _selectedWHMap.WHSystems)
                    {
                        EveSystemNodeModel whSysNode = await DefineEveSystemNodeModel(dbWHSys);
                        Diagram.Nodes.Add(whSysNode);

                    }

                }

                if (_selectedWHMap.WHSystemLinks.Count > 0)
                {
                    foreach (WHSystemLink dbWHSysLink in _selectedWHMap.WHSystemLinks)
                    {
                        var whFrom = await DbWHSystems.GetById(dbWHSysLink.IdWHSystemFrom);
                        var whTo = await DbWHSystems.GetById(dbWHSysLink.IdWHSystemTo);
                        if (whFrom != null && whTo != null)
                        { 
                            EveSystemNodeModel newSystemNodeFrom = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whFrom.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;
                            EveSystemNodeModel newSystemNodeTo = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whTo.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;

                            Diagram.Links.Add(new EveSystemLinkModel(dbWHSysLink, newSystemNodeFrom, newSystemNodeTo));
                        }
                        else
                        {
                            Logger.LogWarning("Bad Link,Auto remove");

                            await DbWHSystemLinks.DeleteById(dbWHSysLink.Id);
                        }
                    }
                }
                Logger.LogInformation("Restore Mapper Success");
                StateHasChanged();
                return true;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Mapper Restore");
                return false;
            }
        }

        private async Task HandleTimerAsync()   
        {
            _currentLocation = null;
            _currentSystemNode = null;
            _selectedSystemNode = null;


            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));

            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    var state = await AuthState.GetAuthenticationStateAsync();

                    if (!String.IsNullOrEmpty(state?.User?.Identity?.Name))
                        await GetCharacterPositionInSpace();
                    else
                        _cts.Cancel();//todo redirect to logout
                }
            }
            catch (Exception ex)
            {
                
                //Handle the exception but don't propagate it
            }
        }

        private async Task<EveSystemNodeModel> DefineEveSystemNodeModel(WHSystem wh)
        {

            EveSystemNodeModel res = null;
            if (wh == null)
                throw new ArgumentNullException();

            if (wh.SecurityStatus <= -0.9)
            {

                string whClass = await AnoikServices.GetSystemClass(wh.Name);
                string whEffect = await AnoikServices.GetSystemEffects(wh.Name);
                IEnumerable<KeyValuePair<string, string>> whStatics = await AnoikServices.GetSystemStatics(wh.Name);
                IEnumerable<KeyValuePair<string, string>> whEffectsInfos = null;
                if (!String.IsNullOrWhiteSpace(whEffect))
                {
                    whEffectsInfos = await AnoikServices.GetSystemEffectsInfos(whEffect, whClass);
                }

                res = new EveSystemNodeModel(wh, whClass, whEffect, whEffectsInfos, whStatics);

            }
            else
            {
                res = new EveSystemNodeModel(wh);
            }

            res.SetPosition(wh.PosX, wh.PosY);
            return res;
        }

        private async Task<bool> CreateLink(WHMap map, EveSystemNodeModel src, EveSystemNodeModel target)
        {
            var link = await DbWHMaps?.AddWHSystemLink(map.Id, src.IdWH, target.IdWH);

            if (link != null)
            {               
                Diagram.Links.Add(new EveSystemLinkModel(link, src, target));
                await this.NotifyLinkAdded(map.Id, link.Id);

                return true;
            }
            return false;
        }

        private async Task GetCharacterPositionInSpace()
        {
            EveSystemNodeModel? previousSystem = null;
            WHSystem? newWHSystem = null;
            double defaultNewSystemPosX = 0;
            double defaultNewSystemPosY = 0;
            try
            {
                EveLocation el = await EveServices.LocationServices.GetLocation();
                
                if (el != null && (_currentLocation == null || _currentLocation.SolarSystemId != el.SolarSystemId) )
                {
                    previousSystem = _currentSystemNode;
                    _currentLocation = el;
                    
                    var newSystem = await EveServices.UniverseServices.GetSystem(el.SolarSystemId);

                    

                    if (Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, newSystem.Name, StringComparison.OrdinalIgnoreCase)) == null)
                    {

                        //determine position on map. depends of previous system
                        if (previousSystem != null)
                        {
                            defaultNewSystemPosX  = (double)previousSystem.Position.X + previousSystem.Size.Width + 10;
                            defaultNewSystemPosY = (double)previousSystem.Position.Y + previousSystem.Size.Height + 10;
                        }

                        //determine if source have same system link and get next unique ident
                        if (newSystem.SecurityStatus <= -0.9 && previousSystem != null)
                        {
                            string whClass = await AnoikServices.GetSystemClass(newSystem.Name);
                            int nbSameWHClassLink = Diagram.Links.Where(x => ((EveSystemNodeModel)x.Target.Model).Class.Contains(whClass) && ((EveSystemNodeModel)x.Source.Model).IdWH == previousSystem.IdWH).Count();
                            if (nbSameWHClassLink > 0)
                            {
                                char extension = (Char)(Convert.ToUInt16('A') + nbSameWHClassLink);
                                newWHSystem = await DbWHMaps?.AddWHSystem(_selectedWHMap.Id, new WHSystem(newSystem.Name, extension, newSystem.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                            }
                            else
                                newWHSystem = await DbWHMaps?.AddWHSystem(_selectedWHMap.Id, new WHSystem(newSystem.Name, newSystem.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                        }
                        else
                            newWHSystem = await DbWHMaps?.AddWHSystem(_selectedWHMap.Id, new WHSystem(newSystem.Name, newSystem.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                      
                        if(newWHSystem!=null)
                        {
                            var newSystemNode = await DefineEveSystemNodeModel(newWHSystem);
                            await newSystemNode.AddConnectedUser(_userName);

                            Diagram.Nodes.Add(newSystemNode);
                            await this.NotifyWormoleAdded(_selectedWHMap.Id, newWHSystem.Id);
                            await this.NotifyUserPosition(newSystem.Name);

                            if (previousSystem != null)
                            {
                                if(!await CreateLink(_selectedWHMap, previousSystem, newSystemNode))
                                {
                                    Snackbar?.Add("Add Wormhole Link db error", Severity.Error);
                                }

                                //remove ConnectedUser on previous system
                                await previousSystem.RemoveConnectedUser(_userName);
                                previousSystem.Refresh();
                            }

                            _currentSystemNode = newSystemNode;
                            _currentWHSystemId = newWHSystem.Id;
                            _selectedSystemNode = _currentSystemNode;
                        }
                        else
                        {
                            Snackbar?.Add("Add Wormhole db error", Severity.Error);
                        }
                    }
                    else
                    {
                        await this.NotifyUserPosition(newSystem.Name);
                        if (previousSystem != null)
                        {
                            await previousSystem.RemoveConnectedUser(_userName);
                            previousSystem.Refresh();
                        }
                            
                        _currentSystemNode = (EveSystemNodeModel)Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, newSystem.Name, StringComparison.OrdinalIgnoreCase));
                        await _currentSystemNode.AddConnectedUser(_userName);
                        _currentSystemNode.Refresh();

                        _currentWHSystemId = (await DbWHSystems.GetByName(newSystem.Name)).Id;
                        _selectedSystemNode = _currentSystemNode;

                        if (previousSystem != null)
                        {
                            //check if link exist, if not create it
                            var whLink = Diagram.Links.FirstOrDefault(x =>
                            ((((EveSystemNodeModel)x.Source.Model).IdWH == previousSystem.IdWH) && (((EveSystemNodeModel)x.Target.Model).IdWH == _currentSystemNode.IdWH))
                            ||
                            ((((EveSystemNodeModel)x.Source.Model).IdWH == _currentSystemNode.IdWH) && (((EveSystemNodeModel)x.Target.Model).IdWH == previousSystem.IdWH))
                        );

                            if (whLink == null)
                            {
                                if (!await CreateLink(_selectedWHMap, previousSystem, _currentSystemNode))
                                {
                                    Snackbar?.Add("Add Wormhole Link db error", Severity.Error);
                                }
                            }
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
      
            }
        }


        public void Cancel()
        {
            _cts?.Cancel();
        }

        public async ValueTask DisposeAsync()
        {
            Cancel();
            _timer?.Dispose();

            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        #region Menu Actions
        public async Task<bool> ToggleSlectedSystemLinkEOL()
        {
            try
            {
                if (_selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks?.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.IsEndOfLifeConnection = !link.IsEndOfLifeConnection;
                        link = await DbWHSystemLinks?.Update(_selectedSystemLink.Id, link);
                        _selectedSystemLink.IsEoL = link.IsEndOfLifeConnection;
                        _selectedSystemLink.Refresh();
                        await NotifyLinkChanged(_selectedWHMap.Id, link.Id, link.IsEndOfLifeConnection, link.Size,link.MassStatus);
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("Toggle system link eol error", ex);
                return false;
            }
        }

        public async Task<bool> SetSelectedSystemLinkStatus(SystemLinkMassStatus massStatus)
        {
            try
            {
                if (_selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks?.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.MassStatus = massStatus;
                        link = await DbWHSystemLinks?.Update(_selectedSystemLink.Id, link);
                        _selectedSystemLink.MassStatus = link.MassStatus;
                        ClickRightMenu.CloseMenu();
                        _selectedSystemLink.Refresh();
                        await NotifyLinkChanged(_selectedWHMap.Id, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("System link status error", ex);
                return false;
            }
        }

        public async Task<bool> SetSelectedSystemLinkSize(SystemLinkSize size)
        {
            try
            {
                if (_selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks?.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.Size = size;
                        link = await DbWHSystemLinks?.Update(_selectedSystemLink.Id, link);
                        _selectedSystemLink.Size = link.Size;
                        ClickRightMenu.CloseMenu();
                        _selectedSystemLink.Refresh();
                        await NotifyLinkChanged(_selectedWHMap.Id, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);

                        return true;

                    }
                    else
                    {
                        return false;
                    }

                    
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("System link size error", ex);
                return false;
            }
        }

        #endregion
    }
}


