using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Charts;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.SDE;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOnlineUserInfosProvider;
using WHMapper.Services.SDE;
using WHMapper.Services.WHSignature;
using YamlDotNet.Core.Tokens;
using static MudBlazor.Colors;

namespace WHMapper.Pages.Mapper
{
    public partial class Add : Microsoft.AspNetCore.Components.ComponentBase
    {
        

        [Inject]
        private IEveMapperHelper MapperServices { get; set; } = null!;

        [Inject]
        private IEveAPIServices EveServices { get; set; } = null!;

        [Inject]
        private IWHMapRepository DbWHMaps { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private ISDEServices SDEServices { get; set; } = null!;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public BlazorDiagram CurrentDiagram { get; set; } = null!;

        [Parameter]
        public WHMap CurrentWHMap { get; set; } = null!;

        [Parameter]
        public double MouseX { get; set; }

        [Parameter]
        public double MouseY { get; set; }

        private MudForm _form = null!;
        private bool _success = false;

        private HashSet<SDESolarSystem> _systems = null!;
        private string _searchResult = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        private async Task Submit()
        {
            await _form.Validate();

            if (_form.IsValid)
            {
                try
                {
                    if (CurrentWHMap == null || CurrentDiagram==null)//add log and message
                    {
                        Snackbar?.Add("CurrentWHMap or CurrentDiagram is null", Severity.Error);
                        MudDialog.Close(DialogResult.Cancel);
                    }



                    var sdeSolarSystem = _systems.Where(x => x.Name == _searchResult).FirstOrDefault();
               
                    if(CurrentWHMap?.WHSystems.Where(x => x.SoloarSystemId == sdeSolarSystem?.SolarSystemID).FirstOrDefault()!=null)
                    {
                        Snackbar?.Add("Solar System is already added", Severity.Normal);
                        MudDialog.Close(DialogResult.Ok(0));
                    }

                    var solarSystem = await EveServices.UniverseServices.GetSystem(sdeSolarSystem.SolarSystemID);
                    var newWHSystem = await DbWHMaps.AddWHSystem(CurrentWHMap.Id, new WHSystem(solarSystem.SystemId, solarSystem.Name, solarSystem.SecurityStatus, MouseX, MouseY));//change position

                    if (newWHSystem == null)
                    {
                        //Logger.LogError("Add Wormhole db error");
                        Snackbar?.Add("Add Wormhole db error", Severity.Error);
                    }


                    var nodeModel = await MapperServices.DefineEveSystemNodeModel(newWHSystem);
                    CurrentDiagram?.Nodes.Add(nodeModel);

                    Snackbar?.Add(String.Format("{0} solar system successfully added",nodeModel.Name), Severity.Success);
                    MudDialog.Close(DialogResult.Ok(nodeModel.SolarSystemId));

                }
                catch (Exception ex)
                {
                    Snackbar?.Add(ex.Message, Severity.Error);
                    MudDialog.Close(DialogResult.Cancel);
                }
            }
            else
            {
                Snackbar?.Add("Bad solar system name", Severity.Error);
                MudDialog.Close(DialogResult.Cancel);
            }
        }



        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task<IEnumerable<string>?> Search(string value)
        {
           
            if (string.IsNullOrEmpty(value) || SDEServices == null)
                return null;

            _systems =  SDEServices.SearchSystem(value).ToHashSet<SDESolarSystem>();


            if (_systems != null) 
                return _systems.Select(x => x.Name);
            else
                return null; 
        }

        private IEnumerable<string> Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                yield return "The system solar name is required";
                yield break;
            }

            if (value.Length<3)
            {
                yield return "Please enter 3 or more characters";
                yield break;
            }

            if(_systems==null || _systems.Where(x=>x.Name==value).FirstOrDefault()==null)
            {
                yield return "Bad Solar system name";
                yield break;
            }

        }
    }
}

