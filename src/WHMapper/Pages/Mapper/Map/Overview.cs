using Blazor.Diagrams;
using Blazor.Diagrams.Core.Behaviors;
using BlazorContextMenu;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Pages.Mapper.CustomNode;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOnlineUserInfosProvider;

namespace WHMapper.Pages.Mapper.Map;

public partial class Overview : ComponentBase,IAsyncDisposable
{
    private const float EPSILON = 0.0001f; // or some other small value
    private BlazorDiagram _blazorDiagram  = null!;
    private int? _selectedWHMapId = null;

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

    private SystemEntity? _currentSolarSystem = null!;
    private Ship? _currentShip = null!;
    private ShipEntity? _currentShipInfos = null!;

    private string _userName = string.Empty;
    private int _characterId = 0;

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
    IEveUserInfosServices UserInfos { get; set; } = null!;

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
    

    protected override async Task OnInitializedAsync()
    {
        _userName = await UserInfos.GetUserName();
        _characterId = await UserInfos.GetCharactedID();

        if(!await InitDiagram())
        {
            Snackbar?.Add("Map Initialization error", Severity.Error);
        }

        await base.OnInitializedAsync();
    }


    protected override async Task OnParametersSetAsync()
    {
        if(MapId.HasValue && (!_selectedWHMapId.HasValue || _selectedWHMapId.Value!=MapId.Value))
        {
            if(await Restore(MapId.Value))
            {   
                TrackerServices.SystemChanged += OnSystemChanged;
                TrackerServices.ShipChanged += OnShipChanged;

                EveMapperRealTime.UserDisconnected += OnUserDisconnected;
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

                IDictionary<string,string> usersPosition = await EveMapperRealTime.GetConnectedUsersPosition();
                try
                {
                    await Parallel.ForEachAsync(usersPosition, async (item, cancellationToken) =>
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            EveSystemNodeModel? systemToAddUser = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).Title == item.Value);
                            if (systemToAddUser != null)
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
                
                await TrackerServices.StartTracking();
                await InitBlazorDiagramEvents();
            }
            else
            {
                Snackbar?.Add("Map Restore error", Severity.Error);
            }
            
        }
        await base.OnParametersSetAsync();
    }  

    public async ValueTask DisposeAsync()
    {
        if(TrackerServices!=null)
        {
            await TrackerServices.StopTracking();
            TrackerServices.SystemChanged-=OnSystemChanged;
            TrackerServices.ShipChanged-=OnShipChanged;

        }

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
                await ClearDiagram();
                await InitializeSystemNodes(selectedWHMap);
                await InitializeSystemLinks(selectedWHMap);
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
        if (selectedWHMap != null && selectedWHMap.WHSystems != null)
        {
            foreach (var dbWHSys in selectedWHMap.WHSystems)
            {
                EveSystemNodeModel whSysNode = await MapperServices.DefineEveSystemNodeModel(dbWHSys);
                whSysNode.OnLocked += OnWHSystemNodeLocked;
                whSysNode.OnSystemStatusChanged += OnWHSystemStatusChange;
                _blazorDiagram.Nodes.Add(whSysNode);
            }
        }
    }

    private async Task InitializeSystemLinks(WHMap? selectedWHMap)
    {
        if (selectedWHMap?.WHSystemLinks == null || selectedWHMap.WHSystemLinks.Count == 0)
        {
            return;
        }

        foreach (var dbWHSysLink in selectedWHMap.WHSystemLinks)
        {
            var srcNode = await GetSystemNode(dbWHSysLink.IdWHSystemFrom);
            var targetNode = await GetSystemNode(dbWHSysLink.IdWHSystemTo);

            if (srcNode != null && targetNode != null)
            {
                _blazorDiagram.Links.Add(new EveSystemLinkModel(dbWHSysLink, srcNode, targetNode));
            }
            else
            {
                Logger.LogWarning("Bad Link, srcNode or Targetnode is null, Auto remove");
                await DbWHSystemLinks.DeleteById(dbWHSysLink.Id);
        }
        }
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

                if (item is EveSystemNodeModel node)
                {
                    HandleNodeSelection(node);
                }
                else if (item is EveSystemLinkModel link)
                {
                    HandleLinkSelection(link);
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

       // StateHasChanged();
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
                if (!await AddSystemNodeLink(MapId.Value, _selectedSystemNodes.ElementAt(0), _selectedSystemNodes.ElementAt(1), true))
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

            if (SelectedSystemNode != null && _selectedSystemNodes != null && _selectedSystemNodes.Count > 0)
            {
                await HandleNodeDeletion();
                return true;
            }

            if (SelectedSystemLink != null && _selectedSystemLinks != null && _selectedSystemLinks.Count > 0)
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

        await TrackerServices.StopTracking();
        _currentSolarSystem = null;
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
        await TrackerServices.StartTracking();
    }

    private async Task HandleLinkDeletion()
    {
        if(_selectedSystemLinks==null || !_selectedSystemLinks.Any() || !MapId.HasValue)
            return;

        await TrackerServices.StopTracking();
        foreach (var link in _selectedSystemLinks)
        {
            if (!await DeletedLinkOnMap(MapId.Value, link))
                Snackbar?.Add("Remove wormhole link db error", Severity.Error);
        }
        _selectedSystemLink = null;
        await TrackerServices.StartTracking();
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

            await EveMapperRealTime.NotifyWormholeMoved(mapId.Value, wh.Id, wh.PosX, wh.PosY);
        }
    }
    #endregion

