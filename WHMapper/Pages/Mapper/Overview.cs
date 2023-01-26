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

namespace WHMapper.Pages.Mapper
{
    public partial class Overview : ComponentBase
    {
        protected BlazorDiagram Diagram { get; private set; }

        private EveLocation? _currentLocation = null;
        private EveSystemNodeModel? _currentSystemNode = null;
        private int _currentWHSystemId = 0;
        private EveSystemNodeModel? _selectedSystemNode = null;
        private PeriodicTimer? _timer;

        private static SemaphoreSlim semSlim = new SemaphoreSlim(1, 1);

        private CancellationTokenSource? _cts;

        [Inject]
        AuthenticationStateProvider? AuthState { get; set; }

        [Inject]
        IWHMapRepository? DbWHMaps { get; set; }

        [Inject]
        IWHSystemRepository? DbWHSystems { get; set; }

        [Inject]
        IEveAPIServices? EveServices { get; set; }

        [Inject]
        IAnoikServices? AnoikServices { get; set; }

        [Inject]
        public ISnackbar? Snackbar { get; set; }


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

        protected override async Task OnInitializedAsync()
        {
            _loading = true;
            await base.OnInitializedAsync();
        }

        

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if(firstRender)
            {

                var options = new BlazorDiagramOptions
                {
                    AllowMultiSelection = false, // Whether to allow multi selection using CTRL
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
                Diagram.SelectionChanged += (item) =>
                {
                    if (item.GetType() == typeof(EveSystemNodeModel))
                    {
                        if (((EveSystemNodeModel)item).Selected)
                            _selectedSystemNode = (EveSystemNodeModel)item;
                        else
                            _selectedSystemNode = null;

                        StateHasChanged();
                    }
                };


                Diagram.Nodes.Removed += async (item) =>
                {
                    if (item.GetType() == typeof(EveSystemNodeModel))
                    {
                        try
                        {
                            Cancel();
                            await semSlim.WaitAsync();
                            if (await DbWHMaps?.RemoveWHSystem(_selectedWHMap.Id, ((EveSystemNodeModel)item).IdWH) == null)
                            {
                                Snackbar?.Add("Remove wormhole node db error", Severity.Error);
                            }

                        }
                        finally
                        {
                            semSlim.Release();
                            HandleTimerAsync();
                        }
                    }
                };
        
                Diagram.Links.Removed += async (item) =>
                {
                    if (item.GetType() == typeof(LinkModel))
                    {
                        try
                        {
                            Cancel();
                            await semSlim.WaitAsync();

                            if (await DbWHMaps?.RemoveWHSystemLink(_selectedWHMap.Id,
                                ((EveSystemNodeModel)((LinkModel)item).Source.Model).IdWH,
                                ((EveSystemNodeModel)((LinkModel)item).Target.Model).IdWH) == null)
                            {
                                Snackbar?.Add("Remove wormhole link db error", Severity.Error);
                            }

                        }
                        finally
                        {
                            semSlim.Release();
                            HandleTimerAsync();
                        }
                    }
                };

                Diagram.PointerUp += async (item, mouseEvent) =>
                {
                    if(item!=null && (item.GetType() == typeof(EveSystemNodeModel)))
                    {
                        try
                        {
                            Cancel();
                            await semSlim.WaitAsync();
                            var wh = await DbWHSystems?.GetById(((EveSystemNodeModel)item).IdWH);
                            if(wh!=null)
                            {
                                wh.PosX = ((EveSystemNodeModel)item).Position.X;
                                wh.PosY = ((EveSystemNodeModel)item).Position.Y;

                                if(await DbWHSystems?.Update(((EveSystemNodeModel)item).IdWH,wh)==null)
                                {
                                    Snackbar?.Add("Update wormhole node position db error", Severity.Error);
                                }
                            }
                            else
                            {
                                Snackbar?.Add("Unable to find moved wormhole node dd error", Severity.Error);
                            }
                            
                        }
                        finally
                        {
                            semSlim.Release();
                            HandleTimerAsync();
                        }
                    }
                };

                Diagram.Options.Zoom.Enabled = false;
                Diagram.RegisterComponent<EveSystemNodeModel, EveSystemNode>();
                Diagram.RegisterComponent<EveSystemLinkModel, EveSystemLink>();


                await Restore();
                HandleTimerAsync();

                _loading = false;
                StateHasChanged();
            }

        }

   
        private async Task Restore()
        {
            
            WHMaps = await DbWHMaps?.GetAll();
            if (WHMaps == null || WHMaps.Count() == 0)
            {
                _selectedWHMap = await DbWHMaps.Create(new WHMap("Default Maps"));
                if(_selectedWHMap!=null)
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

                    EveSystemNodeModel newSystemNodeFrom = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whFrom.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;
                    EveSystemNodeModel newSystemNodeTo = Diagram?.Nodes?.FirstOrDefault(x => string.Equals(x.Title, whTo.Name, StringComparison.OrdinalIgnoreCase)) as EveSystemNodeModel;

                    Diagram.Links.Add(new EveSystemLinkModel(newSystemNodeFrom, newSystemNodeTo));

                    /*
                    Diagram.Links.Add(new LinkModel(newSystemNodeFrom, newSystemNodeTo)
                    {
                        Router = Routers.Normal,
                        PathGenerator = PathGenerators.Smooth
                    });*/

                }
                
            }
            StateHasChanged();
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
                        _cts.Cancel();
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
       

