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

namespace WHMapper.Pages.Mapper.Map;

[Authorize(Policy = "Access")]
public partial class SearchSystem :ComponentBase
{
    private const string MSG_SEARCH_ERROR = "Search System Error";
    private const string MSG_BAD_SOLAR_SYSTEM_NAME_ERROR = "Bad solar system name";
    private const string MSG_ADD_WORHMOLE_DB_ERROR = "Add Wormhole db error";


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
                if(_searchResult==null)
                {
                    Logger.LogError("Solar System not found");
                    Snackbar?.Add("Solar System not found", Severity.Error);
                    MudDialog.Close(DialogResult.Cancel);
                }
                else
                {
                    SystemEntity? solarSystem = await EveMapperEntity.GetSystem(_searchResult.SolarSystemID);
                    if (solarSystem == null)
                    {
                        Logger.LogError("Solar System not found");
                        Snackbar?.Add("Solar System not found", Severity.Error);
                        MudDialog.Close(DialogResult.Cancel);
                    }
                    else
                    {
                        MudDialog.Close(DialogResult.Ok(solarSystem));
                    }
                }         
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