    #endregion

    #region WHSystemNode Events
    private void OnWHSystemNodeLocked(EveSystemNodeModel whNodeModel)
    {
        if (whNodeModel != null)
        {
            Task.Run(async () => await EveMapperRealTime.NotifyWormholeLockChanged(whNodeModel.IdWHMap, whNodeModel.IdWH, whNodeModel.Locked));
        }
    }

    private void OnWHSystemStatusChange(EveSystemNodeModel whNodeModel)
    {
        if (whNodeModel != null)
        {
            Task.Run(async ()=>await EveMapperRealTime.NotifyWormholeSystemStatusChanged(whNodeModel.IdWHMap, whNodeModel.IdWH, whNodeModel.SystemStatus));
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

    private async Task<bool> AddSystemNode(int? mapId,SystemEntity node,double nodePositionX=0,double nodePositionY=0)
    {
        return await AddSystemNode(mapId, null, node,nodePositionX,nodePositionY,true);
    }

    private async Task<bool> AddSystemNode(int? mapId,SystemEntity? src,SystemEntity target,double nodePositionX=0,double nodePositionY=0,bool isManual=false)
    {
        EveSystemNodeModel? previousSystemNode = null;
        WHSystem? newWHSystem= null;
        double defaultNewSystemPosX = nodePositionX;
        double defaultNewSystemPosY = nodePositionY;
        char extension;
        int nbSameWHClassLink =0;
        
        try
        {
            if(!mapId.HasValue)
            {
                Logger.LogError("AddSystemNode mapId is null");
                return false;
            }

            //determine position on map. depends of previous system , todo refactor
            if (src != null)
            {
                previousSystemNode = await GetNodeBySolarSystemId(src.Id);
                        
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
                    extension = (Char)(Convert.ToUInt16('A') + nbSameWHClassLink);
                    newWHSystem = await DbWHSystems.Create(new WHSystem(mapId.Value, target.Id, target.Name, extension, target.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                }
                else
                    newWHSystem = await DbWHSystems.Create(new WHSystem(mapId.Value, target.Id, target.Name, target.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
            
            }
            else
                newWHSystem = await DbWHSystems.Create(new WHSystem(mapId.Value,target.Id,target.Name, target.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));

            if (newWHSystem!=null)
            {
                var newSystemNode = await MapperServices.DefineEveSystemNodeModel(newWHSystem);
                newSystemNode.OnLocked += OnWHSystemNodeLocked;
                newSystemNode.OnSystemStatusChanged += OnWHSystemStatusChange;
                
                if(!isManual)
                {
                    await newSystemNode.AddConnectedUser(_userName);
                }

                _blazorDiagram?.Nodes?.Add(newSystemNode);
                await EveMapperRealTime.NotifyWormoleAdded(mapId.Value, newWHSystem.Id);
                
                
                if (previousSystemNode != null && !isManual)
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
                Logger.LogError("AddSystemNode db error");
                return false;
            }

            
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "AddSystemNode error");
            return false;
        }
    }
    private async Task<bool> AddSystemNodeLink(int? mapId, SystemEntity src, SystemEntity target)
    {
        if (_blazorDiagram == null  || !mapId.HasValue || src == null || target == null)
        {
            Logger.LogError("CreateLink map or src or target is null");
            return false;
        }

        EveSystemNodeModel? srcNode = await GetNodeBySolarSystemId(src.Id);
        EveSystemNodeModel? targetNode = await GetNodeBySolarSystemId(target.Id);

        return await AddSystemNodeLink(mapId.Value,srcNode,targetNode);
    }
    private async Task<bool> AddSystemNodeLink(int mapId, EveSystemNodeModel? srcNode, EveSystemNodeModel? targetNode,bool isManual=false)
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

            var newLink = await DbWHSystemLinks.Create(new WHSystemLink(mapId, srcNode.IdWH, targetNode.IdWH));

            if (newLink != null)
            {
                await AddSystemNodeLinkLog(newLink,isManual);

                _blazorDiagram?.Links?.Add(new EveSystemLinkModel(newLink, srcNode, targetNode));
                await EveMapperRealTime.NotifyLinkAdded(mapId, newLink.Id);
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
    private async Task<bool> AddSystemNodeLinkLog(int whSystemLinkID,bool isManual=false)
    {
        if(_currentShip==null || _currentShipInfos==null)
        {
            Logger.LogError("AddSystemNodeLinkLog currentShip or currentShipInfos is null");
            return false;
        }
        WHJumpLog? jumpLog = null;

        if(isManual)
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
                await EveMapperRealTime.NotifyWormholeRemoved(mapId.Value, node.IdWH);
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
                await EveMapperRealTime.NotifyLinkRemoved(mapId.Value, link.Id);
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
                    await EveMapperRealTime.NotifyWormholeNameExtensionChanged(mapId.Value, wh.Id, increment);
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
    private Task OnShipChanged(Ship ship,ShipEntity shipInfos)
    {
        _currentShip=ship;
        _currentShipInfos=shipInfos;

        return Task.CompletedTask;
    }
    private async Task OnSystemChanged(SystemEntity? targetSoloarSystem)
    {
        EveSystemNodeModel? srcNode  = null;
        EveSystemNodeModel? targetNode = null;

        if(_blazorDiagram==null)
        {
            Logger.LogError("Error OnSystemChanged, blazorDiagram is nullable");
            throw new NullReferenceException("Blazor Diagram is null");
        }

        if(MapId.HasValue==false)
        {
            Logger.LogError("Error OnSystemChanged, selectedWHMap is nullable");
            throw new NullReferenceException("Selected WH Map is null");
        }

        if(targetSoloarSystem==null)
        {
            Logger.LogError("Error OnSystemChanged, targetSoloarSystem is nullable");
            throw new NullReferenceException("Target Solar System is null");
        }

        if(_currentSolarSystem!=null && _currentSolarSystem.Id==targetSoloarSystem.Id)
        {
            Logger.LogWarning("On System Changed, target system is the same as current system");
            return;
        }

        await _semaphoreSlim.WaitAsync();
        try
        {
            
            if(_currentSolarSystem!=null)
            {
                srcNode=await GetNodeBySolarSystemId(_currentSolarSystem.Id);
            }
            targetNode = await GetNodeBySolarSystemId(targetSoloarSystem.Id);
           

            if (targetNode== null)//System is not added
            {
                if(await AddSystemNode(MapId.Value,_currentSolarSystem,targetSoloarSystem))//add system node if system is not added
                {
                    if (_currentSolarSystem != null)
                    {
                        if(!await AddSystemNodeLink(MapId.Value, _currentSolarSystem, targetSoloarSystem))//create if new target system added from src
                        {
                            Logger.LogError("Add Wormhole Link error");
                            Snackbar?.Add("Add Wormhole Link error", Severity.Error);
                        }
                        else
                        {
                            targetNode = await GetNodeBySolarSystemId(targetSoloarSystem.Id);
                        }
                    }

                    await EveMapperRealTime.NotifyUserPosition(targetSoloarSystem.Name);
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
                if(srcNode!=null && !await IsLinkExist(srcNode,targetNode))
                {
                    if((_currentSolarSystem==null) || (!await AddSystemNodeLink(MapId.Value, _currentSolarSystem, targetSoloarSystem)))//create if new target system added from src
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
                await EveMapperRealTime.NotifyUserPosition(targetSoloarSystem.Name);
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
                int solarSystemId = SelectedSystemNode.SolarSystemId;
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
                if(SelectedSystemNode==null)
                {
                    Logger.LogError("Set system status error, no node selected");
                    return false;
                }
                int solarSystemId = SelectedSystemNode.SolarSystemId;
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
        private async Task<bool> ToggleSlectedSystemLinkEOL()
        {
            try
            {
                if (MapId.HasValue && SelectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(SelectedSystemLink.Id);
                    if (link != null)
                    {
                        link.IsEndOfLifeConnection = !link.IsEndOfLifeConnection;
                        link = await DbWHSystemLinks.Update(SelectedSystemLink.Id, link);
                        if(link!=null)
                        {
                            SelectedSystemLink.IsEoL = link.IsEndOfLifeConnection;
                            SelectedSystemLink.Refresh();
                            await EveMapperRealTime.NotifyLinkChanged(MapId.Value, link.Id, link.IsEndOfLifeConnection, link.Size,link.MassStatus);
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
                if (MapId.HasValue && SelectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(SelectedSystemLink.Id);
                    if (link != null)
                    {
                        link.MassStatus = massStatus;
                        link = await DbWHSystemLinks.Update(SelectedSystemLink.Id, link);
                        if(link!=null)
                        {
                            SelectedSystemLink.MassStatus = link.MassStatus;
                            SelectedSystemLink.Refresh();
                            await EveMapperRealTime.NotifyLinkChanged(MapId.Value, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);
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
                            await EveMapperRealTime.NotifyLinkChanged(MapId.Value, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);

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
                    if(await AddSystemNode(MapId.Value,solarSystem,e.MouseEvent.ClientX,e.MouseEvent.ClientY))
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
                    Logger.LogError("OpenSearchAndAddDialog, unable to find selected map to notify wormhole added");
                    Snackbar?.Add("Unable to find selected map to notify wormhole added", Severity.Warning);
                }
            }

            return true;

        }
        #endregion

    #region RealTime Events
    private async Task OnUserDisconnected(string user)
    {   
        try
        {
            EveSystemNodeModel? userSystem = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.ConnectedUsers.Contains(user));
            if (userSystem != null)
            {
                await userSystem.RemoveConnectedUser(user);
                userSystem.Refresh();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyUserDisconnected error");
        }
    }
    private async Task OnUserPositionChanged(string user, string systemName)
    {
        try
        {
            EveSystemNodeModel? userSystem = (EveSystemNodeModel?)_blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x)!.ConnectedUsers.Contains(user));
            if (userSystem != null)
            {
                await userSystem.RemoveConnectedUser(user);
                userSystem.Refresh();
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
            Logger.LogError(ex, "On NotifyUserPositionChanged error");
        }
    }

    private async Task OnWormholeAdded(string user,int mapId, int whId)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
            {
                var wh = await DbWHSystems.GetById(whId);
                if (wh != null)
                {
                    var newSystemNode = await MapperServices.DefineEveSystemNodeModel(wh);
                    newSystemNode.OnLocked += OnWHSystemNodeLocked;
                    newSystemNode.OnSystemStatusChanged += OnWHSystemStatusChange;
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
    }
    private async Task OnWormholeRemoved(string user,int mapId, int whId)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
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
    }

    private async Task OnWormholeMoved(string user,int mapId, int whId, double posX, double posY)
    {
        try
        {
             if (MapId.HasValue && MapId.Value == mapId)
            {
                var node = await GetSystemNode(whId);
                if (node != null)
                {
                    node.SetPosition(posX, posY);
                }
                else
                {
                    Logger.LogWarning("On NotifyWormholeMoved, unable to find moved wormhole");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeMoved error");
        }
    }

    private async Task OnLinkAdded(string user,int mapId, int linkId)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
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
    }

    private async Task OnLinkRemoved(string user,int mapId, int linkId)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
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
    }

    private async Task OnLinkChanged(string user,int mapId, int linkId, bool isEoL, SystemLinkSize size, SystemLinkMassStatus massStatus)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
            {
                var link = await DbWHSystemLinks.GetById(linkId);
                if (link != null)
                {
                    var linkToChange = _blazorDiagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linkId);
                    if (linkToChange != null)
                    {
                        ((EveSystemLinkModel)linkToChange).IsEoL = isEoL;
                        ((EveSystemLinkModel)linkToChange).Size = size;
                        ((EveSystemLinkModel)linkToChange).MassStatus = massStatus;
                        linkToChange.Refresh();
                    }
                    else
                    {
                        Logger.LogWarning("On NotifyLinkChanged, unable to find changed link");
                    }
                }
                else
                {
                    Logger.LogWarning("On NotifyLinkChanged, unable to find changed link");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyLinkChanged error");
        }
    }

    private async Task OnWormholeLockChanged(string user,int mapId, int whId, bool locked)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
            {
                var node = await GetSystemNode(whId);
                if (node != null)
                {
                    node.Locked = locked;
                    node.Refresh();
                }
                else
                {
                    Logger.LogWarning("On NotifyWormholeLockChanged, unable to find changed wormhole");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeLockChanged error");
        }
    }

    private async Task OnWormholeSystemStatusChanged(string user,int mapId, int whId, WHSystemStatus systemStatus)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
            {
                var node = await GetSystemNode(whId);
                if (node != null)
                {
                    node.SystemStatus = systemStatus;
                    node.Refresh();
                }
                else
                {
                    Logger.LogWarning("On NotifyWormholeSystemStatusChanged, unable to find changed wormhole");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeSystemStatusChanged error");
        }
    }

    private async Task OnWormholeNameExtensionChanged(string user,int mapId, int whId, bool increment)
    {
        try
        {
            if (MapId.HasValue && MapId.Value == mapId)
            {
                var node = await GetSystemNode(whId);
                if (node != null)
                {
                    if (increment)
                        node.IncrementNameExtension();
                    else
                        node.DecrementNameExtension();
                    node.Refresh();
                }
                else
                {
                    Logger.LogWarning("On NotifyWormholeNameExtensionChanged, unable to find changed wormhole");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyWormholeNameExtensionChanged error");
        }
    }

#endregion

}
