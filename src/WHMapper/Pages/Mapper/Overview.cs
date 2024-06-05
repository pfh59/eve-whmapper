

using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Pages.Mapper.CustomNode;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;
using MudBlazor;
using Blazor.Diagrams;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHSystemLinks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Models.DTO;
using WHMapper.Services.EveOnlineUserInfosProvider;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;
using System.Data;
using Microsoft.AspNetCore.Components.Web;
using WHMapper.Services.WHSignature;
using WHMapper.Services.EveMapper;
using Blazor.Diagrams.Core.Behaviors;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Repositories.WHJumpLogs;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;
using BlazorContextMenu;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.WHColor;
using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Pages.Mapper
{


    [Authorize(Policy = "Access")]
    public partial class Overview : ComponentBase, IAsyncDisposable
    {
        protected BlazorDiagram _blazorDiagram  = null!;
        private WHMapper.Pages.Mapper.Signatures.Overview WHSignaturesView { get; set; } = null!;
        
        private EveSystemNodeModel? _selectedSystemNode = null;
        private EveSystemLinkModel? _selectedSystemLink = null;
        
        private ICollection<EveSystemNodeModel>? _selectedSystemNodes = null;
        private ICollection<EveSystemLinkModel>? _selectedSystemLinks = null;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private HubConnection _hubConnection=null!;


        [Inject]
        AuthenticationStateProvider AuthState { get; set; } = null!;

        [Inject]
        TokenProvider TokenProvider { get; set; } = null!;

        [Inject]
        IEveUserInfosServices UserInfos { get; set; } = null!;

        [Inject]
        IWHMapRepository DbWHMaps { get; set; } = null!;

        [Inject]
        IWHSystemRepository DbWHSystems { get; set; } = null!;

        [Inject]
        IWHSystemLinkRepository DbWHSystemLinks { get; set; } = null!;

        [Inject]
        IWHNoteRepository DbNotes { get; set; } = null!;

        [Inject]
        IWHJumpLogRepository DbWHJumpLogs { get; set; } = null!;

        [Inject]
        IEveAPIServices EveServices { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject] IWHSignatureHelper SignatureHelper { get; set; } = null!; 

        [Inject]
        public NavigationManager Navigation { get; set; } = null!;

        [Inject]
        public ILogger<Overview> Logger { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IEveMapperHelper MapperServices { get; set; } = null!;

        [Inject]
        private IEveMapperTracker TrackerServices { get; set; } = null!;

        [Inject]
        private IPasteServices PasteServices {get;set;}=null!;


        private string _userName = string.Empty;
        private int _characterId = 0;

        private IEnumerable<WHMap>? WHMaps { get; set; } = new List<WHMap>();
        private WHMap? _selectedWHMap = null!;
        
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
                    if(WHMaps!=null)
                        _selectedWHMap = WHMaps.ElementAtOrDefault(value);
                }
            }
        }

        private bool _loading = true;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        //private bool _isAdmin = false;


        private SystemEntity? _currentSolarSystem = null!;
        private Ship? _currentShip = null!;
        private ShipEntity? _currentShipInfos = null!;

        protected override async Task OnInitializedAsync()
        {
            _userName = await UserInfos.GetUserName();
            _characterId = await UserInfos.GetCharactedID();
            if(await InitDiagram())
            {
                _ = Task.Run(Init);
            }    
            else
            {
                Snackbar?.Add("Mapper Initialization error", Severity.Error);
            }      
        }

        private async Task Init()
        {
            if (await Restore())
            {
                if (await InitNotificationHub())
                {
                    TrackerServices.SystemChanged+=OnSystemChanged;
                    TrackerServices.ShipChanged+=OnShipChanged;
                    await TrackerServices.StartTracking();

                    PasteServices.Pasted+=OnPasted;
                }

                _loading = false;
                await InvokeAsync(() => {
                    StateHasChanged();
                });
            }
            else
            {
                Snackbar?.Add("Mapper restore error", Severity.Error);
            }
        }

        #region Hub methodes
        private async Task<bool> InitNotificationHub()
        {

            try
            {
                if (TokenProvider != null && !string.IsNullOrEmpty(TokenProvider.AccessToken) && _hubConnection == null)
                {
                    _hubConnection = new HubConnectionBuilder()
                         .WithUrl(Navigation.ToAbsoluteUri("/whmappernotificationhub"), options =>
                         {
                             options.AccessTokenProvider = () => Task.FromResult<string?>(TokenProvider.AccessToken);
                         }).Build();

                    _hubConnection.On<string>("NotifyUserConnected", (user) =>
                    {
                        try
                        {
                            Snackbar?.Add($"{user} are connected", Severity.Info);
                        }
                        catch(Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyUserConnected error");
                        }
                    });

                    _ = _hubConnection.On<string>("NotifyUserDisconnected", async (user) =>
                    {
                        try
                        {
                            EveSystemNodeModel? userSystem = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.ConnectedUsers.Contains(user));
                            if (userSystem != null)
                            {
                                await userSystem.RemoveConnectedUser(user);
                                userSystem.Refresh();
                            }
                            Snackbar?.Add($"{user} are disconnected", Severity.Info);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyUserDisconnected error");
                        }
                    });

                    _hubConnection.On<string, string>("NotifyUserPosition", async (user, systemName) =>
                    {
                        try
                        {
                            EveSystemNodeModel? previousSytem = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.ConnectedUsers.Contains(user)));
                            if (previousSytem != null)
                            {
                                await previousSytem.RemoveConnectedUser(user);
                                previousSytem.Refresh();
                            }


                            EveSystemNodeModel? systemToAddUser = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.Title == systemName);
                            if (systemToAddUser != null)
                            {
                                await systemToAddUser.AddConnectedUser(user);
                                systemToAddUser.Refresh();
                            }
                            else
                            {
                                Logger.LogWarning("On NotifyUserPosition, unable to find system to add user");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyUserPosition error");
                        }
                    });

                    _hubConnection.On<IDictionary<string, string>>("NotifyUsersPosition", async (usersPosition) =>
                    {
                        try
                        {
                            await Parallel.ForEachAsync(usersPosition, async (item, cancellationToken) =>
                            {
                                if(!string.IsNullOrEmpty(item.Value))
                                {
                                    EveSystemNodeModel? systemToAddUser = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).Title == item.Value);
                                    if(systemToAddUser!=null)
                                    {
                                        await systemToAddUser.AddConnectedUser(item.Key);
                                        systemToAddUser.Refresh();
                                    }
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyUserPosition error");
                        }
                    });

                    _hubConnection.On<string, int, int>("NotifyWormoleAdded", async (user, mapId, wormholeId) =>
                    {
                        try
                        {
                            if (wormholeId > 0 && mapId == _selectedWHMap?.Id)
                            {
                                var newWHSystem = await DbWHSystems.GetById(wormholeId);
                                while (newWHSystem == null)
                                    newWHSystem = await DbWHSystems.GetById(wormholeId);

                                var newSystemNode = await MapperServices.DefineEveSystemNodeModel(newWHSystem);
                                newSystemNode.OnLocked += OnWHSystemNodeLocked;
                                newSystemNode.OnSystemStatusChanged += OnWHSystemStatusChange;
                                _selectedWHMap.WHSystems.Add(newWHSystem);
                                _blazorDiagram?.Nodes?.Add(newSystemNode);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormoleAdded error");
                        }

                    });

                    _hubConnection.On<string, int, int>("NotifyWormholeRemoved", async (user, mapId, wormholeId) =>
                    {
                        try
                        {
                            if (DbWHMaps!=null && _selectedWHMap!=null && wormholeId > 0 && _selectedWHMap.Id== mapId)
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);
                                var systemNodeToDelete = _blazorDiagram.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                                if (systemNodeToDelete != null)
                                    _blazorDiagram.Nodes?.Remove(systemNodeToDelete);
                                else
                                {
                                    Logger.LogWarning("On NotifyWormholeRemoved, unable to find system to remove");
                                    Snackbar?.Add("On NotifyWormholeRemoved, unable to find system to remove", Severity.Warning);
                                
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormholeRemoved error");
                        }
                    });

                    _hubConnection.On<string, int, int>("NotifyLinkAdded", async (user, mapId, linKId) =>
                    {
                        try
                        {
                            if (DbWHMaps != null && linKId > 0 && _selectedWHMap!=null && _selectedWHMap?.Id== mapId )
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);
                                var link = _selectedWHMap?.WHSystemLinks.Where(x => x.Id == linKId).SingleOrDefault();
                                if (link != null)
                                {
                                    EveSystemNodeModel? newSystemNodeFrom = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => (x as EveSystemNodeModel)!.IdWH == link.IdWHSystemFrom));
                                    EveSystemNodeModel? newSystemNodeTo = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => (x as EveSystemNodeModel)!.IdWH == link.IdWHSystemTo));
                                    if(newSystemNodeTo!=null && newSystemNodeFrom!=null)
                                        _blazorDiagram.Links?.Add(new EveSystemLinkModel(link, newSystemNodeFrom, newSystemNodeTo));
                                    else
                                    {
                                        Logger.LogWarning("On NotifyLinkAdded, unable to find system to add link");
                                        Snackbar?.Add("On NotifyLinkAdded, unable to find system to add link", Severity.Warning);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyLinkAdded error");
                        }
                    });

                    _hubConnection.On<string, int, int>("NotifyLinkRemoved", async (user, mapId, linKId) =>
                    {
                        try
                        {
                            if (DbWHMaps != null && linKId > 0 && _selectedWHMap != null && _selectedWHMap.Id == mapId)
                            { 
                                _selectedWHMap = await DbWHMaps.GetById(mapId);

                                var linkToDel = _blazorDiagram.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linKId);
                                if (linkToDel != null)
                                    _blazorDiagram.Links?.Remove(linkToDel);
                                else
                                {
                                    Logger.LogWarning("On NotifyLinkRemoved, unable to find link to remove");
                                    Snackbar?.Add("On NotifyLinkRemoved, unable to find link to remove", Severity.Warning);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyLinkRemoved error");
                        }
                    });

                    _hubConnection.On<string, int, int, double, double>("NotifyWormoleMoved", async (user, mapId, wormholeId, posX, posY) =>
                    {
                        try
                        {
                            if (DbWHMaps != null && _selectedWHMap != null && wormholeId > 0 && _selectedWHMap.Id == mapId)
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);

                                var whToMoved = _blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                                if (whToMoved != null)
                                    whToMoved.SetPosition(posX, posY);
                                else
                                {
                                    Logger.LogWarning("On NotifyWormoleMoved, unable to find wormhole to move");
                                    Snackbar?.Add("On NotifyWormoleMoved, unable to find wormhole to move", Severity.Warning);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormoleMoved error");
                        }
                    });

                    _hubConnection.On<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus>("NotifyLinkChanged",async (user, mapId, linkId, eol, size, mass) =>
                    {
                        try
                        {
                            if (DbWHMaps != null && linkId > 0 && _selectedWHMap != null && _selectedWHMap.Id == mapId)
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);
                                var linkToChanged = _blazorDiagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linkId);
                                if (linkToChanged != null)
                                {
                                    ((EveSystemLinkModel)linkToChanged).IsEoL = eol;
                                    ((EveSystemLinkModel)linkToChanged).Size = size;
                                    ((EveSystemLinkModel)linkToChanged).MassStatus = mass;
                                    ((EveSystemLinkModel)linkToChanged).Refresh();
                                }
                                else
                                {
                                    Logger.LogWarning("On NotifyLinkChanged, unable to find link to change");
                                    Snackbar?.Add("On NotifyLinkChanged, unable to find link to change", Severity.Warning);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyLinkChanged error");
                        }
                    });

                    _hubConnection.On<string, int, int,bool>("NotifyWormholeNameExtensionChanged", async (user, mapId, wormholeId,increment) =>
                    {
                        try
                        {
                            if (DbWHMaps != null && _selectedWHMap != null && wormholeId > 0 && _selectedWHMap.Id == mapId)
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);
                                EveSystemNodeModel? systemToIncrementNameExtenstion = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                                if (systemToIncrementNameExtenstion != null)
                                {
                                    if (increment)
                                        systemToIncrementNameExtenstion.IncrementNameExtension();
                                    else
                                        systemToIncrementNameExtenstion.DecrementNameExtension();

                                    systemToIncrementNameExtenstion.Refresh();
                                }
                                else
                                {
                                    Logger.LogWarning("On NotifyWormholeNameExtensionChanged, unable to find wormhole to change name extension");
                                    Snackbar?.Add("On NotifyWormholeNameExtensionChanged, unable to find wormhole to change name extension", Severity.Warning);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormholeNameExtensionChanged error");
                        }

                    });

                    _hubConnection.On<string, int, int>("NotifyWormholeSignaturesChanged", async (user, mapId, wormholeId) =>
                    {
                        try
                        {
                            if (DbWHMaps != null && _selectedWHMap != null && wormholeId > 0 && _selectedWHMap.Id == mapId)
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);
                                if(WHSignaturesView!=null && WHSignaturesView.CurrentSystemNodeId ==wormholeId)
                                    await WHSignaturesView.Restore();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormholeSignaturesChanged error");
                        }
                    });



                    _ = _hubConnection.On<string, int, int, bool>("NotifyWormholeLockChanged", async (user, mapId, wormholeId, locked) =>
                    {
                        try
                        {
                            if (_selectedWHMap != null && wormholeId > 0 && mapId == _selectedWHMap.Id)
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);
                                EveSystemNodeModel? whChangeLock = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId));
                                if (whChangeLock != null)
                                {
                                    whChangeLock.Locked = locked;
                                    whChangeLock.Refresh();
                                }
                                else
                                {
                                    Logger.LogWarning("On NotifyWormholeLockChanged, unable to find wormhole to change lock");
                                    Snackbar?.Add("On NotifyWormholeLockChanged, unable to find wormhole to change lock", Severity.Warning);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormholeLockChanged error");
                        }

                    });

                    _hubConnection.On<string, int, int, WHSystemStatusEnum>("NotifyWormholeSystemStatusChanged", async (user, mapId, wormholeId, systemStatus) =>
                    {
                        try
                        {
                            if (_blazorDiagram!=null && _selectedWHMap != null && wormholeId > 0 && mapId == _selectedWHMap.Id)
                            {
                                _selectedWHMap = await DbWHMaps.GetById(mapId);
                                EveSystemNodeModel whChangeSystemStatus = (EveSystemNodeModel?)_blazorDiagram.Nodes.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                                if(whChangeSystemStatus!=null)
                                {
                                    whChangeSystemStatus.SystemStatus = systemStatus;
                                    whChangeSystemStatus.Refresh();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormholeLockChanged error");
                        }

                    });

                    await _hubConnection.StartAsync();

                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "InitNotificationHub error");
                return false;
            }

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

        private async Task NotifyWormholeNameExtensionChanged(int mapId, int wormholeId,bool increment)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeNameExtensionChanged", mapId, wormholeId, increment);
            }
        }

        private async Task NotifyWormholeSignaturesChanged(int mapId, int wormholeId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeSignaturesChanged", mapId, wormholeId);
            }
        }

        private async Task NotifyWormholeLockChanged(int mapId, int wormholeId, bool locked)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeLockChanged", mapId, wormholeId, locked);
            }
        }

        private async Task NotifyWormholeSystemStatusChanged(int mapId, int wormholeId, WHSystemStatusEnum systemStatus)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeSystemStatusChanged", mapId, wormholeId, systemStatus);
            }
        }

        #endregion

        private Task<bool> InitDiagram()
        {
            try
            {
                if (_blazorDiagram == null)
                {
                    Logger.LogInformation("Start Init Diagram");
                    _blazorDiagram = new BlazorDiagram();
                    _blazorDiagram.UnregisterBehavior<DragMovablesBehavior>();
                    _blazorDiagram.RegisterBehavior(new CustomDragMovablesBehavior(_blazorDiagram));


                    _blazorDiagram.SelectionChanged += async (item) => await OnDiagramSelectionChanged(item);
                    _blazorDiagram.KeyDown += async (kbevent) => await OnDiagramKeyDown(kbevent);
                    _blazorDiagram.PointerUp += async (item, pointerEvent) => await OnDiagramPointerUp(item, pointerEvent);
                    //_blazorDiagram.PointerEnter += async (item, pointerEvent) => await OnDiagramPointerEnter(item, pointerEvent);
                    //_blazorDiagram.PointerLeave += async (item, pointerEvent) => await OnDiagramPointerLeave(item, pointerEvent);

                    _blazorDiagram.Options.Zoom.Enabled = true;
                    _blazorDiagram.Options.Zoom.Inverse = false;
                    _blazorDiagram.Options.Links.EnableSnapping = false;
                    _blazorDiagram.Options.AllowMultiSelection = true;
                    _blazorDiagram.RegisterComponent<EveSystemNodeModel, EveSystemNode>();
                    _blazorDiagram.RegisterComponent<EveSystemLinkModel, EveSystemLink>();
                }

                return Task.FromResult(true);
            }            
            catch(Exception ex)
            {
                Logger.LogError(ex, "Init Diagram Error");
                return Task.FromResult(false);
            }
        }

        private async Task OnDiagramSelectionChanged(Blazor.Diagrams.Core.Models.Base.SelectableModel? item)
        {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    await InvokeAsync(() => {

                        if (item == null)
                            return;

                        var selectedModels = _blazorDiagram.GetSelectedModels();
                        _selectedSystemNodes = selectedModels.Where(x => x.GetType() == typeof(EveSystemNodeModel)).Select(x => (EveSystemNodeModel)x).ToList();
                        _selectedSystemLinks = selectedModels.Where(x => x.GetType() == typeof(EveSystemLinkModel)).Select(x => (EveSystemLinkModel)x).ToList();


                        if (item.GetType() == typeof(EveSystemNodeModel))
                        {
                            _selectedSystemLink = null;

                            if (((EveSystemNodeModel)item).Selected)
                            {
                                _selectedSystemNode = (EveSystemNodeModel)item;
                                _blazorDiagram.SendToFront(_selectedSystemNode);
                            }
                            else
                                _selectedSystemNode = null;

                            StateHasChanged();
                            return;

                        }

                        if (item.GetType() == typeof(EveSystemLinkModel))
                        {
                            _selectedSystemNode = null;

                            if (((EveSystemLinkModel)item).Selected)
                            {
                                _selectedSystemLink = (EveSystemLinkModel)item;
                                _blazorDiagram.SendToFront(_selectedSystemLink);
                            }
                            else
                                _selectedSystemLink = null;

                            
                            StateHasChanged();
                            return;
                        }

                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "On diagram selection changed error");
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
        }

        private async Task OnDiagramKeyDown(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
        {

            await _semaphoreSlim.WaitAsync();
            try
            {
                //To link systel
                if (await OnLinkSystemKeyPressed(eventArgs))
                    return;


                //To increment or decrment node extenstion name
                if (await OnIncrementOrDecrementExtensionKeyPressed(eventArgs))
                    return;


                //To Delete selected Node on current map
                if (await OnDeleteKeyPressed(eventArgs))
                    return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "OnDiagramKeyDown error");
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        #region Diagram Keyboard Pressed
        private async Task<bool> OnLinkSystemKeyPressed(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Code == "KeyL" && _selectedWHMap != null && _selectedSystemNodes != null && _selectedSystemNodes.Count == 2)
            { 
                if (IsLinkExist(_selectedSystemNodes.ElementAt(0), _selectedSystemNodes.ElementAt(1)))
                {
                    Snackbar.Add("Nodes are already linked", Severity.Warning);
                    return false;
                }
                else
                {
                    //link create manually, no ship jump
                    if (!await AddSystemNodeLink(_selectedWHMap, _selectedSystemNodes.ElementAt(0), _selectedSystemNodes.ElementAt(1),true))
                    {
                        Logger.LogError("Add Wormhole Link db error");
                        Snackbar.Add("Add Wormhole Link db error", Severity.Error);
                        return false;
                    }
                    else
                        return true;
                }
               
            }
            return false;

        }

        private async Task<bool> OnIncrementOrDecrementExtensionKeyPressed(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
        {
            if ((eventArgs.Code == "NumpadAdd" || eventArgs.Code == "NumpadSubtract" || eventArgs.Code == "ArrowUp" || eventArgs.Code == "ArrowDown") && _selectedWHMap != null && _selectedSystemNode != null)
            {
                bool res = false;
                if (eventArgs.Code == "NumpadAdd" || eventArgs.Code == "ArrowUp")
                    res = await IncrementOrDecrementNodeExtensionNameOnMap(_selectedWHMap, _selectedSystemNode, true);
                else
                    res = await IncrementOrDecrementNodeExtensionNameOnMap(_selectedWHMap, _selectedSystemNode, false);

                if (res)
                {
                    _selectedSystemNode.Refresh();
                    return true;
                }
                else
                {
                    Snackbar?.Add("Loading wormhole node db error", Severity.Error);
                    return false;
                }
            }
            return false;
        }

        private async Task<bool> OnDeleteKeyPressed(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Code == "Delete")
            {
                if (_selectedWHMap == null)
                {
                    Logger.LogError("OnDiagramKeyDown, no map selected to delete node or link");
                    Snackbar?.Add("No map selected to delete node or link", Severity.Error);
                    return false;
                }

                if (_selectedSystemNode != null && _selectedSystemNodes != null && _selectedSystemNodes.Count() > 0)
                {
                    await TrackerServices.StopTracking();
                    _currentSolarSystem=null;
                    foreach (var node in _selectedSystemNodes)
                    {
                        if (node.Locked)
                        {
                                Snackbar?.Add(string.Format("{0} wormhole is locked.You can't remove it.",node.Name), Severity.Warning);
                        }
                        else
                        {
                            if (!await DeletedNodeOnMap(_selectedWHMap, node))
                                Snackbar?.Add("Remove wormhole node db error", Severity.Error);
                        }
                    }
                    _selectedSystemNode = null;
                    StateHasChanged();
                    await TrackerServices.StartTracking();
                    return true;
                }

                if (_selectedSystemLink != null && _selectedSystemLinks != null && _selectedSystemLinks.Count() > 0)
                {
                    foreach (var link in _selectedSystemLinks)
                    {
                        if (!await DeletedLinkOnMap(_selectedWHMap, link))
                            Snackbar?.Add("Remove wormhole link db error", Severity.Error);

                    }
                    _selectedSystemLink = null;
                    StateHasChanged();

                    return true;

                }

                
            }
            return false;
        }

        #endregion

        private async Task OnDiagramPointerUp(Blazor.Diagrams.Core.Models.Base.Model? item, Blazor.Diagrams.Core.Events.PointerEventArgs eventArgs)
        {
            if (item == null)
                return;

            if (item.GetType() == typeof(EveSystemNodeModel))
            {
                await _semaphoreSlim.WaitAsync();
                try
                {

                    var wh = await DbWHSystems.GetById(((EveSystemNodeModel)item).IdWH);
                    if (wh != null)
                    {
                        if (wh.PosX != ((EveSystemNodeModel)item).Position.X || wh.PosY != ((EveSystemNodeModel)item).Position.Y)
                        {
                            wh.PosX = ((EveSystemNodeModel)item).Position.X;
                            wh.PosY = ((EveSystemNodeModel)item).Position.Y;

                            if (await DbWHSystems.Update(((EveSystemNodeModel)item).IdWH, wh) == null)
                            {
                                Snackbar?.Add("Update wormhole node position db error", Severity.Error);
                            }
                            if(_selectedWHMap!=null)
                                await NotifyWormholeMoved(_selectedWHMap.Id, wh.Id, wh.PosX, wh.PosY);
                            else
                            {
                                Logger.LogWarning("On Mouse pointer up, unable to find selected map to notify wormhole moved");
                                Snackbar?.Add("Unable to find selected map to notify wormhole moved", Severity.Warning);
                            
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError("On Mouse pointer up, unable to find moved wormhole node db error");
                        Snackbar?.Add("Unable to find moved wormhole node dd error", Severity.Error);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Mouse Pointer Up");
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

            }
            
        }

        private async Task<bool> Restore()
        {
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            try
            {
                if (DbWHMaps == null)
                {
                    Logger.LogError("DbWHMaps is null");
                    return false;
                }

                Logger.LogInformation("Beginning Restore Mapper");
                WHMaps = await DbWHMaps.GetAll();
                if (WHMaps == null || WHMaps.Count() == 0)
                {
                    _selectedWHMap = await DbWHMaps.Create(new WHMap("Default Maps"));
                    if (_selectedWHMap != null)
                        WHMaps = await DbWHMaps.GetAll();

                }
                else
                {
                    _selectedWHMap = WHMaps.FirstOrDefault();
                }

                if (_selectedWHMap != null && _selectedWHMap.WHSystems!=null &&_selectedWHMap.WHSystems.Count > 0)
                {

                    //await Parallel.ForEachAsync(_selectedWHMap.WHSystems, options, async (dbWHSys, token) =>
                    //{
                    foreach(var dbWHSys in _selectedWHMap.WHSystems)
                    {
                            EveSystemNodeModel whSysNode = await MapperServices.DefineEveSystemNodeModel(dbWHSys);
                            whSysNode.OnLocked += OnWHSystemNodeLocked;
                            whSysNode.OnSystemStatusChanged += OnWHSystemStatusChange;
                            _blazorDiagram.Nodes.Add(whSysNode);
                    }
                    //});
                    //StateHasChanged();
                

                    if (_selectedWHMap.WHSystemLinks!=null && _selectedWHMap.WHSystemLinks.Count > 0)
                    {
                        EveSystemNodeModel? srcNode=null!;
                        EveSystemNodeModel? targetNode=null!;

                        //await Parallel.ForEachAsync(_selectedWHMap.WHSystemLinks, options, async (dbWHSysLink, token) =>
                        foreach(var dbWHSysLink in _selectedWHMap.WHSystemLinks)
                        {
                            var whFrom = await DbWHSystems.GetById(dbWHSysLink.IdWHSystemFrom);
                            var whTo = await DbWHSystems.GetById(dbWHSysLink.IdWHSystemTo);
                            if (whFrom != null && whTo != null)
                            {
                                srcNode= _blazorDiagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, whFrom.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;
                                targetNode = _blazorDiagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, whTo.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;

                                if(srcNode!=null && targetNode!=null)
                                    _blazorDiagram.Links.Add(new EveSystemLinkModel(dbWHSysLink, srcNode, targetNode));
                                else
                                {
                                    Logger.LogWarning("Bad Link, srcNode or Targetnode is null, Auto remove");
                                    if(await DbWHSystemLinks.DeleteById(dbWHSysLink.Id))
                                    {
                                        _selectedWHMap.WHSystemLinks.Remove(dbWHSysLink);
                                    }

                                }
                            }
                            else
                            {
                                Logger.LogWarning("Bad Link,Auto remove");
                                if(await DbWHSystemLinks.DeleteById(dbWHSysLink.Id))
                                {
                                    _selectedWHMap.WHSystemLinks.Remove(dbWHSysLink);
                                }
                            }
                        }
                        //});
                        await InvokeAsync(() => {
                            StateHasChanged();
                        });
                    }
                }
                Logger.LogInformation("Restore Mapper Success");
                
                return true;
                
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Mapper Restore");
                return false;
            }
        }
        private async Task<bool> DeletedNodeOnMap(WHMap map, EveSystemNodeModel node)
        {
            try
            {
                if (map == null || node == null)
                {
                    Logger.LogError("DeletedNodeOnMap map or node is null");
                    return false;
                }

                if(map.WHSystems.Count>0 && map.WHSystems.FirstOrDefault(x=>x.Id==node.IdWH)==null)
                {
                    Logger.LogError("DeletedNodeOnMap map doesn't contain this node");
                    return false;
                }

                if (await DbWHSystems.DeleteById(node.IdWH))
                {
                    var whSystemToDelete = map.WHSystems.FirstOrDefault(x => x.Id == node.IdWH);
                    if(whSystemToDelete!=null)
                        map.WHSystems.Remove(whSystemToDelete);

                    //db link will be automatically delete via db foreignkey cascade
                    var whSystemLinksToDetele = map.WHSystemLinks.Where(x => x.IdWHSystemFrom == node.IdWH || x.IdWHSystemTo == node.IdWH);
                    foreach(var linkToDelete in whSystemLinksToDetele)
                        map.WHSystemLinks.Remove(linkToDelete);
                    
                    await NotifyWormholeRemoved(map.Id, node.IdWH);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Deleted node on map error");
                return false;
            }
        }
        private async Task<bool> DeletedLinkOnMap(WHMap map, EveSystemLinkModel link)
        {
            try
            {
                if (map == null || link == null)
                {
                    Logger.LogError("DeletedLinkOnMap map or link is null");
                    return false;
                }

                if(await DbWHSystemLinks.DeleteById(link.Id))
                {
                    var whSystemLinkToDelete = map.WHSystemLinks.FirstOrDefault(x => x.Id == link.Id);
                    if (whSystemLinkToDelete != null)
                        map.WHSystemLinks.Remove(whSystemLinkToDelete);

                    await NotifyLinkRemoved(map.Id, link.Id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Deleted link on map error");
                return false;
            }

        }
        private async Task<bool> IncrementOrDecrementNodeExtensionNameOnMap(WHMap map, EveSystemNodeModel node,bool increment)
        {
            try
            {
                if (map == null || node == null)
                {
                    Logger.LogError("IncrementOrDecrementNodeExtensionNameOnMap map or node is null");
                    return false;
                }

                var wh = await DbWHSystems.GetById(node.IdWH);
                if (wh != null)
                {
                    if (increment)
                        node.IncrementNameExtension();
                    else
                        node.DecrementNameExtension();

                    if (node.NameExtension == null)
                        wh.NameExtension = 0;
                    else
                        wh.NameExtension = ((byte)((node.NameExtension.ToCharArray())[0]));

                    wh = await DbWHSystems.Update(wh.Id, wh);


                    if (wh != null)
                    {
                        await NotifyWormholeNameExtensionChanged(map.Id, wh.Id, increment);
                        return true;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Increment Or Decrement node extension name error");
                return false;
            }
        }


        private EveSystemLinkModel? GetLink(EveSystemNodeModel src, EveSystemNodeModel target)
        {
            try
            {
                if (src == null || target == null)
                {
                    Logger.LogError("LinkExist src or target is null");
                    return null;
                }

                var whLink = _blazorDiagram.Links.FirstOrDefault(x =>
                ((((EveSystemNodeModel)x.Source!.Model!).IdWH == src.IdWH) && (((EveSystemNodeModel)x.Target!.Model!).IdWH == target.IdWH))
                ||
                ((((EveSystemNodeModel)x.Source!.Model!).IdWH == target.IdWH) && (((EveSystemNodeModel)x.Target!.Model!).IdWH == src.IdWH)));

                if (whLink == null)
                    return null;
                else
                    return (EveSystemLinkModel)whLink;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Get Link error");
                return null;
            }
        }

        private bool IsLinkExist(EveSystemNodeModel src, EveSystemNodeModel target)
        {
            try
            {
                var link = this.GetLink(src, target);

                if (link == null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Is Link Exist error");
                return false;
            }

        }

        private async Task<bool> AddSystemNode(WHMap map,SystemEntity? src,SystemEntity target)
        {
            EveSystemNodeModel? previousSystemNode = null;
            WHSystem? newWHSystem= null;
            double defaultNewSystemPosX = 0;
            double defaultNewSystemPosY = 0;
            char extension;
            int nbSameWHClassLink =0;
            
            try
            {
                //determine position on map. depends of previous system , todo refactor
                if (src != null)
                {
                    previousSystemNode = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).SolarSystemId == src.Id));
            
                    if(previousSystemNode!=null)
                    {
                        defaultNewSystemPosX  = previousSystemNode.Position!.X + previousSystemNode.Size!.Width + 10;
                        defaultNewSystemPosY = previousSystemNode.Position!.Y + previousSystemNode!.Size!.Height + 10;
                    }
                    else
                    {
                        defaultNewSystemPosX=10;
                        defaultNewSystemPosY=10;
                    }
                }

                //determine if source have same system link and get next unique ident
                if(src != null && await MapperServices.IsRouteViaWH(src, target)) //check if HS/LS/NS to HS/LS/NS via WH not gate
                {

                    //get whClass an determine if another connection to another wh with same class exist from previous system. Increment extension value in that case
                    EveSystemType whClass = await MapperServices.GetWHClass(target);
                    var sameWHClassWHList = _blazorDiagram?.Links?.Where(x =>  ((EveSystemNodeModel)(x.Target!.Model!)).SystemType == whClass && ((EveSystemNodeModel)x.Source!.Model!).SolarSystemId == src.Id);
                    
                    if(sameWHClassWHList!=null)
                        nbSameWHClassLink = sameWHClassWHList.Count();
                    else
                        nbSameWHClassLink=0;

                    if (nbSameWHClassLink > 0)
                    {
                        extension = (Char)(Convert.ToUInt16('A') + (nbSameWHClassLink));
                        newWHSystem = await DbWHSystems.Create(new WHSystem(map.Id, target.Id, target.Name, extension, target.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                    }
                    else
                        newWHSystem = await DbWHSystems.Create(new WHSystem(map.Id, target.Id, target.Name, target.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                
                }
                else
                    newWHSystem = await DbWHSystems.Create(new WHSystem(map.Id,target.Id,target.Name, target.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));

                if (newWHSystem!=null)
                {
                    var newSystemNode = await MapperServices.DefineEveSystemNodeModel(newWHSystem);
                    newSystemNode.OnLocked += OnWHSystemNodeLocked;
                    newSystemNode.OnSystemStatusChanged += OnWHSystemStatusChange;
                    
                    await newSystemNode.AddConnectedUser(_userName);

                    map.WHSystems.Add(newWHSystem);
                    _blazorDiagram?.Nodes?.Add(newSystemNode);
                    await NotifyWormoleAdded(map.Id, newWHSystem.Id);
                    

                    if (previousSystemNode != null)
                    {
                        //remove ConnectedUser on previous system
                        await previousSystemNode.RemoveConnectedUser(_userName);
                        previousSystemNode.Refresh();
                    }


                    _blazorDiagram?.SelectModel(newSystemNode, true);
                    return true;
                }
                else
                {
                    Logger.LogError("Add Wormhole db error");
                    return false;
                }
 
                
            }
            catch(Exception ex)
            {
                Logger.LogError(ex,"AddSystemNode Error");
                return false;
            }
        }
        private async Task<bool> AddSystemNodeLink(WHMap map, SystemEntity src, SystemEntity target)
        {
            if (_blazorDiagram == null  || map == null || src == null || target == null)
            {
                Logger.LogError("CreateLink map or src or target is null");
                return false;
            }

            EveSystemNodeModel? srcNode = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).SolarSystemId == src.Id));
            EveSystemNodeModel? targetNode = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).SolarSystemId == target.Id));
    
            return await AddSystemNodeLink(map,srcNode,targetNode);
        }
        private async Task<bool> AddSystemNodeLink(WHMap map, EveSystemNodeModel? srcNode, EveSystemNodeModel? targetNode,bool isManual=false)
        {
            try
            {
                if(srcNode==null || targetNode ==null)
                {
                    Logger.LogError("CreateLink src or target node is null");
                    return false;
                }

                if(srcNode.SolarSystemId==targetNode.SolarSystemId)
                {
                    Logger.LogError("CreateLink src and target node are the same");
                    return false;
                }

                var newLink = await DbWHSystemLinks.Create(new WHSystemLink(map.Id, srcNode.IdWH, targetNode.IdWH));

                if (newLink != null)
                {
                    await AddSystemNodeLinkLog(newLink,isManual);

                    map.WHSystemLinks.Add(newLink);
                    _blazorDiagram?.Links?.Add(new EveSystemLinkModel(newLink, srcNode, targetNode));
                    await NotifyLinkAdded(map.Id, newLink.Id);
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "AddSystemNodeLink error");
                return false;
            }
        }
        internal async Task<bool> AddSystemNodeLinkLog(int whSystemLinkID,bool manuelJump=false)
        {
            if(_currentShip==null || _currentShipInfos==null)
            {
                Logger.LogError("AddSystemNodeLinkLog currentShip or currentShipInfos is null");
                return false;
            }
            WHJumpLog? jumpLog = null;

            if(manuelJump)
                jumpLog = await DbWHJumpLogs.Create(new WHJumpLog(whSystemLinkID,_characterId));
            else
                jumpLog = await DbWHJumpLogs.Create(new WHJumpLog(whSystemLinkID,_characterId,_currentShip.ShipTypeId,_currentShip.ShipItemId,_currentShipInfos.Mass));
            
            if(jumpLog==null)
            {
                Logger.LogError("AddSystemNodeLinkLog jumpLog is null");
                return false;
            }        

            return true;

        }
        private async Task<bool> AddSystemNodeLinkLog(WHSystemLink? link,bool isManual=false)
        {
            if(link==null)
            {
                Logger.LogError("AddSystemNodeLinkLog link is null");
                return false;
            }

            return await AddSystemNodeLinkLog(link.Id,isManual);
        }

        private Task OnShipChanged(Ship ship,ShipEntity shipInfos)
        {
            _currentShip=ship;
            _currentShipInfos=shipInfos;

            return Task.CompletedTask;
        }
        private async Task OnSystemChanged(SystemEntity targetSoloarSystem)
        {
            EveSystemNodeModel? srcNode  = null;
            EveSystemNodeModel? targetNode = null;

            if(_blazorDiagram==null || targetSoloarSystem==null)
            {
                Logger.LogError("Error Diagram or newSoloarSystem is nullable");
                return;
            }

            if(_selectedWHMap==null)
            {
                Logger.LogError("Error OnSystemChanged, selectedWHMap is nullable");
                return;
            }

            await _semaphoreSlim.WaitAsync();
            try
            {
                
                if(_currentSolarSystem!=null)
                {
                    srcNode = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).SolarSystemId == _currentSolarSystem.Id));
                }
                targetNode = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).SolarSystemId == targetSoloarSystem.Id));
                

                if (targetNode== null)//System is not added
                {
                    if(await AddSystemNode(_selectedWHMap,_currentSolarSystem,targetSoloarSystem))//add system node if system is not added
                    {
                        if (_currentSolarSystem != null)
                        {
                            if(!await AddSystemNodeLink(_selectedWHMap, _currentSolarSystem, targetSoloarSystem))//create if new target system added from src
                            {
                                Logger.LogError("Add Wormhole Link error");
                                Snackbar?.Add("Add Wormhole Link error", Severity.Error);
                            }
                            else
                            {
                                targetNode = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).SolarSystemId == targetSoloarSystem.Id));
                            }
                        }

                        await NotifyUserPosition(targetSoloarSystem.Name);
                    }
                    else
                    {
                        Logger.LogError("Add System Node error");
                        Snackbar?.Add("Add System Node error", Severity.Error);
                    }
                }
                else// tartget system already added
                {
                    //check if link already exist, if not create if
                    if(srcNode!=null && !IsLinkExist(srcNode,targetNode))
                    {
                        if((_currentSolarSystem==null) || (!await AddSystemNodeLink(_selectedWHMap, _currentSolarSystem, targetSoloarSystem)))//create if new target system added from src
                        {
                            Logger.LogError("Add Wormhole Link error");
                            Snackbar?.Add("Add Wormhole Link error", Severity.Error);
                        }
                    }
                    else//log jump
                    {
                        if(srcNode!=null && targetNode!=null)
                        {
                            var link = GetLink(srcNode,targetNode);

                            if(link!=null && !await AddSystemNodeLinkLog(link.Id))
                            {
                                Logger.LogError("Add Wormhole Link Log error");
                                Snackbar?.Add("Add Wormhole Link Log error", Severity.Error);
                            }
                        }
                    }


                    srcNode?.RemoveConnectedUser(_userName);
                    targetNode?.AddConnectedUser(_userName);
                    await NotifyUserPosition(targetSoloarSystem.Name);

                }

                _currentSolarSystem = targetSoloarSystem;
                if(targetNode!=null)
                    _blazorDiagram?.SelectModel(targetNode, true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "On System Changed");
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task OnPasted(string? data)
        {
            if((_selectedWHMap!=null) && (_selectedSystemNode!=null))
            {
                try
                {
                    string scanUser = await UserInfos.GetUserName();
                    if (await SignatureHelper.ImportScanResult(scanUser, _selectedSystemNode.IdWH, data,false))
                    {
                        await WHSignaturesView.Restore();
                        Snackbar?.Add("Signatures successfully added/updated", Severity.Success);
                        await NotifyWormholeSignaturesChanged(_selectedWHMap.Id, _selectedSystemNode.IdWH);
                    }
                    else
                        Snackbar?.Add("No signatures added/updated", Severity.Error);
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "Handle Custom Paste error");
                    Snackbar?.Add(ex.Message, Severity.Error);
                }
            }
        }
        public async ValueTask DisposeAsync()
        {
            if(TrackerServices!=null)
            {
                await TrackerServices.StopTracking();
                TrackerServices.SystemChanged-=OnSystemChanged;
                TrackerServices.ShipChanged-=OnShipChanged;

            }
            if (_hubConnection!=null)
            {
    
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        private void OnWHSystemNodeLocked(EveSystemNodeModel whNodeModel)
        {
            if (whNodeModel != null)
            {
                Task.Run(()=>NotifyWormholeLockChanged(whNodeModel.IdWHMap, whNodeModel.IdWH, whNodeModel.Locked));
            }
        }

        private void OnWHSystemStatusChange(EveSystemNodeModel whNodeModel)
        {
            if (whNodeModel != null)
            {
                Task.Run(()=>NotifyWormholeSystemStatusChanged(whNodeModel.IdWHMap, whNodeModel.IdWH, whNodeModel.SystemStatus));
            }
        }

        #region Menu Actions

        #region Selected Node Menu Actions
        private async Task<bool> SetSelectedSystemDestinationWaypoint()
        {
            try
            {
                if(_selectedSystemNode==null)
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }
                int solarSystemId = _selectedSystemNode.SolarSystemId;
                if (solarSystemId>0)
                {
                    await EveServices.UserInterfaceServices.SetWaypoint(solarSystemId, false, true);
                    return true;

                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Set destination waypoint error");
                return false;
            }
        }

        private async Task<bool> ToggleSystemLock()
        {
            try
            {
                if(_selectedSystemNode==null)
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }

                var whSystem = await DbWHSystems.GetById(_selectedSystemNode.IdWH);
                if (whSystem != null && whSystem.Id==_selectedSystemNode.IdWH)
                {
                    whSystem.Locked = !whSystem.Locked;
                    whSystem = await DbWHSystems.Update(whSystem.Id, whSystem);
                    if (whSystem == null)
                    {
                        Logger.LogError("Update lock system status error");
                        return false;
                    }
                    _selectedSystemNode.Locked = whSystem.Locked;
                    _selectedSystemNode.Refresh();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Toggle system lock error");
                return false;
            }
         
        }
        private async Task<bool> SetSelectedSystemStatus(WHSystemStatusEnum systemStatus)
        {
            try
            {
                if(_selectedSystemNode==null)
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }
                int solarSystemId = _selectedSystemNode.SolarSystemId;
                if (solarSystemId>0)
                {
                    var note = await DbNotes.GetBySolarSystemId(solarSystemId);

                    if(note == null)
                    {
                        note = await DbNotes.Create(new WHNote(solarSystemId, systemStatus));
                    }
                    else
                    {
                        note.SystemStatus = systemStatus;
                        note = await DbNotes.Update(note.Id, note);
                    }

           
                    if(note==null)
                    {
                        Logger.LogError("Could not update system status");
                        return false;
                    }

                    _selectedSystemNode.SystemStatus = systemStatus;
                    _selectedSystemNode.Refresh();
                    return true;
                }
                else
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Set system status error");
                return false;
            }
        }
        #endregion
        private async Task<bool> ToggleSlectedSystemLinkEOL()
        {
            try
            {
                if (_selectedWHMap!=null && _selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.IsEndOfLifeConnection = !link.IsEndOfLifeConnection;
                        link = await DbWHSystemLinks.Update(_selectedSystemLink.Id, link);
                        if(link!=null)
                        {
                            _selectedSystemLink.IsEoL = link.IsEndOfLifeConnection;
                            _selectedSystemLink.Refresh();
                            await NotifyLinkChanged(_selectedWHMap.Id, link.Id, link.IsEndOfLifeConnection, link.Size,link.MassStatus);
                            return true;
                        }
                        else
                        {
                            Logger.LogError("Toggle system link eol db error");
                            return false;
                        }
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
                Logger.LogError(ex, "Toggle system link eol error");
                return false;
            }
        }

        private async Task<bool> SetSelectedSystemLinkStatus(SystemLinkMassStatus massStatus)
        {
            try
            {
                if (_selectedWHMap!=null && _selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.MassStatus = massStatus;
                        link = await DbWHSystemLinks.Update(_selectedSystemLink.Id, link);
                        if(link!=null)
                        {
                            _selectedSystemLink.MassStatus = link.MassStatus;
                            _selectedSystemLink.Refresh();
                            await NotifyLinkChanged(_selectedWHMap.Id, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);
                            return true;
                        }
                        else
                        {
                            Logger.LogError("Set system link status db error");
                            return false;
                        }
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
                Logger.LogError(ex, "System link status error");
                return false;
            }
        }

        private async Task<bool> SetSelectedSystemLinkSize(SystemLinkSize size)
        {
            try
            {
                if (_selectedWHMap!=null && _selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.Size = size;
                        link = await DbWHSystemLinks.Update(_selectedSystemLink.Id, link);
                        if(link!=null)
                        {
                                //update link size on diagram (refresh link
                            _selectedSystemLink.Size = link.Size;
                            _selectedSystemLink.Refresh();
                            await NotifyLinkChanged(_selectedWHMap.Id, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);

                            return true;
                        }
                        else
                        {
                            Logger.LogError("Set system link size db error");
                            return false;
                        }

                    }
                }

                return false;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "System link size error");
                return false;
            }
        }
       
        private async Task<bool> OpenSearchAndAddDialog(ItemClickEventArgs e)
        {
            DialogOptions disableBackdropClick = new DialogOptions()
            {
                DisableBackdropClick = true,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters(); 
            parameters.Add("CurrentDiagram", _blazorDiagram);
            parameters.Add("CurrentWHMap", _selectedWHMap);
            parameters.Add("MouseX", e.MouseEvent.ClientX);
            parameters.Add("MouseY", e.MouseEvent.ClientY);

            var dialog = DialogService.Show<Add>("Search and Add System Dialog", parameters, disableBackdropClick);
            DialogResult result = await dialog.Result;

            if (!result.Canceled && result.Data != null)
            {
                int whAddedId = (int)result.Data;
                if (_selectedWHMap!=null && whAddedId > 0)
                    await this.NotifyWormoleAdded(_selectedWHMap.Id, whAddedId);
                else
                {
                    Logger.LogError("OpenSearchAndAddDialog, unable to find selected map to notify wormhole added");
                    Snackbar?.Add("Unable to find selected map to notify wormhole added", Severity.Warning);
                }
            }

            return true;

        }
        #endregion


#if !DISABLE_MULTI_MAP
        #region Tabs Actions

        [Authorize(Policy = "Access")]
        private Task AddNewMap()
        {
            return Task.CompletedTask;
        }
        #endregion
#endif
    }
}