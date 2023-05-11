﻿using Blazor.Diagrams.Core;
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
using Microsoft.AspNetCore.Components.Web;
using WHMapper.Services.WHSignature;
using MudBlazor.Services;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using WHMapper.Pages.Mapper.Signatures;
using System.Diagnostics.Tracing;
using WHMapper.Services.EveMapper;

namespace WHMapper.Pages.Mapper
{
    public partial class Overview : ComponentBase, IAsyncDisposable
    {
        protected BlazorDiagram Diagram { get; private set; } = null!;
        private MudMenu ClickRightMenu { get; set; } = null!;
        private WHMapper.Pages.Mapper.Signatures.Overview WHSignaturesView { get; set; } = null!;


        private EveLocation _currentLocation = null!;
        private EveSystemNodeModel _currentSystemNode = null!;
        private SolarSystem _currentSolarSystem = null!;

        private int _currentWHSystemId = 0;
        private EveSystemNodeModel? _selectedSystemNode = null;
        private EveSystemLinkModel? _selectedSystemLink = null;

        private ICollection<EveSystemNodeModel>? _selectedSystemNodes = null;
        private ICollection<EveSystemLinkModel>? _selectedSystemLinks = null;

        private PeriodicTimer _timer=null!;
        private CancellationTokenSource _cts= null!;

        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

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
        IEveAPIServices EveServices { get; set; } = null!;

        [Inject]
        IAnoikServices AnoikServices { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject] IWHSignatureHelper SignatureHelper { get; set; } = null!; 

        [Inject]
        public NavigationManager Navigation { get; set; } = null!;

        [Inject]
        public ILogger<EveAPIServices> Logger { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private IEveMapperHelper MapperServices { get; set; } = null!;

        private string _userName = string.Empty;

        private IEnumerable<WHMap> WHMaps { get; set; } = new List<WHMap>();
        private WHMap _selectedWHMap = null!;

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
                             options.AccessTokenProvider = () => Task.FromResult(TokenProvider.AccessToken);
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

                    _hubConnection.On<string>("NotifyUserDisconnected", async (user) =>
                    {
                        try
                        {
                            EveSystemNodeModel userSystem = (EveSystemNodeModel)Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).ConnectedUsers.Contains(user));
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
                                EveSystemNodeModel systemToAddUser = (EveSystemNodeModel)Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).Title == item.Value);
                                await systemToAddUser.AddConnectedUser(item.Key);
                                systemToAddUser.Refresh();
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
                                Diagram?.Nodes?.Add(newSystemNode);
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
                            if (wormholeId > 0 && mapId == _selectedWHMap?.Id)
                            {
                                var sustemNodeToDelete = Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
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
                            if (linKId > 0 && mapId == _selectedWHMap?.Id)
                            {

                                var link = await DbWHSystemLinks.GetById(linKId);
                                while (link == null)
                                    link = await DbWHSystemLinks.GetById(linKId);

                                var whFrom = await DbWHSystems.GetById(link.IdWHSystemFrom);
                                while (whFrom == null)
                                    whFrom = await DbWHSystems.GetById(link.IdWHSystemFrom);


                                var whTo = await DbWHSystems.GetById(link.IdWHSystemTo);
                                while (whTo == null)
                                    whTo = await DbWHSystems.GetById(link.IdWHSystemTo);

                                if (whFrom != null && whTo != null)
                                {
                                    EveSystemNodeModel newSystemNodeFrom = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whFrom.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;
                                    EveSystemNodeModel newSystemNodeTo = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whTo.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;
                                    Diagram?.Links?.Add(new EveSystemLinkModel(link, newSystemNodeFrom, newSystemNodeTo));
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyLinkAdded error");
                        }
                    });

