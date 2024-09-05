

using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveAPI;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;
using MudBlazor;
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

namespace WHMapper.Pages.Mapper;

[Authorize(Policy = "Access")]
public partial class Overview : ComponentBase, IAsyncDisposable
{
    private IEnumerable<WHMap>? WHMaps { get; set; } = new List<WHMap>();

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
            }
        }
    }

    private bool _loading = true;

    [Inject]
    IEveMapperRealTimeService? RealTimeService {get;set;} = null!;


    private WHMapper.Pages.Mapper.Signatures.Overview WHSignaturesView { get; set; } = null!;


    private HubConnection _hubConnection = null!;

    [Inject]
    TokenProvider TokenProvider { get; set; } = null!;

    [Inject]
    IEveUserInfosServices UserInfos { get; set; } = null!;

    [Inject]
    IWHMapRepository DbWHMaps { get; set; } = null!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public ILogger<Overview> Logger { get; set; } = null!;

    [Inject]
    private IEveMapperTracker TrackerServices { get; set; } = null!;

    [Inject]
    private IPasteServices PasteServices { get; set; } = null!;



    private string _userName = string.Empty;
    private int _characterId = 0;

    private WHMap? _selectedWHMap = null!;


    protected override async Task OnInitializedAsync()
    {
        _userName = await UserInfos.GetUserName();
        _characterId = await UserInfos.GetCharactedID();

        if (!await RestoreMaps())
        {
            Snackbar?.Add("Mapper Initialization error", Severity.Error);
        }
        if(!await InitRealTimeService())
        {
            Snackbar?.Add("RealTimeService Initialization error", Severity.Error);
        }

    
        _loading = false;
        await base.OnInitializedAsync();
    }


    private void InitPasteServices()
    {
        //PasteServices.Pasted += OnPasted;
    }

    private async Task<bool> RestoreMaps()
    {
        WHMaps = await DbWHMaps.GetAll();
        if (WHMaps == null || !WHMaps.Any())
        {
            _selectedWHMap = await DbWHMaps.Create(new WHMap("Default Maps"));
            if (_selectedWHMap != null)
            {
                WHMaps = await DbWHMaps.GetAll();
            }
        }
        else
        {
            _selectedWHMap = WHMaps.FirstOrDefault();
        }
        
        return _selectedWHMap != null;
    }

    public async ValueTask DisposeAsync()
    {
        if (TrackerServices != null)
        {
            await TrackerServices.StopTracking();

        }

        if(RealTimeService!=null)
        {
            await RealTimeService.Stop();
            await RealTimeService.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private async Task<bool> InitRealTimeService()
    {
        try
        {
            if(RealTimeService==null) 
            {
                Logger.LogError("RealTimeService is null");
                return false;
            }

            RealTimeService.UserConnected += OnUserConnected;
            RealTimeService.UserDisconnected+=OnUserDisconnected;

            return await RealTimeService.Start();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "InitRealTimeService error");
            return false;
        }
    } 
    
    private async Task OnUserConnected(string user)
    {
        try
        {
            Snackbar?.Add($"{user} are connected", Severity.Info);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyUserConnected error");
        }
    }

    private async Task OnUserDisconnected(string user)
    {
        
        try
        {
            Snackbar?.Add($"{user} are disconnected", Severity.Info);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyUserDisconnected error");
        }
    }



    #region Hub methodes
   


    private async Task<bool> InitNotificationHubold()
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
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "On NotifyUserConnected error");
                    }
                });

                _ = _hubConnection.On<string>("NotifyUserDisconnected", async (user) =>
                {
                    /*
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
                    }*/
                });

                _hubConnection.On<string, string>("NotifyUserPosition", async (user, systemName) =>
                {
                    /*
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
                    */
                });

                _hubConnection.On<IDictionary<string, string>>("NotifyUsersPosition", async (usersPosition) =>
                {
                    /*
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
                    }*/
                });

                _hubConnection.On<string, int, int>("NotifyWormoleAdded", async (user, mapId, wormholeId) =>
                {
                    /*
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
                    }*/

                });

                _hubConnection.On<string, int, int>("NotifyWormholeRemoved", async (user, mapId, wormholeId) =>
                {
                    /*
                    try
                    {
                        if (DbWHMaps != null && _selectedWHMap != null && wormholeId > 0 && _selectedWHMap.Id == mapId)
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
                    }*/
                });

                _hubConnection.On<string, int, int>("NotifyLinkAdded", async (user, mapId, linKId) =>
                {
                    /*
                    try
                    {
                        if (DbWHMaps != null && linKId > 0 && _selectedWHMap != null && _selectedWHMap?.Id == mapId)
                        {
                            _selectedWHMap = await DbWHMaps.GetById(mapId);
                            var link = _selectedWHMap?.WHSystemLinks.Where(x => x.Id == linKId).SingleOrDefault();
                            if (link != null)
                            {
                                EveSystemNodeModel? newSystemNodeFrom = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => (x as EveSystemNodeModel)!.IdWH == link.IdWHSystemFrom));
                                EveSystemNodeModel? newSystemNodeTo = (EveSystemNodeModel?)(_blazorDiagram?.Nodes?.FirstOrDefault(x => (x as EveSystemNodeModel)!.IdWH == link.IdWHSystemTo));
                                if (newSystemNodeTo != null && newSystemNodeFrom != null)
                                    _blazorDiagram?.Links?.Add(new EveSystemLinkModel(link, newSystemNodeFrom, newSystemNodeTo));
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
                    }*/
                });

                _hubConnection.On<string, int, int>("NotifyLinkRemoved", async (user, mapId, linKId) =>
                {
                    /*
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
                    }*/
                });

                _hubConnection.On<string, int, int, double, double>("NotifyWormoleMoved", async (user, mapId, wormholeId, posX, posY) =>
                {
                    /*
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
                    }*/
                });

                _hubConnection.On<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus>("NotifyLinkChanged", async (user, mapId, linkId, eol, size, mass) =>
                {
                    /*
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
                    */
                });

                _hubConnection.On<string, int, int, bool>("NotifyWormholeNameExtensionChanged", async (user, mapId, wormholeId, increment) =>
                {
                    /*
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
                    }*/

                });

                _hubConnection.On<string, int, int>("NotifyWormholeSignaturesChanged", async (user, mapId, wormholeId) =>
                {
                    try
                    {
                        if (DbWHMaps != null && _selectedWHMap != null && wormholeId > 0 && _selectedWHMap.Id == mapId)
                        {
                            _selectedWHMap = await DbWHMaps.GetById(mapId);
                            if (WHSignaturesView != null && WHSignaturesView.CurrentSystemNodeId == wormholeId)
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
                    /*
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
                    */
                });

                _hubConnection.On<string, int, int, WHSystemStatus>("NotifyWormholeSystemStatusChanged", async (user, mapId, wormholeId, systemStatus) =>
                {
                    /*
                    try
                    {
                        if (_blazorDiagram != null && _selectedWHMap != null && wormholeId > 0 && mapId == _selectedWHMap.Id)
                        {
                            _selectedWHMap = await DbWHMaps.GetById(mapId);
                            EveSystemNodeModel? whChangeSystemStatus = _blazorDiagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId) as EveSystemNodeModel;
                            if (whChangeSystemStatus != null)
                            {
                                whChangeSystemStatus.SystemStatus = systemStatus;
                                whChangeSystemStatus.Refresh();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "On NotifyWormholeLockChanged error");
                    }*/

                });

                await _hubConnection.StartAsync();

                return true;
            }
            return false;
        }
        catch (Exception ex)
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

    private async Task NotifyWormoleAdded(int mapId, int wormholeId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeAdded", mapId, wormholeId);
        }
    }

    private async Task NotifyWormholeRemoved(int mapId, int wormholeId)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeRemoved", mapId, wormholeId);
        }
    }

    private async Task NotifyLinkAdded(int mapId, int linkId)
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

    private async Task NotifyWormholeMoved(int mapId, int wormholeId, double posX, double posY)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeMoved", mapId, wormholeId, posX, posY);
        }
    }

    private async Task NotifyLinkChanged(int mapId, int linkId, bool eol, SystemLinkSize size, SystemLinkMassStatus mass)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendLinkChanged", mapId, linkId, eol, size, mass);
        }
    }

    private async Task NotifyWormholeNameExtensionChanged(int mapId, int wormholeId, bool increment)
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

    private async Task NotifyWormholeSystemStatusChanged(int mapId, int wormholeId, WHSystemStatus systemStatus)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendWormholeSystemStatusChanged", mapId, wormholeId, systemStatus);
        }
    }

    #endregion

    /*
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
    }*/

}