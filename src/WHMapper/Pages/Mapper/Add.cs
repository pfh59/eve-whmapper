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
using WHMapper.Models.DTO.EveMapper.EveEntity;
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
    public partial class Add :ComponentBase
    {
        private const string MSG_SEARCH_ERROR = "Search System Error";
        private const string MSG_BAD_SOLAR_SYSTEM_NAME_ERROR = "Bad solar system name";
        private const string MSG_ADD_WORHMOLE_DB_ERROR = "Add Wormhole db error";


        [Inject]
        public ILogger<Add> Logger { get; set; } = null!;

        [Inject]
        private IEveMapperHelper MapperServices { get; set; } = null!;

        [Inject]
        private IEveMapperEntity EveMapperEntity { get; set; } = null!;

        [Inject]
        IWHSystemRepository DbWHSystems { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;


        [Inject]
        private IEveMapperSearch EveMapperSearch { get; set; } = null!;

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

        private SDESolarSystem _searchResult = null!;


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
                        Logger.LogError("CurrentWHMap or CurrentDiagram is null");
                        Snackbar?.Add("CurrentWHMap or CurrentDiagram is null", Severity.Error);
                        MudDialog.Close(DialogResult.Cancel);
                    }

                    if(_searchResult==null)
                    {
                        Logger.LogError("Solar System not found");
                        Snackbar?.Add("Solar System not found", Severity.Error);
                        MudDialog.Close(DialogResult.Ok(0));
                    }

                                    
               
                    if(CurrentWHMap?.WHSystems.Where(x => x.SoloarSystemId == _searchResult?.SolarSystemID).FirstOrDefault()!=null)
                    {
                        Snackbar?.Add("Solar System is already added", Severity.Normal);
                        MudDialog.Close(DialogResult.Cancel);
                    }

                    int solarSystemId = ((_searchResult==null) ? -1 : _searchResult.SolarSystemID);
                    int currentMapId = ((CurrentWHMap == null) ? -1 : CurrentWHMap.Id);
                    SystemEntity? solarSystem = await EveMapperEntity.GetSystem(solarSystemId);
                    

                    WHSystem? newWHSystem=null!;
                    if (solarSystem == null)
                    {
                        Logger.LogError("Solar System not found");
                        Snackbar?.Add("Solar System not found", Severity.Error);
                        MudDialog.Close(DialogResult.Cancel);
                    }
                    else
                        newWHSystem = await DbWHSystems.Create(new WHSystem(currentMapId,solarSystem.Id, solarSystem.Name, solarSystem.SecurityStatus, MouseX, MouseY)); //change position


                    if (newWHSystem == null)
                    {
                        Logger.LogError(MSG_ADD_WORHMOLE_DB_ERROR);
                        Snackbar?.Add(MSG_ADD_WORHMOLE_DB_ERROR, Severity.Error);
                        MudDialog.Close(DialogResult.Cancel);
                    }

                    EveSystemNodeModel nodeModel = await MapperServices.DefineEveSystemNodeModel(newWHSystem!);
                    CurrentWHMap?.WHSystems.Add(newWHSystem!);
                    CurrentDiagram?.Nodes.Add(nodeModel);

                    Snackbar?.Add(String.Format("{0} solar system successfully added",nodeModel.Name), Severity.Success);
                    MudDialog.Close(DialogResult.Ok(newWHSystem!.Id));

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
    }
}

