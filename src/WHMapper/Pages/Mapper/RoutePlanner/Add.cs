

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.DTO.SDE;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.SDE;

namespace WHMapper.Pages.Mapper.RoutePlanner
{

    [Authorize(Policy = "Admin, Access")]
    public partial class Add : ComponentBase
    {

        private const string MSG_SEARCH_ERROR = "Search System Error";
        private const string MSG_BAD_SOLAR_SYSTEM_NAME_ERROR = "Bad solar system name";
        private const string MSG_ADD_ROUTE_DB_ERROR = "Add route db error";


        [Inject]
        public ILogger<Add> Logger { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        private ISDEServices SDEServices { get; set; } = null!;

        [Inject]
        private IEveMapperRoutePlannerHelper EveMapperRoutePlannerHelper{ get; set; } = null!;

        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; } = null!;

        private MudForm _form = null!;
        private bool _success = false;

        private HashSet<SDESolarSystem> _systems = null!;
        private string _searchResult = string.Empty;
        private bool _searchInProgress = false;

        private bool _global = false;

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
                    var sdeSolarSystem = _systems.Where(x => x.Name.ToLower() == _searchResult.ToLower()).FirstOrDefault();

                    if (sdeSolarSystem == null)
                    {
                        Logger.LogError(MSG_BAD_SOLAR_SYSTEM_NAME_ERROR);
                        Snackbar?.Add(MSG_BAD_SOLAR_SYSTEM_NAME_ERROR, Severity.Error);
                        return;
                    }

                    var route = await EveMapperRoutePlannerHelper.AddRoute(sdeSolarSystem.SolarSystemID, _global);
                                     


                    if (route == null)
                    {
                        Logger.LogError(MSG_ADD_ROUTE_DB_ERROR);
                        Snackbar?.Add(MSG_ADD_ROUTE_DB_ERROR, Severity.Error);
                        return;
                    }


                    Snackbar?.Add(String.Format("{0} route successfully added",sdeSolarSystem.Name), Severity.Success);
                    MudDialog.Close(DialogResult.Ok(route.Id));
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



