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

namespace WHMapper.Pages.Mapper
{
    public partial class Overview : ComponentBase
    {
        protected Diagram Diagram { get; private set; }

        private EveLocation? _currentLocation = null;
        private EveSystemNodeModel? _currentSystemNode = null;
        private int _currentWHSystemId = 0;
        private EveSystemNodeModel? _selectedSystemNode = null;
        private PeriodicTimer? _timer; 

        private CancellationTokenSource? _cts;

        [Inject]
        IWHMapRepository DbWHMaps { get; set; }

        [Inject]
        IWHSignature DbWHSystems { get; set; }

        [Inject]
        IEveAPIServices EveServices { get; set; }

        [Inject]
        IAnoikServices AnoikServices { get; set; }


        private IEnumerable<WHMap>? WHMaps { get; set; }
        private WHMap _selectedWHMap = null;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var options = new DiagramOptions
            {
                DefaultNodeComponent = null, // Default component for nodes
                AllowMultiSelection = false, // Whether to allow multi selection using CTRL
                Links = new DiagramLinkOptions
                {
                },
                Zoom = new DiagramZoomOptions
                {
                    Minimum = 0.25, // Minimum zoom value
                    Inverse = false, // Whether to inverse the direction of the zoom when using the wheel
                },

            };

            Diagram = new Diagram(options);
            Diagram.SelectionChanged += (item) =>
            {
                if (item.GetType() == typeof(EveSystemNodeModel))
                {
                    _selectedSystemNode = (EveSystemNodeModel)item;
                }
                StateHasChanged();
            };

            Diagram.RegisterModelComponent<EveSystemNodeModel, EveSystemNode>();

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if(firstRender)
            {

                WHMaps = await DbWHMaps.GetAll();
                if (WHMaps == null || WHMaps.Count() == 0)
                    _selectedWHMap = await DbWHMaps.Create(new WHMap("Default Maps"));
                else
                    _selectedWHMap = WHMaps.FirstOrDefault();


                //todo init maps if system registered
                if (_selectedWHMap.WHSystems.Count > 0)
                {
                     foreach (WHSystem dbWHSys in _selectedWHMap.WHSystems)
                     {

                        Diagram.Nodes.Add(await DefineEveSystemNodeModel(dbWHSys.Name, dbWHSys.SecurityStatus));

                    }
                    StateHasChanged();
                }

                if (_selectedWHMap.WHSystemLinks.Count > 0)
                {
                    foreach (WHSystemLink dbWHSysLink in _selectedWHMap.WHSystemLinks)
                    {
                        var whFrom = await DbWHSystems.GetById(dbWHSysLink.IdWHSystemFrom);
                        var whTo = await DbWHSystems.GetById(dbWHSysLink.IdWHSystemTo);

                        var newSystemNodeFrom = Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, whFrom.Name, StringComparison.OrdinalIgnoreCase));
                        var newSystemNodeTo =  Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, whTo.Name, StringComparison.OrdinalIgnoreCase));

                        Diagram.Links.Add(new LinkModel(newSystemNodeFrom, newSystemNodeTo)
                        {
                            SourceMarker = LinkMarker.Circle,
                            TargetMarker = LinkMarker.Circle,
                        });

                    }
                    StateHasChanged();
                }



                HandleTimerAsync(/*_timer, _cts.Token*/);
            }

        }

        private async Task HandleTimerAsync(/*PeriodicTimer timer, CancellationToken cancel = default*/)   
        {
            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    await GetCharacterPositionInSpace();
                }
            }
            catch (Exception ex)
            {
                //Handle the exception but don't propagate it
            }
        }

        private async Task<EveSystemNodeModel> DefineEveSystemNodeModel(string name,float securityStatus)
        {
            if (securityStatus <= -0.9)
            {

                var whClass = await AnoikServices.GetSystemClass(name);
                var whEffect = await AnoikServices.GetSystemEffects(name);
                var whStatics = await AnoikServices.GetSystemStatics(name);
                return new EveSystemNodeModel(name, securityStatus, whClass, whEffect, whStatics);

            }
            else
            {
                return new EveSystemNodeModel(name, securityStatus);
            }
        }


        private async Task GetCharacterPositionInSpace()
        {
            EveLocation el = await EveServices.LocationServices.GetLocation();
            if ((_currentLocation == null || _currentLocation.SolarSystemId != el.SolarSystemId) && el!=null)
            {
                _currentLocation = el;
                SolarSystem newSystem = await EveServices.UniverseServices.GetSystem(_currentLocation.SolarSystemId);
                //Star newStar = await EveOnlineServices.UniverseServices.GetStar(newSystem.StarId);

                
                if (Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, newSystem.Name, StringComparison.OrdinalIgnoreCase)) == null)
                {

                    EveSystemNodeModel newSystemNode = await DefineEveSystemNodeModel(newSystem.Name,newSystem.SecurityStatus);

                    // New WH System Discover, need to add to Diagram,Db,Botify to other people
                    Diagram.Nodes.Add(newSystemNode);
                    //WHSystem newHWSys = new WHSystem(newSystemNode.Name);
                    //_selectedWHMap?.WHSystems.Add(newHWSys);

                    WHSystem newWHSystem = await DbWHMaps.AddWHSystem(_selectedWHMap.Id, new WHSystem(newSystemNode.Name, newSystemNode.SecurityStatus));
                    

                    if (_currentSystemNode != null)
                    {
                        Diagram.Links.Add(new LinkModel(_currentSystemNode, newSystemNode)
                        {
                            SourceMarker = LinkMarker.Circle,
                            TargetMarker = LinkMarker.Circle,
                        });


                       
                        await DbWHMaps.AddWHSystemLink(_selectedWHMap.Id, _currentWHSystemId, newWHSystem.Id);

                        //Diagram.Links.Add(new LinkModel(_currentSystemNode.GetPort(PortAlignment.Right), newSolarNode.GetPort(PortAlignment.Left)));
                        //Diagram.ReconnectLinksToClosestPorts();
                    }
                    _currentSystemNode = newSystemNode;
                    _currentWHSystemId = newWHSystem.Id;
                    _selectedSystemNode = _currentSystemNode;
                    StateHasChanged();
                }
                else
                {
                    _currentSystemNode = (EveSystemNodeModel)Diagram.Nodes.FirstOrDefault(x => string.Equals(x.Title, newSystem.Name, StringComparison.OrdinalIgnoreCase));
                    _currentWHSystemId = (await DbWHSystems.GetByName(newSystem.Name)).Id;
                    _selectedSystemNode = _currentSystemNode;
                    StateHasChanged();
                }
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


