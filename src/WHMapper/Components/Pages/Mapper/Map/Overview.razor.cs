using Blazor.Diagrams;
using Blazor.Diagrams.Core.Behaviors;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Components.Pages.Mapper.Search;
using WHMapper.Services.EveOAuthProvider.Services;
using System.Collections.Concurrent;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO;
using Microsoft.AspNetCore.Components.Web;
using WHMapper.Components.Pages.Mapper.CustomNode;
using Mono.TextTemplating;

namespace WHMapper.Components.Pages.Mapper.Map;

public partial class Overview : IAsyncDisposable
{
    private const float EPSILON = 0.0001f; // or some other small value
    private const string UidNullOrEmptyMessage = "UID is null or empty";

    private BlazorDiagram _blazorDiagram  = null!;
    private int? _selectedWHMapId = null;

    private bool _loading = true;

    private EveSystemNodeModel? _selectedSystemNode =null;
    private EveSystemNodeModel? SelectedSystemNode
    {
        get => _selectedSystemNode;
        set
        {
            if (_selectedSystemNode != value)
            {
                _selectedSystemNode = value;
                StateHasChanged();
            }
        }
    }

    private EveSystemLinkModel? _selectedSystemLink =null;
    private EveSystemLinkModel? SelectedSystemLink
    {
        get => _selectedSystemLink;
        set
        {
            if (_selectedSystemLink != value)
            {
                _selectedSystemLink = value;
                StateHasChanged();
            }
        }
    }

    private ICollection<EveSystemNodeModel>? _selectedSystemNodes = null;
    private ICollection<EveSystemLinkModel>? _selectedSystemLinks = null;
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _semaphoreSlim2 = new SemaphoreSlim(1, 1);

    private ConcurrentDictionary<int, Ship> _currentShips= new ConcurrentDictionary<int, Ship>();


    [Inject]
    private ILogger<Overview> Logger { get; set; } = null!;
    
    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    
    [Inject]
    private IEveAPIServices EveServices { get; set; } = null!;
    [Inject]
    private IEveMapperTracker TrackerServices { get; set; } = null!;
    [Inject]
    private IEveMapperHelper MapperServices { get; set; } = null!;

    [Inject]
    private IEveMapperRealTimeService EveMapperRealTime { get; set; } = null!;    

    [Inject]
    private IEveOnlineTokenProvider TokenProvider {get; set;} = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;


    [Inject]
    private IEveMapperService eveMapperService { get; set; } = null!;
    

    #region Repositories
    [Inject]
    IWHMapRepository DbWHMaps { get; set; } = null!;
    
    [Inject]
    IWHSystemRepository DbWHSystems { get; set; } = null!;

    [Inject]
    IWHSystemLinkRepository DbWHSystemLinks { get; set; } = null!;

    [Inject]
    IWHJumpLogRepository DbWHJumpLogs { get; set; } = null!;

    [Inject]
    IWHNoteRepository DbNotes { get; set; } = null!;
    #endregion
    
    [Parameter]
    public int? MapId {  get; set; } = null!;


    private async Task<WHMapperUser[]?> GetAccountsAsync()
    {
        if (UID == null || string.IsNullOrEmpty(UID.ClientId))
        {
            Logger.LogError(UidNullOrEmptyMessage);
            throw new InvalidOperationException(UidNullOrEmptyMessage);
        }

        return await UserManagement.GetAccountsAsync(UID.ClientId);
    }

    private async Task<WHMapperUser?> GetPrimaryAccountAsync()
    {
        if (UID == null || string.IsNullOrEmpty(UID.ClientId))
        {
            Logger.LogError(UidNullOrEmptyMessage);
            throw new InvalidOperationException(UidNullOrEmptyMessage);
        }

        return await UserManagement.GetPrimaryAccountAsync(UID.ClientId);
    }

    
    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        if(UID==null || String.IsNullOrEmpty(UID.ClientId))
        {
            Logger.LogError(UidNullOrEmptyMessage);
            throw new InvalidOperationException(UidNullOrEmptyMessage);
        }


        if(!await InitDiagram())
        {
            Snackbar?.Add("Map Initialization error", Severity.Error);
        }

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _loading = true;
            if(MapId.HasValue && (!_selectedWHMapId.HasValue || _selectedWHMapId.Value!=MapId.Value))
            {
                
                if(await Restore(MapId.Value))
                {   

                    //get all user account authorized on map
                    WHMapperUser[]? accounts= await GetAccountsAsync();
                    if(accounts!=null)
                    {
                        foreach(var account in accounts)
                        {
                            IDictionary<int,KeyValuePair<int,int>?> usersPosition = await EveMapperRealTime.GetConnectedUsersPosition(account.Id);
                            try
                            {
                                var tasks = usersPosition
                                    .Where(item => item.Value.HasValue && item.Value.Value.Key == MapId.Value)
                                    .Select(async item =>
                                    {
                                        var systemToAddUser = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == (item.Value?.Value ?? 0));
                                        if (systemToAddUser != null)
                                        {
                                            CharactereEntity? user= await eveMapperService.GetCharacter(item.Key);
                                            if(user!=null)
                                            {
                                                await systemToAddUser.AddConnectedUser(user.Name);
                                                systemToAddUser.Refresh();
                                            }
                                        }
                                    });

                                await Task.WhenAll(tasks);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "On NotifyUserPosition error");
                            }

                            await EveMapperRealTime.NotifyUserOnMapConnected(account.Id,MapId.Value);
                            if(account.Tracking)
                                await TrackerServices.StartTracking(account.Id);
                        }
                        
                        await InitBlazorDiagramEvents();
                    }

                    TrackerServices.SystemChanged += OnSystemChanged;
                    TrackerServices.ShipChanged += OnShipChanged;


                    EveMapperRealTime.UserDisconnected += OnUserDisconnected;
                    EveMapperRealTime.UserOnMapConnected += OnUserOnMapConnected;
                    EveMapperRealTime.UserOnMapDisconnected += OnUserOnMapDisconnected;

                    EveMapperRealTime.UserPosition += OnUserPositionChanged;

                    EveMapperRealTime.WormholeAdded += OnWormholeAdded;
                    EveMapperRealTime.WormholeRemoved += OnWormholeRemoved;
                    EveMapperRealTime.WormholeMoved += OnWormholeMoved;
                    EveMapperRealTime.WormholeLockChanged += OnWormholeLockChanged;
                    EveMapperRealTime.WormholeSystemStatusChanged += OnWormholeSystemStatusChanged;
                    EveMapperRealTime.WormholeNameExtensionChanged += OnWormholeNameExtensionChanged;