        private async Task GetCharacterPositionInSpace()
        {
            await semSlim.WaitAsync();
            try
            {

                EveLocation el = await EveServices.LocationServices.GetLocation();
                if (el != null && (_currentLocation == null || _currentLocation.SolarSystemId != el.SolarSystemId) )
                {
                    _currentLocation = el;
                    var newSystem = await EveServices.UniverseServices.GetSystem(_currentLocation.SolarSystemId);
               
                
                    if (Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, newSystem.Name, StringComparison.OrdinalIgnoreCase)) == null)
                    {

                        WHSystem? newWHSystem = await DbWHMaps?.AddWHSystem(_selectedWHMap.Id, new WHSystem(newSystem.Name, newSystem.SecurityStatus));

                        if(newWHSystem!=null)
                        {

                            var newSystemNode = await DefineEveSystemNodeModel(newWHSystem);
                            Diagram.Nodes.Add(newSystemNode);


                            if (_currentSystemNode != null)
                            {
                                var lk = await DbWHMaps.AddWHSystemLink(_selectedWHMap.Id, _currentWHSystemId, newWHSystem.Id);

                                if (lk != null)
                                {
                                    newSystemNode.SetPosition(_currentSystemNode.Position.X + 10, _currentSystemNode.Position.Y + 10);
                                    Diagram.Links.Add(new EveSystemLinkModel(_currentSystemNode, newSystemNode));
                                    /*{
                                        Router = Routers.Normal,
                                        PathGenerator = PathGenerators.Smooth,
                                       
                                    });*/

                                }
                                else//add snack error  link added
                                {
                                    Snackbar?.Add("Add Wormhole Link db error", Severity.Error);
                                }
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
                        _currentSystemNode = (EveSystemNodeModel)Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, newSystem.Name, StringComparison.OrdinalIgnoreCase));
                        _currentWHSystemId = (await DbWHSystems.GetByName(newSystem.Name)).Id;
                        _selectedSystemNode = _currentSystemNode;
                    }
                }
            }
            finally
            {
                semSlim.Release();
            }
        }


        public void Cancel() => _cts?.Cancel();

        public ValueTask DisposeAsync()
        {
            Cancel();
            _timer?.Dispose();
            
            GC.SuppressFinalize(this);
            return new ValueTask();
        }
    }
}