                    _hubConnection.On<string, int, int>("NotifyLinkRemoved", (user, mapId, linKId) =>
                    {
                        try
                        {
                            if (linKId > 0 && mapId == _selectedWHMap?.Id)
                            {
                                var linkToDel = Diagram?.Links?.FirstOrDefault(x => ((EveSystemLinkModel)x).Id == linKId);
                                if (linkToDel != null)
                                    Diagram?.Links?.Remove(linkToDel);

                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyLinkRemoved error");
                        }
                    });

                    _hubConnection.On<string, int, int, double, double>("NotifyWormoleMoved", (user, mapId, wormholeId, posX, posY) =>
                    {
                        try
                        {
                            if (wormholeId > 0 && mapId == _selectedWHMap?.Id)
                            {
                                var whToMoved = Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                                if (whToMoved != null)
                                    whToMoved.SetPosition(posX, posY);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormoleMoved error");
                        }
                    });

                    _hubConnection.On<string, int, int, bool, SystemLinkSize, SystemLinkMassStatus>("NotifyLinkChanged", (user, mapId, linkId, eol, size, mass) =>
                    {
                        try
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
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyLinkChanged error");
                        }
                    });

                    _hubConnection.On<string, int, int>("NotifyWormholeNameExtensionChanged", (user, mapId, wormholeId) =>
                    {
                        try
                        {
                            if (wormholeId > 0 && mapId == _selectedWHMap?.Id)
                            {
                                EveSystemNodeModel systemToIncrementNameExtenstion = (EveSystemNodeModel)Diagram?.Nodes?.FirstOrDefault(x => ((EveSystemNodeModel)x).IdWH == wormholeId);
                                if (systemToIncrementNameExtenstion != null)
                                    systemToIncrementNameExtenstion.Refresh();
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
                            if (_selectedWHMap!=null && _selectedSystemNode!=null && wormholeId > 0 && mapId == _selectedWHMap.Id)
                            {
                                if (_selectedSystemNode.IdWH == wormholeId)
                                {
                                    await WHSignaturesView.Restore();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "On NotifyWormholeSignaturesChanged error");
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

        private async Task NotifyWormholeNameExtensionChanged(int mapId, int wormholeId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeNameExtensionChanged", mapId, wormholeId);
            }
        }

        private async Task NotifyWormholeSignaturesChanged(int mapId, int wormholeId)
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.SendAsync("SendWormholeSignaturesChanged", mapId, wormholeId);
            }
        }

        #endregion

        private async Task<bool> InitDiagram()
        {
            try
            {
                Logger.LogInformation("Start Init Diagram");
               /* var options = new BlazorDiagramOptions
                { 
                    AllowMultiSelection = false// Whether to allow multi selection using CTRL
                };*/


                Diagram = new BlazorDiagram();
                Diagram.SelectionChanged += async (item) => OnDiagramSelectionChanged(item);
                
                Diagram.KeyDown += async (kbevent) => OnDiagramKeyDown(kbevent);
                Diagram.PointerUp +=  async (item, pointerEvent) => OnDiagramPointerUp(item, pointerEvent);

                Diagram.Options.Zoom.Enabled = true;
                Diagram.Options.Zoom.Inverse = false;
                Diagram.Options.Links.EnableSnapping = false;
                Diagram.Options.AllowMultiSelection = true;
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

        private async Task OnDiagramSelectionChanged(Blazor.Diagrams.Core.Models.Base.SelectableModel? item)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (item == null)
                    return;

                var selectedModels = Diagram.GetSelectedModels();
                _selectedSystemNodes = selectedModels.Where(x => x.GetType() == typeof(EveSystemNodeModel)).Select(x => (EveSystemNodeModel)x).ToList();
                _selectedSystemLinks = selectedModels.Where(x => x.GetType() == typeof(EveSystemLinkModel)).Select(x => (EveSystemLinkModel)x).ToList();


                if (item.GetType() == typeof(EveSystemNodeModel))
                {
                    _selectedSystemLink = null;

                    if (((EveSystemNodeModel)item).Selected)
                        _selectedSystemNode = (EveSystemNodeModel)item;
                    else
                        _selectedSystemNode = null;

                    StateHasChanged();
                    return;

                }

                if (item.GetType() == typeof(EveSystemLinkModel))
                {
                    _selectedSystemNode = null;

                    if (((EveSystemLinkModel)item).Selected)
                        _selectedSystemLink = (EveSystemLinkModel)item;
                    else
                        _selectedSystemLink = null;

                    
                    StateHasChanged();
                    return;
                }
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
            if (eventArgs.Code == "KeyL" && _selectedWHMap != null && _selectedSystemNodes != null && _selectedSystemNodes.Count() == 2)
            { 
            
                if (IsLinkExist(_selectedSystemNodes.ElementAt(0), _selectedSystemNodes.ElementAt(1)))
                {
                    Snackbar.Add("Nodes are already linked", Severity.Warning);
                    return false;
                }
                else
                {
                    if (!await CreateLink(_selectedWHMap, _selectedSystemNodes.ElementAt(0), _selectedSystemNodes.ElementAt(1)))
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
                if (_selectedWHMap != null)
                {
                    Cancel();
                    try
                    {

                        if (_selectedSystemNode != null && _selectedSystemNodes != null && _selectedSystemNodes.Count() > 0)
                        {
                            foreach (var node in _selectedSystemNodes)
                            {
                                if (!await DeletedNodeOnMap(_selectedWHMap, node))
                                    Snackbar?.Add("Remove wormhole node db error", Severity.Error);
                                else
                                {
                                    if (_selectedSystemNode?.IdWH == node.IdWH)
                                    {
                                        _currentLocation = null;
                                        _currentSystemNode = null;
                                        _currentWHSystemId = -1;
                                        _currentSolarSystem = null;
                                    }
                                    _selectedSystemNode = null;
                                }

                            }
                            StateHasChanged();
                            return true;
                        }

                        if (_selectedSystemLink != null && _selectedSystemLinks != null && _selectedSystemLinks.Count() > 0)
                        {
                            foreach (var link in _selectedSystemLinks)
                            {
                                if (!await DeletedLinkOnMap(_selectedWHMap, link))
                                    Snackbar?.Add("Remove wormhole link db error", Severity.Error);
                                else
                                {

                                    if (_selectedSystemLink?.Id == link.Id)
                                        _selectedSystemLink = null;
                                }

                            }
                            StateHasChanged();
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "OnDiagramKeyDown, on delete error");
                        return false;
                    }
                    finally
                    {
                        HandleTimerAsync();
                    }

                }
                else
                {
                    Logger.LogError("OnDiagramKeyDown, no map selected to delete node or link");
                    Snackbar?.Add("No map selected to delete node or link", Severity.Error);
                    return false;
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
                            await NotifyWormholeMoved(_selectedWHMap.Id, wh.Id, wh.PosX, wh.PosY);
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
                        EveSystemNodeModel whSysNode = await MapperServices.DefineEveSystemNodeModel(dbWHSys);
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
                        await AutoMapper();
                    else
                        _cts.Cancel();//todo redirect to logout
                }
            }
            catch(OperationCanceledException oce)
            {
                Logger.LogInformation(oce, "Cancel operation");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,"Polling error");
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


                if (await DbWHMaps.RemoveWHSystem(map.Id, node.IdWH) != null)
                {
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

                if (await DbWHMaps.RemoveWHSystemLink(map.Id, ((EveSystemNodeModel)link.Source.Model).IdWH, ((EveSystemNodeModel)link.Target.Model).IdWH) != null)
                {
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
                        await node.IncrementNameExtension();
                    else
                        await node.DecrementNameExtension();

                    if (node.NameExtension == null)
                        wh.NameExtension = 0;
                    else
                        wh.NameExtension = ((byte)((node.NameExtension.ToCharArray())[0]));

                    wh = await DbWHSystems.Update(wh.Id, wh);


                    if (wh != null)
                    {
                        await NotifyWormholeNameExtensionChanged(map.Id, wh.Id);
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
    
        private async Task<bool> CreateLink(WHMap map, EveSystemNodeModel src, EveSystemNodeModel target)
        {
            try
            {
                if (map == null || src == null || target == null)
                {
                    Logger.LogError("CreateLink map or src or target is null");
                    return false;
                }


                var link = await DbWHMaps.AddWHSystemLink(map.Id, src.IdWH, target.IdWH);

                if (link != null)
                {
                    Diagram.Links.Add(new EveSystemLinkModel(link, src, target));
                    await this.NotifyLinkAdded(map.Id, link.Id);

                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Create link error");
                return false;
            }
        }

        private bool IsLinkExist(EveSystemNodeModel src, EveSystemNodeModel target)
        {
            try
            {
                if (src == null || target == null)
                {
                    Logger.LogError("IsLinkExist src or target is null");
                    return false;
                }

                //check if link exist, if not create it
                var whLink = Diagram.Links.FirstOrDefault(x =>
                ((((EveSystemNodeModel)x.Source.Model).IdWH == src.IdWH) && (((EveSystemNodeModel)x.Target.Model).IdWH == target.IdWH))
                ||
                ((((EveSystemNodeModel)x.Source.Model).IdWH == target.IdWH) && (((EveSystemNodeModel)x.Target.Model).IdWH == src.IdWH)));


                if (whLink == null)
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

        private async Task<bool> IsRouteViaWH(SolarSystem src, SolarSystem dst)
        {
            if (src == null)
                return false;

            if (dst == null)
                return false;


            if((src != null && src.Stargates==null) || dst.Stargates==null)
                return true;
            else
            {
                int[]? startgatesToCheck = null;
                int systemTarget = -1;
                if (src!=null && src.Stargates.Length <= dst.Stargates.Length)
                {
                    startgatesToCheck = dst.Stargates;
                    systemTarget = src.SystemId;
                }
                else
                {
                    startgatesToCheck = src.Stargates;
                    systemTarget = dst.SystemId;
                }

                foreach (int sgId in startgatesToCheck)
                {
                    var sg = await EveServices.UniverseServices.GetStargate(sgId);

                    if (sg.Destination.SystemId == systemTarget)
                        return false;
                }

                 return true;
            }
        }

        private async Task AutoMapper()
        {
            EveSystemNodeModel? previousSystem = null;
            SolarSystem?  previousSolarSystem = null;
            WHSystem? newWHSystem = null;
            double defaultNewSystemPosX = 0;
            double defaultNewSystemPosY = 0;
            await _semaphoreSlim.WaitAsync();
            try
            {
                EveLocation el = await EveServices.LocationServices.GetLocation();
                
                if (el != null && (_currentLocation == null || _currentLocation.SolarSystemId != el.SolarSystemId) )
                {
                    previousSystem = _currentSystemNode;
                    previousSolarSystem = _currentSolarSystem;
                    _currentLocation = el;

                    _currentSolarSystem = await EveServices.UniverseServices.GetSystem(el.SolarSystemId);

                    if (Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, _currentSolarSystem.Name, StringComparison.OrdinalIgnoreCase)) == null)
                    {
                        //determine position on map. depends of previous system
                        if (previousSystem != null)
                        {
                            defaultNewSystemPosX  = (double)previousSystem.Position.X + previousSystem.Size.Width + 10;
                            defaultNewSystemPosY = (double)previousSystem.Position.Y + previousSystem.Size.Height + 10;
                        }

                        //determine if source have same system link and get next unique ident
                        if(previousSystem != null && await IsRouteViaWH(previousSolarSystem, _currentSolarSystem))
                        {
                            var whClass = AnoikServices.GetSystemClass(_currentSolarSystem.Name);
                            if (!string.IsNullOrEmpty(whClass))
                            {
                                int nbSameWHClassLink = Diagram.Links.Where(x => ((EveSystemNodeModel)x.Target.Model).Class.Contains(whClass) && ((EveSystemNodeModel)x.Source.Model).IdWH == previousSystem.IdWH).Count();

                                if (nbSameWHClassLink > 0)
                                {
                                    char extension = (Char)(Convert.ToUInt16('A') + nbSameWHClassLink);
                                    newWHSystem = await DbWHMaps.AddWHSystem(_selectedWHMap.Id, new WHSystem(_currentSolarSystem.SystemId, _currentSolarSystem.Name, extension, _currentSolarSystem.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                                }
                                else
                                    newWHSystem = await DbWHMaps.AddWHSystem(_selectedWHMap.Id, new WHSystem(_currentSolarSystem.SystemId, _currentSolarSystem.Name, _currentSolarSystem.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                            }
                            else
                            {
                                newWHSystem = await DbWHMaps.AddWHSystem(_selectedWHMap.Id, new WHSystem(_currentSolarSystem.SystemId, _currentSolarSystem.Name, _currentSolarSystem.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));
                            }

                        }
                        else
                         newWHSystem = await DbWHMaps.AddWHSystem(_selectedWHMap.Id, new WHSystem(_currentSolarSystem.SystemId,_currentSolarSystem.Name, _currentSolarSystem.SecurityStatus, defaultNewSystemPosX, defaultNewSystemPosY));


                           
                        if (newWHSystem!=null)
                        {
                            var newSystemNode = await MapperServices.DefineEveSystemNodeModel(newWHSystem);
                            await newSystemNode.AddConnectedUser(_userName);

                            Diagram.Nodes.Add(newSystemNode);
                            await this.NotifyWormoleAdded(_selectedWHMap.Id, newWHSystem.Id);
                            await this.NotifyUserPosition(_currentSolarSystem.Name);

                            if (previousSystem != null)
                            {
                                if(!await CreateLink(_selectedWHMap, previousSystem, newSystemNode))
                                {
                                    Logger.LogError("Add Wormhole Link db error");
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
                            Logger.LogError("Add Wormhole db error");
                            Snackbar?.Add("Add Wormhole db error", Severity.Error);
                        }
                    }
                    else
                    {
                        await this.NotifyUserPosition(_currentSolarSystem.Name);
                        if (previousSystem != null)
                        {
                            await previousSystem.RemoveConnectedUser(_userName);
                            previousSystem.Refresh();
                        }
                            
                        _currentSystemNode = (EveSystemNodeModel)Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, _currentSolarSystem.Name, StringComparison.OrdinalIgnoreCase));
                        await _currentSystemNode.AddConnectedUser(_userName);
                        _currentSystemNode.Refresh();

                        _currentWHSystemId = (await DbWHSystems.GetByName(_currentSolarSystem.Name)).Id;
                        _selectedSystemNode = _currentSystemNode;

                        if (previousSystem != null)
                        {
                            if(IsLinkExist(previousSystem, _currentSystemNode)==false)
                            {
                                if (!await CreateLink(_selectedWHMap, previousSystem, _currentSystemNode))
                                {
                                    Logger.LogError("Add Wormhole Link db error");
                                    Snackbar?.Add("Add Wormhole Link db error", Severity.Error);
                                }
                            }
                        } 
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Auto mapper error");
            }
            finally
            {
                _semaphoreSlim.Release();
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

        /// <summary>
        /// Menu intercep paste
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        private async Task HandleCustomPaste(CustomPasteEventArgs eventArgs)
        {
            if(_selectedSystemNode!=null)
            {
                try
                {
                    String scanUser = await UserInfos.GetUserName();
                    String? message = eventArgs.PastedData;
                    if (await SignatureHelper.ImportScanResult(scanUser, _selectedSystemNode.IdWH, message))
                    {
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


        public async Task<bool> ToggleSlectedSystemLinkEOL()
        {
            try
            {
                if (_selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.IsEndOfLifeConnection = !link.IsEndOfLifeConnection;
                        link = await DbWHSystemLinks.Update(_selectedSystemLink.Id, link);
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
                Logger.LogError(ex, "Toggle system link eol error");
                return false;
            }
        }

        public async Task<bool> SetSelectedSystemLinkStatus(SystemLinkMassStatus massStatus)
        {
            try
            {
                if (_selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.MassStatus = massStatus;
                        link = await DbWHSystemLinks.Update(_selectedSystemLink.Id, link);
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
                Logger.LogError(ex, "System link status error");
                return false;
            }
        }

        public async Task<bool> SetSelectedSystemLinkSize(SystemLinkSize size)
        {
            try
            {
                if (_selectedSystemLink != null)
                {
                    WHSystemLink? link = await DbWHSystemLinks.GetById(_selectedSystemLink.Id);
                    if (link != null)
                    {
                        link.Size = size;
                        link = await DbWHSystemLinks.Update(_selectedSystemLink.Id, link);
                        _selectedSystemLink.Size = link.Size;
                        ClickRightMenu.CloseMenu();
                        _selectedSystemLink.Refresh();
                        await NotifyLinkChanged(_selectedWHMap.Id, link.Id, link.IsEndOfLifeConnection, link.Size, link.MassStatus);

                        return true;

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

        public async Task<bool> SetSelectedSystemDestinationWaypoint()
        {
            try
            {
                if(_selectedSystemNode !=null)
                {
                    var res = await EveServices.UserInterfaceServices.SetWaypoint(_selectedSystemNode.SolarSystemId,false,true);
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

        

        private async Task<bool> OpenSearchAndAddDialog(MouseEventArgs args)
        {
            DialogOptions disableBackdropClick = new DialogOptions()
            {
                DisableBackdropClick = true,
                Position = DialogPosition.Center,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };
            var parameters = new DialogParameters(); 
            parameters.Add("CurrentDiagram", Diagram);
            parameters.Add("CurrentWHMap", _selectedWHMap);
            parameters.Add("MouseX", args.ClientX);
            parameters.Add("MouseY", args.ClientY);

            var dialog = DialogService.Show<Add>("Search and Add System Dialog", parameters, disableBackdropClick);
            DialogResult result = await dialog.Result;

            if (!result.Cancelled)
            {
                int whAddedId = (int)result.Data;
                if(whAddedId>0)
                    await this.NotifyWormoleAdded(_selectedWHMap.Id, whAddedId);
            }

            return true;

        }
        #endregion
    }
}