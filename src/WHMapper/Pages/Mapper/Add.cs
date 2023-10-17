using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Events;
using Microsoft.AspNetCore.Authorization;
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
using WHMapper.Repositories.WHSystems;
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
    [Authorize(Policy = "Access")]
    public partial class Add : Microsoft.AspNetCore.Components.ComponentBase
    {
        private const string MSG_SEARCH_ERROR = "Search System Error";
        private const string MSG_BAD_SOLAR_SYSTEM_NAME_ERROR = "Bad solar system name";
        private const string MSG_ADD_WORHMOLE_DB_ERROR = "Add Wormhole db error";


        [Inject]
        public ILogger<Add> Logger { get; set; } = null!;

        [Inject]
        private IEveMapperHelper MapperServices { get; set; } = null!;

        [Inject]
        private IEveAPIServices EveServices { get; set; } = null!;

        [Inject]
        IWHSystemRepository DbWHSystems { get; set; } = null!;

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
        private bool _searchInProgress = false;

        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        private async Task Submit()
        {
            await _form.Validate();

            if (_form.IsValid)
            {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    
                    if (CurrentWHMap == null || CurrentDiagram==null)//add log and message
                    {
                        Snackbar?.Add("CurrentWHMap or CurrentDiagram is null", Severity.Error);
                        MudDialog.Close(DialogResult.Cancel);
                    }


                    var sdeSolarSystem = _systems.Where(x => x.Name.ToLower() == _searchResult.ToLower()).FirstOrDefault();
               
                    if(CurrentWHMap?.WHSystems.Where(x => x.SoloarSystemId == sdeSolarSystem?.SolarSystemID).FirstOrDefault()!=null)
                    {
                        Snackbar?.Add("Solar System is already added", Severity.Normal);
                        MudDialog.Close(DialogResult.Ok(0));
                    }

                    var solarSystem = await EveServices.UniverseServices.GetSystem(sdeSolarSystem.SolarSystemID);
                    var newWHSystem = await DbWHSystems.Create(new WHSystem(CurrentWHMap.Id,solarSystem.SystemId, solarSystem.Name, solarSystem.SecurityStatus, MouseX, MouseY)); //change position


                    if (newWHSystem == null)
                    {
                        Logger.LogError(MSG_ADD_WORHMOLE_DB_ERROR);
                        Snackbar?.Add(MSG_ADD_WORHMOLE_DB_ERROR, Severity.Error);
                        return;
                    }


                   
                    var nodeModel = await MapperServices.DefineEveSystemNodeModel(newWHSystem);
                    CurrentWHMap.WHSystems.Add(newWHSystem);
                    CurrentDiagram?.Nodes.Add(nodeModel);

                    Snackbar?.Add(String.Format("{0} solar system successfully added",nodeModel.Name), Severity.Success);
                    MudDialog.Close(DialogResult.Ok(newWHSystem.Id));

                }
                catch (Exception ex)
                {
                    Snackbar?.Add(ex.Message, Severity.Error);
                    MudDialog.Close(DialogResult.Cancel);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
            else
            {
                Logger.LogError(MSG_BAD_SOLAR_SYSTEM_NAME_ERROR);
                Snackbar?.Add(MSG_BAD_SOLAR_SYSTEM_NAME_ERROR, Severity.Error);
                MudDialog.Close(DialogResult.Cancel);
            }
        }


        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task<IEnumerable<string>?> Search(string value)
        {
            try
            {

                if (string.IsNullOrEmpty(value) || SDEServices == null || value.Length < 3 || _searchInProgress)
                    return null;

                _systems?.Clear();
                _searchInProgress = true;

                _systems = (HashSet<SDESolarSystem>)await SDEServices.SearchSystem(value);

                _searchInProgress = false;
                if (_systems != null)
                    return _systems.Select(x => x.Name);
                else
                    return null;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, MSG_SEARCH_ERROR);
                Snackbar.Add(MSG_SEARCH_ERROR, Severity.Error);
                return null;
            }
}

        private IEnumerable<string> Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _searchInProgress = false;
                yield return "The system solar name is required";
                yield break;
            }

            if (value.Length<3)
            {
                _searchInProgress = false;
                yield return "Please enter 3 or more characters";
                yield break;
            }

            if(_systems==null || _systems.Where(x=>x.Name.ToLower() == value.ToLower()).FirstOrDefault()==null)
            {
                _searchInProgress = false;
                yield return "Bad Solar system name";
                yield break;
            }

        }
    }
}