                    EveMapperRealTime.LinkAdded += OnLinkAdded;
                    EveMapperRealTime.LinkRemoved += OnLinkRemoved;
                    EveMapperRealTime.LinkChanged += OnLinkChanged;
                }
                else
                {
                    Snackbar?.Add("Map Restore error", Severity.Error);
                }
            }
            _loading = false;
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        if(TrackerServices!=null)
        {
            TrackerServices.SystemChanged-=OnSystemChanged;
            TrackerServices.ShipChanged-=OnShipChanged;
            await TrackerServices.DisposeAsync();
        }

        if(EveMapperRealTime!=null)
        {
            EveMapperRealTime.UserDisconnected -= OnUserDisconnected;
            EveMapperRealTime.UserOnMapConnected -= OnUserOnMapConnected;
            EveMapperRealTime.UserOnMapDisconnected -= OnUserOnMapDisconnected;
            
            EveMapperRealTime.UserPosition -= OnUserPositionChanged;
            EveMapperRealTime.WormholeAdded -= OnWormholeAdded;
            EveMapperRealTime.WormholeRemoved -= OnWormholeRemoved;
            EveMapperRealTime.WormholeMoved -= OnWormholeMoved;
            EveMapperRealTime.WormholeLockChanged -= OnWormholeLockChanged;
            EveMapperRealTime.WormholeSystemStatusChanged -= OnWormholeSystemStatusChanged;
            EveMapperRealTime.WormholeNameExtensionChanged -= OnWormholeNameExtensionChanged;
            EveMapperRealTime.LinkAdded -= OnLinkAdded;
            EveMapperRealTime.LinkRemoved -= OnLinkRemoved;
            EveMapperRealTime.LinkChanged -= OnLinkChanged;



/*
            if (MapId.HasValue && EveMapperRealTime.IsConnected()
            {

                await EveMapperRealTime.NotifyUserOnMapDisconnected(MapId.Value);
            }*/
        }

        _currentShips.Clear();
        
        await ClearDiagram();
        GC.SuppressFinalize(this);
    }

    private async Task<bool> Restore(int mapId)
    {
        try
        {

            WHMap? selectedWHMap = null;
            Logger.LogInformation("Beginning Restore Map");

            if (DbWHMaps == null)
            {
                Logger.LogError("DbWHMaps is null");
                return false;
            }

            selectedWHMap = await DbWHMaps.GetById(mapId);

    
            if(selectedWHMap != null)
            {
                _blazorDiagram.SuspendSorting = true;
                _blazorDiagram.SuspendRefresh = true;
                await ClearDiagram();
                await InitializeSystemNodes(selectedWHMap);
                await InitializeSystemLinks(selectedWHMap);
                _blazorDiagram.SuspendSorting = false;
                _blazorDiagram.SuspendRefresh = false;
            }

            Logger.LogInformation("Restore Mapper Success");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Mapper Restore");
            return false;
        }
    }

    private Task ClearDiagram()
    {
        if (_blazorDiagram != null)
        {
            _blazorDiagram.Links.Clear();
            _blazorDiagram.Nodes.Clear();

        }

        return Task.CompletedTask;
    }

    private Task<bool> InitDiagram()
    {
        try
        {
            Logger.LogInformation("Start Init Diagram");
            if (_blazorDiagram == null)
            {

                _blazorDiagram = new BlazorDiagram();
                _blazorDiagram.UnregisterBehavior<DragMovablesBehavior>();
                _blazorDiagram.RegisterBehavior(new CustomDragMovablesBehavior(_blazorDiagram));
                _blazorDiagram.Options.Zoom.Enabled = true;
                _blazorDiagram.Options.Zoom.Inverse = false;
                _blazorDiagram.Options.Links.EnableSnapping = false;
                _blazorDiagram.Options.AllowMultiSelection = true;
                _blazorDiagram.RegisterComponent<EveSystemNodeModel, EveSystemNode>();
                _blazorDiagram.RegisterComponent<EveSystemLinkModel, EveSystemLink>();
            }
            else
            {
                Logger.LogWarning("Diagram already initialized, clear nodes and links");
                _blazorDiagram.Nodes.Clear();
                _blazorDiagram.Links.Clear();
            }

            Logger.LogInformation("Init Diagram Success");
            return Task.FromResult(true);
        }            
        catch(Exception ex)
        {
            Logger.LogError(ex, "Init Diagram Error");
            return Task.FromResult(false);
        }
    }
    private Task InitBlazorDiagramEvents()
    {
        if (_blazorDiagram == null)
        {
            Logger.LogError("Blazor Diagram is null,Blaor Diagram Events not initialized");
            throw new NullReferenceException("Blazor Diagram is null");
        }

        _blazorDiagram.SelectionChanged += async (item) => await OnDiagramSelectionChanged(item);
        _blazorDiagram.KeyDown += async (kbevent) => await OnDiagramKeyDown(kbevent);
        _blazorDiagram.PointerUp += async (item, pointerEvent) => await OnDiagramPointerUp(item, pointerEvent);

        return Task.CompletedTask;
    }

    private async Task InitializeSystemNodes(WHMap? selectedWHMap)
    {
        if (selectedWHMap?.WHSystems == null || !selectedWHMap.WHSystems.Any())
        {
            return;
        }

        var tasks = selectedWHMap.WHSystems.Select(item => Task.Run(async () =>
        {
            var whSysNode = await MapperServices.DefineEveSystemNodeModel(item);
            whSysNode.OnLocked += OnWHSystemNodeLockedAsync;
            whSysNode.OnSystemStatusChanged += OnWHSystemStatusChangeAsync;
           
            _blazorDiagram.Nodes.Add(whSysNode);
        }));

        await Task.WhenAll(tasks);
    }

    private async Task InitializeSystemLinks(WHMap? selectedWHMap)
    {
        if (selectedWHMap?.WHSystemLinks == null || !selectedWHMap.WHSystemLinks.Any())
        {
            return;
        }

        var tasks = selectedWHMap.WHSystemLinks.Select(item => Task.Run(async () =>
        {
            try
            {
            var srcNode = await GetSystemNode(item.IdWHSystemFrom);
            var targetNode = await GetSystemNode(item.IdWHSystemTo);

            if (srcNode != null && targetNode != null)
            {
                _blazorDiagram.Links.Add(new EveSystemLinkModel(item, srcNode, targetNode));
            }
            else
            {
                Logger.LogWarning("Bad Link, srcNode or Targetnode is null, Auto remove");
                //await DbWHSystemLinks.DeleteById(item.Id);
            }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Initialize System Links Error");
            }
        }));

        await Task.WhenAll(tasks);
    }

    #region Diagram Events

    #region Diagram Selection
    private async Task OnDiagramSelectionChanged(Blazor.Diagrams.Core.Models.Base.SelectableModel? item)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            await InvokeAsync(() =>
            {
                if (item == null)
                    return;

                var selectedModels = _blazorDiagram.GetSelectedModels();
                _selectedSystemNodes = selectedModels.OfType<EveSystemNodeModel>().ToList();
                _selectedSystemLinks = selectedModels.OfType<EveSystemLinkModel>().ToList();


                if(_selectedSystemNodes.Count>1 || _selectedSystemLinks.Count>1)
                {
                    SelectedSystemNode = null;
                    SelectedSystemLink = null;
                }
                else
                {               
                    if (item is EveSystemNodeModel node)
                    {
                        HandleNodeSelection(node);
                    }
                    else if (item is EveSystemLinkModel link)
                    {
                        HandleLinkSelection(link);
                    }
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

    private void HandleNodeSelection(EveSystemNodeModel node)
    {
        SelectedSystemLink = null;

        if (node.Selected)
        {
            SelectedSystemNode = node;
            _blazorDiagram.SendToFront(SelectedSystemNode);
        }
        else
        {
            SelectedSystemNode = null;
        }
    }

    private void HandleLinkSelection(EveSystemLinkModel link)
    {
        SelectedSystemNode = null;

        if (link.Selected)
        {
            SelectedSystemLink = link;
            _blazorDiagram.SendToFront(SelectedSystemLink);
        }
        else
        {
            SelectedSystemLink = null;
        }
    }
    #endregion
        
    #region Diagram Keyboard Events
    private async Task OnDiagramKeyDown(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
{
    await _semaphoreSlim.WaitAsync();
    try
    {
        if (await HandleLinkSystemKey(eventArgs) ||
            await HandleIncrementOrDecrementExtensionKey(eventArgs) ||
            await HandleDeleteKey(eventArgs))
        {
            return;
        }
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
    private async Task<bool> HandleLinkSystemKey(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
    {
        if (eventArgs.Code == "KeyL" && MapId.HasValue && _selectedSystemNodes != null && _selectedSystemNodes.Count == 2)
        {
            if (await IsLinkExist(_selectedSystemNodes.ElementAt(0), _selectedSystemNodes.ElementAt(1)))
            {
                Snackbar.Add("Nodes are already linked", Severity.Warning);
                return false;
            }
            else
            {

                 
                var primaryAccount = await GetPrimaryAccountAsync();
                if (_selectedSystemNodes != null && _selectedSystemNodes.Count >= 2 && primaryAccount != null && !await AddSystemNodeLink(MapId.Value, _selectedSystemNodes.ElementAt(0), _selectedSystemNodes.ElementAt(1), primaryAccount.Id, true))
                {
                    Logger.LogError("Add Wormhole Link db error");
                    Snackbar.Add("Add Wormhole Link db error", Severity.Error);
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    private async Task<bool> HandleIncrementOrDecrementExtensionKey(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
    {
        if ((eventArgs.Code == "NumpadAdd" || eventArgs.Code == "NumpadSubtract" || eventArgs.Code == "ArrowUp" || eventArgs.Code == "ArrowDown") && MapId.HasValue && SelectedSystemNode != null)
        {
            bool res = eventArgs.Code == "NumpadAdd" || eventArgs.Code == "ArrowUp"
                ? await IncrementOrDecrementNodeExtensionNameOnMap(MapId.Value, SelectedSystemNode, true)
                : await IncrementOrDecrementNodeExtensionNameOnMap(MapId.Value, SelectedSystemNode, false);

            if (res)
            {
                SelectedSystemNode.Refresh();
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

    private async Task<bool> HandleDeleteKey(Blazor.Diagrams.Core.Events.KeyboardEventArgs eventArgs)
    {
        if (eventArgs.Code == "Delete")
        {
            if (!MapId.HasValue)
            {
                Logger.LogError("OnDiagramKeyDown, no map selected to delete node or link");
                Snackbar?.Add("No map selected to delete node or link", Severity.Error);
                return false;
            }

            if (_selectedSystemNodes != null && _selectedSystemNodes.Any())
            {
                await HandleNodeDeletion();
                return true;
            }

            if (_selectedSystemLinks != null && _selectedSystemLinks.Any())
            {
                await HandleLinkDeletion();
                return true;
            }
        }
        return false;
    }

    private async Task HandleNodeDeletion()
    {
        if(_selectedSystemNodes==null || !_selectedSystemNodes.Any() || !MapId.HasValue)
        {
            return;
        }

        WHMapperUser[]? accounts= await GetAccountsAsync();
        if(accounts!=null)
        {
            foreach(var account in accounts)
            {
                if(account.Tracking)
                    await TrackerServices.StopTracking(account.Id);
            }
        }

        foreach (var node in _selectedSystemNodes)
        {
            if (node.Locked)
            {
                Snackbar?.Add($"{node.Name} wormhole is locked. You can't remove it.", Severity.Warning);
            }
            else
            {
                if (!await DeletedNodeOnMap(MapId.Value, node))
                    Snackbar?.Add("Remove wormhole node db error", Severity.Error);
            }
        }
        SelectedSystemNode = null;

        if(accounts!=null)
        {
            foreach(var account in accounts)
            {
                if(account.Tracking)
                    await TrackerServices.StartTracking(account.Id);
            }
        }

    }

    private async Task HandleLinkDeletion()
    {
        if(_selectedSystemLinks==null || !_selectedSystemLinks.Any() || !MapId.HasValue)
            return;

        WHMapperUser[]? accounts= await GetAccountsAsync();
        if(accounts!=null)
        {
            foreach(var account in accounts)
            {
                if(account.Tracking)
                    await TrackerServices.StopTracking(account.Id);
            }
        }

        foreach (var link in _selectedSystemLinks)
        {
            if (!await DeletedLinkOnMap(MapId.Value, link))
                Snackbar?.Add("Remove wormhole link db error", Severity.Error);
        }

        _selectedSystemLink = null;

        if(accounts!=null)
        {
            foreach(var account in accounts)
            {
                if(account.Tracking)
                    await TrackerServices.StartTracking(account.Id);
            }
        }
    }
    #endregion

    #endregion

    #region Diagram Mouse Events
    private async Task OnDiagramPointerUp(Blazor.Diagrams.Core.Models.Base.Model? item, Blazor.Diagrams.Core.Events.PointerEventArgs eventArgs)
    {
        if (item == null || item.GetType() != typeof(EveSystemNodeModel))
            return;

        await _semaphoreSlim.WaitAsync();
        try
        {
            var node = (EveSystemNodeModel)item;
            var wh = await DbWHSystems.GetById(node.IdWH);
            if (wh != null)
            {
                await UpdateNodePositionIfNeeded(MapId,node, wh);
            }
            else
            {
                Logger.LogError("On Mouse pointer up, unable to find moved wormhole node db error");
                Snackbar?.Add("Unable to find moved wormhole node db error", Severity.Error);
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

    private async Task UpdateNodePositionIfNeeded(int? mapId,EveSystemNodeModel node, WHSystem wh)
    {
        if(!mapId.HasValue)
        {
            Logger.LogError("UpdateNodePositionIfNeeded mapId is null");
            return;
        }

        if (Math.Abs(wh.PosX - node.Position.X) >= EPSILON || Math.Abs(wh.PosY - node.Position.Y) >= EPSILON)
        {
            wh.PosX = node.Position.X;
            wh.PosY = node.Position.Y;

            if (await DbWHSystems.Update(node.IdWH, wh) == null)
            {
                Snackbar?.Add("Update wormhole node position db error", Severity.Error);
            }

            WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
            if (primaryAccount != null)
            {
                await EveMapperRealTime.NotifyWormholeMoved(primaryAccount.Id, mapId.Value, wh.Id, wh.PosX, wh.PosY);
            }
        }
    }
    #endregion

    #endregion

    #region WHSystemNode Events
    private void OnWHSystemNodeLockedAsync(EveSystemNodeModel whNodeModel)
    {
        if (whNodeModel != null)
        {
            _ = Task.Run(async () =>
            {
                WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
                if (primaryAccount != null)
                {
                    await EveMapperRealTime.NotifyWormholeLockChanged(primaryAccount.Id, whNodeModel.IdWHMap, whNodeModel.IdWH, whNodeModel.Locked);
                }
            });
        }
    }

    private void OnWHSystemStatusChangeAsync(EveSystemNodeModel whNodeModel)
    {
        if (whNodeModel != null)
        {
            _ = Task.Run(async () =>
            {
                WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
                if (primaryAccount != null)
                {
                    await EveMapperRealTime.NotifyWormholeSystemStatusChanged(primaryAccount.Id, whNodeModel.IdWHMap, whNodeModel.IdWH, whNodeModel.SystemStatus);
                }
            });
        }
    }
    #endregion

    #region Diagram Actions

    
    private Task<EveSystemNodeModel?> GetSystemNode(int id)
    {
        if (_blazorDiagram == null || _blazorDiagram.Nodes == null || _blazorDiagram.Nodes.Count == 0)
        {
            Logger.LogWarning("GetNodeById, no node in diagram");
            return Task.FromResult(null as EveSystemNodeModel);
        }
        var res = _blazorDiagram.Nodes
            .FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == id) as EveSystemNodeModel
            ?? null;
        
        return Task.FromResult(res);
    }

    private Task<EveSystemNodeModel?> GetNodeBySolarSystemId(int solarSystemId)
    {
        if (_blazorDiagram == null || _blazorDiagram.Nodes == null || _blazorDiagram.Nodes.Count == 0)
        {
            Logger.LogWarning("GetNodeBySolarSystemId, no node in diagram");
            return Task.FromResult(null as EveSystemNodeModel);
        }
        var res = _blazorDiagram.Nodes
            .FirstOrDefault(x => ((EveSystemNodeModel)x).SolarSystemId == solarSystemId) as EveSystemNodeModel
            ?? null;

        return Task.FromResult(res);
    }

    private Task<EveSystemLinkModel?> GetLink(EveSystemNodeModel src, EveSystemNodeModel target)
    {
        try
        {
            if (src == null || target == null)
            {
                Logger.LogError("LinkExist src or target is null");
                return Task.FromResult(null as EveSystemLinkModel); 
            }

            var whLink = _blazorDiagram.Links.FirstOrDefault(x =>
            ((((EveSystemNodeModel)x.Source!.Model!).IdWH == src.IdWH) && (((EveSystemNodeModel)x.Target!.Model!).IdWH == target.IdWH))
            ||
            ((((EveSystemNodeModel)x.Source!.Model!).IdWH == target.IdWH) && (((EveSystemNodeModel)x.Target!.Model!).IdWH == src.IdWH)));

            return Task.FromResult(whLink as EveSystemLinkModel);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Get Link error");
            return Task.FromResult(null as EveSystemLinkModel); 
        }
    }

    private async Task<bool> IsLinkExist(EveSystemNodeModel src, EveSystemNodeModel target)
    {
        try
        {
            var link = await GetLink(src, target);

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

    private async Task<bool> AddSystemNode(int? mapId,SystemEntity solarSystem,int accountID,char? extension=null,double nodePositionX=0,double nodePositionY=0)
    {
        WHSystem? newWHSystem = null;
        if(!mapId.HasValue)
        {
            Logger.LogError("AddSystemNode mapId is null");
            throw new NullReferenceException("mapId is null");
        }

        if(await GetNodeBySolarSystemId(solarSystem.Id)==null)//location not exist on map
        {
            if(extension.HasValue)
            {
                newWHSystem = await DbWHSystems.Create(new WHSystem(mapId.Value,solarSystem.Id,solarSystem.Name,extension.Value, solarSystem.SecurityStatus, nodePositionX, nodePositionY));
            }
            else
            {
                newWHSystem = await DbWHSystems.Create(new WHSystem(mapId.Value,solarSystem.Id,solarSystem.Name, solarSystem.SecurityStatus, nodePositionX, nodePositionY));
            }

            if (newWHSystem!=null)
            {
                var newSystemNode = await MapperServices.DefineEveSystemNodeModel(newWHSystem);
                newSystemNode.OnLocked += OnWHSystemNodeLockedAsync;
                newSystemNode.OnSystemStatusChanged += OnWHSystemStatusChangeAsync;
                
                _blazorDiagram?.Nodes?.Add(newSystemNode);
                await EveMapperRealTime.NotifyWormoleAdded(accountID,mapId.Value, newWHSystem.Id);
                
                return true;
            }
            else
            {
                Logger.LogError("AddSystemNode db error");
                throw new Exception("AddSystemNode db error");
                
            }
        }

        return false;
    }

    private async Task<bool> AddSystemNode(int? mapId,EveLocation location,int accountID,char? extension=null,double nodePositionX=0,double nodePositionY=0)
    {
        if(!mapId.HasValue)
        {
            Logger.LogError("AddSystemNode mapId is null");
            throw new NullReferenceException("mapId is null");
        }

        if(await GetNodeBySolarSystemId(location.SolarSystemId)==null)//location not exist on map
        {
            var locationEntity = await eveMapperService.GetSystem(location.SolarSystemId);
            if(locationEntity==null)
            {
                Logger.LogError("AddSystemNode locationEntity is null");
                return false;
            }

            return await AddSystemNode(mapId,locationEntity,accountID,extension,nodePositionX,nodePositionY);
            
        }
        return false;
    }
    
    private async Task<bool> AddSystemNodeLink(int? mapId, SystemEntity src, SystemEntity target,int accountID)
    {
        if(!mapId.HasValue)
        {
            Logger.LogError("AddSystemNodeLink mapId is null");
            throw new NullReferenceException("mapId is null");
        }

        EveSystemNodeModel? srcNode = await GetNodeBySolarSystemId(src.Id);
        EveSystemNodeModel? targetNode = await GetNodeBySolarSystemId(target.Id);

        if (srcNode == null || targetNode == null)
        {
            Logger.LogError("CreateLink src or target node is null");
            throw new NullReferenceException("src or target node is null");
        }

        return await AddSystemNodeLink(mapId.Value,srcNode,targetNode,accountID);
    }
    private async Task<bool> AddSystemNodeLink(int mapId, EveSystemNodeModel srcNode, EveSystemNodeModel targetNode,int accountID,bool isManual=false)
    {
        try
        {
            if(srcNode.SolarSystemId==targetNode.SolarSystemId)
            {
                Logger.LogError("CreateLink src and target node are the same");
                return false;
            }

            var newLink = await DbWHSystemLinks.Create(new WHSystemLink(mapId, srcNode.IdWH, targetNode.IdWH));

            if (newLink != null)
            {
                await AddSystemNodeLinkLog(newLink,accountID,isManual);

                _blazorDiagram?.Links?.Add(new EveSystemLinkModel(newLink, srcNode, targetNode));
                await EveMapperRealTime.NotifyLinkAdded(accountID,mapId, newLink.Id);
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


    private async Task<char?> GetTargetExtension(EveLocation? srcNode, EveLocation? targetNode)
    {
        ArgumentNullException.ThrowIfNull(srcNode);
        ArgumentNullException.ThrowIfNull(targetNode);

        int countSameWHClassLink = 0;
        SystemEntity? srcEntity = await eveMapperService.GetSystem(srcNode.SolarSystemId);
        SystemEntity? targetEntity = await eveMapperService.GetSystem(targetNode.SolarSystemId);
        
        //check if HS/LS/NS to HS/LS/NS via WH not gate
        if(srcEntity != null && targetEntity !=null && await MapperServices.IsRouteViaWH(srcEntity, targetEntity)) 
        {
            //get whClass an determine if another connection to another wh with same class exist from previous system. 
            // Increment extension value in that case
            EveSystemType whClass = await MapperServices.GetWHClass(targetEntity);
            var sameWHClassWHList = _blazorDiagram?.Links?.Where(x =>  ((EveSystemNodeModel)(x.Target!.Model!)).SystemType == whClass && ((EveSystemNodeModel)x.Source!.Model!).SolarSystemId == srcEntity.Id);
                
            if(sameWHClassWHList!=null)
                countSameWHClassLink = sameWHClassWHList.Count();
            else
                countSameWHClassLink=0;

            if (countSameWHClassLink > 0)
            {
                return (Char)(Convert.ToUInt16('A') + countSameWHClassLink);
            }
        }
        return null;
    }

    private async Task<bool> AddSystemNodeLinkLog(int whSystemLinkID,int accountID,bool isManual=false)
    {
        Ship? currentShip = null;
        ShipEntity? currentShipInfos = null;

        if(_currentShips.ContainsKey(accountID))
        {
            _currentShips.TryGetValue(accountID,out currentShip);
        }
        
        if(currentShip==null)
        {
            Logger.LogError("AddSystemNodeLinkLog currentShip is null");
            return false;
        }

        currentShipInfos = await eveMapperService.GetShip(currentShip.ShipTypeId);

        WHJumpLog? jumpLog = null;

        if(isManual)
            jumpLog = await DbWHJumpLogs.Create(new WHJumpLog(whSystemLinkID,accountID));
        else
            jumpLog = await DbWHJumpLogs.Create(new WHJumpLog(whSystemLinkID,accountID,currentShip.ShipTypeId,currentShip.ShipItemId,currentShipInfos?.Mass));
        
        if(jumpLog==null)
        {
            Logger.LogError("AddSystemNodeLinkLog jumpLog is null");
            return false;
        }        

        return true;

    }
    private async Task<bool> AddSystemNodeLinkLog(WHSystemLink link,int accoutID,bool isManual=false)
    {
        return await AddSystemNodeLinkLog(link.Id,accoutID,isManual);
    }
    private async Task<bool> DeletedNodeOnMap(int? mapId, EveSystemNodeModel node)
    {
        try
        {
            if (!mapId.HasValue || node == null)
            {
                Logger.LogError("DeletedNodeOnMap map or node is null");
                return false;
            }

            if (await DbWHSystems.DeleteById(node.IdWH))//db link will be automatically delete via db foreignkey cascade
            {                
                WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
                if (primaryAccount != null)
                {
                    await EveMapperRealTime.NotifyWormholeRemoved(primaryAccount.Id, mapId.Value, node.IdWH);
                }
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
    private async Task<bool> DeletedLinkOnMap(int? mapId, EveSystemLinkModel link)
    {
        try
        {
            if (!mapId.HasValue || link == null)
            {
                Logger.LogError("DeletedLinkOnMap map or link is null");
                return false;
            }

            if(await DbWHSystemLinks.DeleteById(link.Id))
            {
                WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
                if (primaryAccount != null)
                {
                    await EveMapperRealTime.NotifyLinkRemoved(primaryAccount.Id, mapId.Value, link.Id);
                }
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
    private async Task<bool> IncrementOrDecrementNodeExtensionNameOnMap(int?  mapId, EveSystemNodeModel node,bool increment)
    {
        try
        {
            if (!mapId.HasValue || node == null)
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
                     WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
                    if (primaryAccount != null)
                    {
                        if(node.NameExtension == null)
                            await EveMapperRealTime.NotifyWormholeNameExtensionChanged(primaryAccount.Id, mapId.Value, wh.Id, null);
                        else
                            await EveMapperRealTime.NotifyWormholeNameExtensionChanged(primaryAccount.Id, mapId.Value, wh.Id, ((node.NameExtension.ToCharArray())[0]));
                    }
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

    #endregion

    #region Tracker Events
    private async Task OnShipChanged(int accountID,Ship? oldShip,Ship newShip)
    {
        if(_currentShips.ContainsKey(accountID))
        {
            while(!_currentShips.TryRemove(accountID, out _))
                await Task.Delay(1);
        }

        while(!_currentShips.TryAdd(accountID,newShip))
            await Task.Delay(1);
    }
    
    private async Task OnSystemChanged(int accountID,  EveLocation? oldLocation, EveLocation newLocation)
    {
        EveSystemNodeModel? srcNode  = null;
        EveSystemNodeModel? targetNode = null;

        await _semaphoreSlim.WaitAsync();
        try
        {
            if(oldLocation!=null)
                srcNode=await GetNodeBySolarSystemId(oldLocation.SolarSystemId);

            targetNode = await GetNodeBySolarSystemId(newLocation.SolarSystemId);

            if(targetNode==null)//location not exist on map
            {   
                //get extension if needed

                char? extension = null;
                if(oldLocation!=null)
                    extension = await GetTargetExtension(oldLocation,newLocation);

                //get placement, shall be refactored to be more dynamic
                double nodePositionX = 0;
                double nodePositionY = 0;
                if(srcNode!=null && srcNode.Position!=null && srcNode.Size!=null)
                {
                    nodePositionX  = srcNode.Position.X + srcNode.Size.Width + 10;
                    nodePositionY = srcNode.Position.Y + srcNode!.Size.Height + 10;
                }

                if(await AddSystemNode(MapId.Value,newLocation,accountID,extension,nodePositionX,nodePositionY))//add system node if system is not already added
                {
                    targetNode = await GetNodeBySolarSystemId(newLocation.SolarSystemId);


                    if(srcNode!=null && targetNode!=null && !await IsLinkExist(srcNode,targetNode))
                    {
                        if(!await AddSystemNodeLink(MapId.Value,srcNode,targetNode,accountID))//create if new target system added from src
                        {
                            Logger.LogError("Add Wormhole Link error");
                            Snackbar?.Add("Add Wormhole Link error", Severity.Error);
                        }
                    }
                }
             }
            else// tartget system already added
            {
                //check if link already exist, if not create if
                if(srcNode!=null && targetNode!=null && !await IsLinkExist(srcNode,targetNode))
                {
                    if(!await AddSystemNodeLink(MapId.Value,srcNode,targetNode,accountID))//create if new target system added from src
                    {
                        Logger.LogError("Add Wormhole Link error");
                        Snackbar?.Add("Add Wormhole Link error", Severity.Error);
                    }
                }
                else//link already exist, just log jump infos
                {
                    if(srcNode!=null && targetNode!=null)
                    {
                        var link = await GetLink(srcNode,targetNode);

                        if(link!=null && !await AddSystemNodeLinkLog(link.Id,accountID))
                        {
                            Logger.LogError("Add Wormhole Link Log error");
                            Snackbar?.Add("Add Wormhole Link Log error", Severity.Error);
                        }
                    }
                }
            }

            //update user position
            CharactereEntity? user= await eveMapperService.GetCharacter(accountID);
            if(user!=null)
            {
                //remove user from old system if exist
                EveSystemNodeModel? userSystem = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.ConnectedUsers.Contains(user.Name));
                if (userSystem != null)
                {
                    await userSystem.RemoveConnectedUser(user.Name);
                    userSystem.Refresh();
                }
                

                if (targetNode != null)
                {
                    await targetNode.AddConnectedUser(user.Name);
                    targetNode.Refresh();
                }
            }

            if(this.MapId.HasValue && targetNode!=null)
            {
                await EveMapperRealTime.NotifyUserPosition(accountID,this.MapId.Value, targetNode.IdWH);
                _blazorDiagram?.SelectModel(targetNode, true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On System Changed. StackTrace: {StackTrace}", ex.StackTrace);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    #endregion

    #region Menu Actions

        #region Selected Node Menu Actions
        private async Task<bool> SetSelectedSystemDestinationWaypoint()
        {
            try
            {
                if(SelectedSystemNode==null)
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }

                var primaryAccount = await GetPrimaryAccountAsync();
                if (primaryAccount == null)
                {
                    Logger.LogError("Set destination waypoint error, no primary account found");
                    return false;
                }

                var token = await TokenProvider.GetToken(primaryAccount.Id.ToString(),true);
                if (token != null)
                {
                    int solarSystemId = SelectedSystemNode.SolarSystemId;
                    if (solarSystemId <= 0)
                    {
                        Logger.LogError("Set destination waypoint error, no solar system id found");
                        return false;
                    }

                    // Set the destination waypoint using the EveServices
                    await EveServices.SetEveCharacterAuthenticatication(token);          
                    await EveServices.UserInterfaceServices.SetWaypoint(solarSystemId,false, true);
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
                if(SelectedSystemNode==null)
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }

                var whSystem = await DbWHSystems.GetById(SelectedSystemNode.IdWH);
                if (whSystem != null && whSystem.Id==SelectedSystemNode.IdWH)
                {
                    whSystem.Locked = !whSystem.Locked;
                    whSystem = await DbWHSystems.Update(whSystem.Id, whSystem);
                    if (whSystem == null)
                    {
                        Logger.LogError("Update lock system status error");
                        return false;
                    }
                    SelectedSystemNode.Locked = whSystem.Locked;
                    SelectedSystemNode.Refresh();
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
        private async Task<bool> SetSelectedSystemStatus(WHSystemStatus systemStatus)
        {
            try
            {
                if(!MapId.HasValue)
                {
                    Logger.LogError("Set system status error, no map selected");
                    return false;
                }


                if(SelectedSystemNode==null)
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }
                int solarSystemId = SelectedSystemNode.SolarSystemId;
                if (solarSystemId>0)
                {
                    var note = await DbNotes.Get(MapId.Value,solarSystemId);

                    if(note == null)
                    {
                        note = await DbNotes.Create(new WHNote(MapId.Value, solarSystemId, systemStatus));
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

                    SelectedSystemNode.SystemStatus = systemStatus;
                    SelectedSystemNode.Refresh();
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
        private async Task<bool> ToggleSelectedSystemLinkEOL()
        {
            if (!MapId.HasValue || SelectedSystemLink == null)
            {
                Logger.LogError("Toggle system link EOL failed: MapId or SelectedSystemLink is null");
                return false;
            }

            try
            {
                var link = await DbWHSystemLinks.GetById(SelectedSystemLink.Id);
                if (link == null)
                {
                    Logger.LogError("Toggle system link EOL failed: Link not found in database");
                    return false;
                }

                link.IsEndOfLifeConnection = !link.IsEndOfLifeConnection;
                var updatedLink = await DbWHSystemLinks.Update(SelectedSystemLink.Id, link);

                if (updatedLink == null)
                {
                    Logger.LogError("Toggle system link EOL failed: Database update error");
                    return false;
                }

                SelectedSystemLink.IsEoL = updatedLink.IsEndOfLifeConnection;
                SelectedSystemLink.Refresh();

                var primaryAccount = await GetPrimaryAccountAsync();
                if (primaryAccount == null)
                {
                    Logger.LogError("Toggle system link EOL failed: Unable to find primary account");
                    return false;
                }

                await EveMapperRealTime.NotifyLinkChanged(
                    primaryAccount.Id,
                    MapId.Value,
                    updatedLink.Id,
                    updatedLink.IsEndOfLifeConnection,
                    updatedLink.Size,
                    updatedLink.MassStatus
                );

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Toggle system link EOL error");
                return false;
            }
        }

        private async Task<bool> SetSelectedSystemLinkStatus(SystemLinkMassStatus massStatus)
        {
            if (!MapId.HasValue || SelectedSystemLink == null)
            {
                Logger.LogError("Set system link status failed: MapId or SelectedSystemLink is null");
                return false;
            }

            try
            {
                var link = await DbWHSystemLinks.GetById(SelectedSystemLink.Id);
                if (link == null)
                {
                    Logger.LogError("Set system link status failed: Link not found in database");
                    return false;
                }

                link.MassStatus = massStatus;
                var updatedLink = await DbWHSystemLinks.Update(SelectedSystemLink.Id, link);

                if (updatedLink == null)
                {
                    Logger.LogError("Set system link status failed: Database update error");
                    return false;
                }

                SelectedSystemLink.MassStatus = updatedLink.MassStatus;
                SelectedSystemLink.Refresh();

                var primaryAccount = await GetPrimaryAccountAsync();
                if (primaryAccount == null)
                {
                    Logger.LogError("Set system link status failed: Unable to find primary account");
                    return false;
                }

                await EveMapperRealTime.NotifyLinkChanged(
                    primaryAccount.Id,
                    MapId.Value,
                    updatedLink.Id,
                    updatedLink.IsEndOfLifeConnection,
                    updatedLink.Size,
                    updatedLink.MassStatus
                );

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Set system link status error");
                return false;
            }
        }

        private async Task<bool> SetSelectedSystemLinkSize(SystemLinkSize size)
        {
            try
            {
                if (MapId.HasValue && SelectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(SelectedSystemLink.Id);
                    if (link != null)
                    {
                        link.Size = size;
                        link = await DbWHSystemLinks.Update(SelectedSystemLink.Id, link);
                        if(link!=null)
                        {
                                //update link size on diagram (refresh link
                            SelectedSystemLink.Size = link.Size;
                            SelectedSystemLink.Refresh();
                            WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
                            if (primaryAccount != null)
                            {
                                await EveMapperRealTime.NotifyLinkChanged(primaryAccount.Id,MapId.Value, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);
                                return true;
                            }
                            else
                            {
                                Logger.LogError("Set system link size, unable to find primary account");
                                return false;
                            }
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
       
        private async Task<bool> OpenSearchAndAddDialog(MouseEventArgs e)
        {
            DialogOptions disableBackdropClick = new DialogOptions()
            {
                BackdropClick=false,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters(); 

            var dialog = await DialogService.ShowAsync<SearchSystem>("Search and Add System Dialog", parameters, disableBackdropClick);
            DialogResult? result = await dialog.Result;

            if (result != null && !result.Canceled && result.Data != null)
            {
                if (MapId.HasValue  && result.Data!=null)
                {
                    SystemEntity solarSystem = (SystemEntity)result.Data;
                    
                    WHMapperUser? primaryAccount = await GetPrimaryAccountAsync();
                    if (primaryAccount != null)
                    {
                        if(await AddSystemNode(MapId.Value,solarSystem,primaryAccount.Id,null,e.ClientX,e.ClientY))
                        {
                            Snackbar?.Add(String.Format("{0} solar system successfully added",solarSystem.Name), Severity.Success);
                        }
                        else
                        {
                            Snackbar?.Add("Add solar system error", Severity.Error);
                        }
                    }
                    else
                    {
                        Logger.LogError("OpenSearchAndAddDialog, unable to find primary account to notify wormhole added");
                        Snackbar?.Add("Unable to find primary account to notify wormhole added", Severity.Warning);
                    }
                }
                else
                {
                    Logger.LogError("OpenSearchAndAddDialog, unable to find selected map to notify wormhole added");
                    Snackbar?.Add("Unable to find selected map to notify wormhole added", Severity.Warning);
                }
            }

            return true;

        }
        #endregion

    #region RealTime Events

    private async Task OnUserOnMapConnected(int accountID, int mapId)
    {
        if (MapId.HasValue && MapId.Value == mapId)
        {
            CharactereEntity? user= await eveMapperService.GetCharacter(accountID);
            if(user==null)
                throw new NullReferenceException("User not found");

            Snackbar?.Add($"{user.Name} are connected", Severity.Info); 
        }
    }

    private async Task OnUserOnMapDisconnected(int accountID, int mapId)
    {
        if (MapId.HasValue && MapId.Value == mapId)
        {
            CharactereEntity? user= await eveMapperService.GetCharacter(accountID);
            if(user==null)
                throw new NullReferenceException("User not found");

            Snackbar?.Add($"{user.Name} are disconnected", Severity.Info);
            await OnUserDisconnected(accountID);
        }
    }

    private async Task OnUserDisconnected(int accountID)
    {   
        await _semaphoreSlim2.WaitAsync();
        try
        {
            CharactereEntity? user= await eveMapperService.GetCharacter(accountID);
            if(user==null)
                throw new NullReferenceException("User not found");

            EveSystemNodeModel? userSystem = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.ConnectedUsers.Contains(user.Name));
            if (userSystem != null)
            {
                await userSystem.RemoveConnectedUser(user.Name);
                userSystem.Refresh();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyUserDisconnected error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnUserPositionChanged(int accountID, int mapId, int wormholeId)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {   
            WHMapperUser[]? accounts= await GetAccountsAsync();
            if(this.MapId.HasValue && this.MapId.Value == mapId && accounts!=null && accounts.FirstOrDefault(x=>x.Id==accountID)==null)
            {
                CharactereEntity? user= await eveMapperService.GetCharacter(accountID);
                if(user==null)
                    throw new NullReferenceException("User not found");

                EveSystemNodeModel? userSystem = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.ConnectedUsers.Contains(user.Name));
                if (userSystem != null && userSystem.IdWH != wormholeId)
                {
                    await userSystem.RemoveConnectedUser(user.Name);
                    userSystem.Refresh();
                }

                EveSystemNodeModel? systemToAddUser = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.IdWH == wormholeId);
                if (systemToAddUser != null && systemToAddUser.ConnectedUsers != null && !systemToAddUser.ConnectedUsers.Contains(user.Name))
                {
                    await systemToAddUser.AddConnectedUser(user.Name);
                    systemToAddUser.Refresh();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyUserPositionChanged error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnWormholeAdded(int accountID,int mapId, int whId)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            WHMapperUser[]? accounts= await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts!=null 
                && accounts.FirstOrDefault(x=>x.Id==accountID)==null 
                && _blazorDiagram?.Nodes.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == whId) == null)
            {
                var wh = await DbWHSystems.GetById(whId);
                if (wh != null)
                {
                    var newSystemNode = await MapperServices.DefineEveSystemNodeModel(wh);
                    newSystemNode.OnLocked += OnWHSystemNodeLockedAsync;
                    newSystemNode.OnSystemStatusChanged += OnWHSystemStatusChangeAsync;
                    _blazorDiagram?.Nodes?.Add(newSystemNode);
                }
                else
                {
                    Logger.LogWarning("On NotifyWormholeAdded, unable to find added wormhole");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeAdded error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }
    private async Task OnWormholeRemoved(int accountID,int mapId, int whId)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            WHMapperUser[]? accounts= await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts!=null 
                && accounts.FirstOrDefault(x=>x.Id==accountID)==null
                && _blazorDiagram?.Nodes.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == whId) != null)
            {
                var node = await GetSystemNode(whId);
                if (node != null)
                {
                    _blazorDiagram?.Nodes?.Remove(node);
               }
                else
                {
                    Logger.LogWarning("On NotifyWormholeRemoved, unable to find removed wormhole");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeRemoved error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnWormholeMoved(int accountID,int mapId, int whId, double posX, double posY)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            WHMapperUser[]? accounts= await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts!=null 
                && accounts.FirstOrDefault(x=>x.Id==accountID)==null
                && _blazorDiagram?.Nodes.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == whId) != null)
            {
                var node = await GetSystemNode(whId);
                if (node != null && node.Position!=null &&
                    ((Math.Abs(node.Position.X - posX) >= EPSILON) || 
                    (Math.Abs(node.Position.Y - posY) >= EPSILON))
                )
                {
                    node?.SetPosition(posX, posY);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeMoved error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnLinkAdded(int accountID,int mapId, int linkId)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            WHMapperUser[]? accounts= await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts!=null 
                && accounts.FirstOrDefault(x=>x.Id==accountID)==null
                && _blazorDiagram?.Links.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linkId) == null)
            {
                var link = await DbWHSystemLinks.GetById(linkId);
                if (link != null)
                {
                    var srcNode = await GetSystemNode(link.IdWHSystemFrom);
                    var targetNode = await GetSystemNode(link.IdWHSystemTo);
                    if (srcNode != null && targetNode != null)
                    {
                        _blazorDiagram?.Links?.Add(new EveSystemLinkModel(link, srcNode, targetNode));
                    }
                    else
                    {
                        Logger.LogWarning("On NotifyLinkAdded, unable to find added link nodes");
                    }
                }
                else
                {
                    Logger.LogWarning("On NotifyLinkAdded, unable to find added link");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyLinkAdded error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnLinkRemoved(int accountID,int mapId, int linkId)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            var accounts = await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts != null 
                && accounts.FirstOrDefault(x => x.Id == accountID) == null
                && _blazorDiagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linkId) != null)
            {
                var link = await DbWHSystemLinks.GetById(linkId);
                if (link != null)
                {
                    var linkToRemove = _blazorDiagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linkId);
                    if (linkToRemove != null)
                    {
                        _blazorDiagram?.Links?.Remove(linkToRemove);
                    }
                    else
                    {
                        Logger.LogWarning("On NotifyLinkRemoved, unable to find removed link");
                    }
                }
                else
                {
                    Logger.LogWarning("On NotifyLinkRemoved, unable to find removed link");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyLinkRemoved error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnLinkChanged(int accountID,int mapId, int linkId, bool isEoL, SystemLinkSize size, SystemLinkMassStatus massStatus)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            var accounts = await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts != null 
                && accounts.FirstOrDefault(x => x.Id == accountID) == null)
            {
                var link = await DbWHSystemLinks.GetById(linkId);
                if (link != null)
                {
                    EveSystemLinkModel? linkToChange = _blazorDiagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linkId) as EveSystemLinkModel;
                    if (linkToChange != null
                        && (linkToChange.IsEoL != isEoL || linkToChange.Size != size || linkToChange.MassStatus != massStatus) // Check if any property has changed
                    )
                    {
                        linkToChange.IsEoL = isEoL;
                        linkToChange.Size = size;
                        linkToChange.MassStatus = massStatus;
                        linkToChange.Refresh();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyLinkChanged error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnWormholeLockChanged(int accountID,int mapId, int whId, bool locked)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            var accounts = await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts != null 
                && accounts.FirstOrDefault(x => x.Id == accountID) == null)
            {
                var node = await GetSystemNode(whId);
                if (node != null && node.Locked!=locked)
                {
                    node.Locked = locked;
                    node.Refresh();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeLockChanged error");
        }
    }

    private async Task OnWormholeSystemStatusChanged(int accountID,int mapId, int whId, WHSystemStatus systemStatus)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            var accounts = await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts != null && accounts.FirstOrDefault(x => x.Id == accountID) == null) 
            {
                var node = await GetSystemNode(whId);
                if (node != null && node.SystemStatus != systemStatus)
                {
                    node.SystemStatus = systemStatus;
                    node.Refresh();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeSystemStatusChanged error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

    private async Task OnWormholeNameExtensionChanged(int accountID,int mapId, int whId, char? extension)
    {
        await _semaphoreSlim2.WaitAsync();
        try
        {
            var accounts = await GetAccountsAsync();
            if (MapId.HasValue && MapId.Value == mapId && accounts != null && accounts.FirstOrDefault(x => x.Id == accountID) == null)
            {
                var node = await GetSystemNode(whId);
                if (node != null && node.NameExtension!=extension.ToString())
                {
                    node.SetNameExtension(extension);
                    node.Refresh();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeNameExtensionChanged error");
        }
        finally
        {
            _semaphoreSlim2.Release();
        }
    }

#endregion

}
