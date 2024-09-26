using Blazor.Diagrams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.SDE;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveMapper;

namespace WHMapper.Pages.Mapper.Search
{
    [Authorize(Policy = "Access")]
    public partial class SearchSystem : ComponentBase
    {
        private const string MSG_SEARCH_ERROR = "Search System Error";
        private const string MSG_BAD_SOLAR_SYSTEM_NAME_ERROR = "Bad solar system name";
        
        [Inject]
        private ILogger<SearchSystem> Logger { get; set; } = null!;

        [Inject]
        private IEveMapperService EveMapperEntity { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private IEveMapperSearch EveMapperSearch { get; set; } = null!;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; } = null!;

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
                    await HandleValidFormSubmission();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
            else
            {
                HandleInvalidFormSubmission();
            }
        }

        private async Task HandleValidFormSubmission()
        {
            if (_searchResult == null)
            {
                LogAndNotifyError("Solar System not found");
                MudDialog.Close(DialogResult.Cancel);
            }
            else
            {
                SystemEntity? solarSystem = await EveMapperEntity.GetSystem(_searchResult.SolarSystemID);
                if (solarSystem == null)
                {
                    LogAndNotifyError("Solar System not found");
                    MudDialog.Close(DialogResult.Cancel);
                }
                else
                {
                    MudDialog.Close(DialogResult.Ok(solarSystem));
                }
            }
        }

        private void HandleException(Exception ex)
        {
            Snackbar?.Add(ex.Message, Severity.Error);
            MudDialog.Close(DialogResult.Cancel);
        }

        private void HandleInvalidFormSubmission()
        {
            Logger.LogError(MSG_BAD_SOLAR_SYSTEM_NAME_ERROR);
            Snackbar?.Add(MSG_BAD_SOLAR_SYSTEM_NAME_ERROR, Severity.Error);
            MudDialog.Close(DialogResult.Cancel);
        }

        private void LogAndNotifyError(string message)
        {
            Logger.LogError(message);
            Snackbar?.Add(message, Severity.Error);
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}


