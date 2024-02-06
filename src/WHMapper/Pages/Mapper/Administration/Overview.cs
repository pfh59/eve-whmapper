using System;
using System.Reflection.Emit;
using Blazor.Diagrams.Core.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.SDE;
using WHMapper.Pages.Mapper.Signatures;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHAdmins;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Search;
using WHMapper.Services.SDE;

namespace WHMapper.Pages.Mapper.Administration
{
    [Authorize(Policy = "Admin")]
    public partial class Overview : ComponentBase
    {
        private const string MSG_SEARCH_ACCESS_ERROR = "Search Access Error";
        private const string MSG_SEARCH_ADMIN_ERROR = "Search Admin Error";

        private const string MSG_SUBMIT_ACCESS_ERROR = "Submit Access Error";
        private const string MSG_SUBMIT_ADMIN_ERROR = "Submit Admin Error";

        private const string MSG_SUBMIT_ACCESS_BAD_PARAMETER = "Submit Access Bad Parameter";
        private const string MSG_SUBMIT_ADMIN_BAD_PARAMETER = "Submit Admin Bad Parameter";

        private const string MSG_NO_ACCESS_ADDED = "No access added";
        private const string MSG_NO_ADMIN_ADDED = "No admin added";

        private IEnumerable<WHAccess> WHAccesses { get; set; } = null!;
        private IEnumerable<WHAdmin> WHAdmins { get; set; } = null!;


        private WHAccess _selectedWHAccess = null!;
        private WHAdmin _selectedWHAdmin = null!;


        private MudForm _formAccess = null!;
        private MudForm _formAdmin = null!;

        private bool _successAccess = false;
        private bool _successAdmin = false;

        private WHAccess _searchResultAccess = null!;
        private WHAdmin _searchResultAdmin = null!;

        private HashSet<WHAccess> _eveCharacterEntities = new HashSet<WHAccess>();
        private HashSet<WHAdmin> _eveCharacters = new HashSet<WHAdmin>();

        private bool _searchInProgress = false;
        private ParallelOptions _options = new ParallelOptions { MaxDegreeOfParallelism = 4 };

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public ILogger<Overview> Logger { get; set; } = null!;

        [Inject]
        private IEveAPIServices EveAPIServices { get; set; } = null!;

        [Inject]
        private IWHAccessRepository DbWHAccesses { get; set; } = null!;

        [Inject]
        private IWHAdminRepository DbWHAdmin { get; set; } = null!;


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (firstRender)
            {
                await Restore();
                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task Restore()
        {
            if (DbWHAccesses != null && DbWHAdmin!=null)
            {
                var access = await DbWHAccesses.GetAll();
                if (access != null)
                    WHAccesses = access;

                var admins = await DbWHAdmin.GetAll();
                if (admins != null)
                    WHAdmins = admins;
            }
        }


        private async Task<IEnumerable<WHAccess>?> SearchAccess(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || EveAPIServices == null || EveAPIServices.SearchServices == null || value.Length < 5 || _searchInProgress)
                    return null;

                _eveCharacterEntities.Clear();
                _searchInProgress = true;

                if (EveAPIServices != null && EveAPIServices.SearchServices != null)
                {
                    var allianceResults = await EveAPIServices.SearchServices.SearchAlliance(value);
                    var coorpoResults = await EveAPIServices.SearchServices.SearchCorporation(value);
                    var characterResults = await EveAPIServices.SearchServices.SearchCharacter(value);

                    if (allianceResults != null && allianceResults.Alliances != null)
                    {
                        await Parallel.ForEachAsync(allianceResults.Alliances.Take(20), _options, async (allianceId, token) =>
                        {
                            var alliance = await EveAPIServices.AllianceServices.GetAlliance(allianceId);
                            _eveCharacterEntities.Add(new WHAccess(allianceId, alliance.Name, Models.Db.Enums.WHAccessEntity.Alliance));
                        });

                    }

                    if (coorpoResults != null && coorpoResults.Corporations != null)
                    {
                        await Parallel.ForEachAsync(coorpoResults.Corporations.Take(20), _options, async (corpoId, token) =>
                        {
                            var corpo = await EveAPIServices.CorporationServices.GetCorporation(corpoId);
                            _eveCharacterEntities.Add(new WHAccess(corpoId, corpo.Name, Models.Db.Enums.WHAccessEntity.Corporation));
                        });
                    }

                    if (characterResults != null && characterResults.Characters != null)
                    {
                        await Parallel.ForEachAsync(characterResults.Characters.Take(20), _options, async (characterId, token) =>
                        {
                            var character = await EveAPIServices.CharacterServices.GetCharacter(characterId);
                            _eveCharacterEntities.Add(new WHAccess(characterId, character.Name, Models.Db.Enums.WHAccessEntity.Character));
                        });
                    }
                }

                _searchInProgress = false;
                if (_eveCharacterEntities != null)
                    return _eveCharacterEntities;
                else
                    return null;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, MSG_SEARCH_ACCESS_ERROR);
                Snackbar.Add(MSG_SEARCH_ACCESS_ERROR, Severity.Error);
                return null;
            }
        }

        private async Task<IEnumerable<WHAdmin>?> SearchAdmin(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || EveAPIServices == null || EveAPIServices.SearchServices == null || value.Length < 5 || _searchInProgress)
                    return null;

                _eveCharacters.Clear();
                _searchInProgress = true;

                if (EveAPIServices != null && EveAPIServices.SearchServices != null)
                {
                    var characterResults = await EveAPIServices.SearchServices.SearchCharacter(value);

                    if (characterResults != null && characterResults.Characters != null)
                    {

                        await Parallel.ForEachAsync(characterResults.Characters.Take(20), _options, async (characterId, token) =>
                        {
                            var character = await EveAPIServices.CharacterServices.GetCharacter(characterId);
                            _eveCharacters.Add(new WHAdmin(characterId, character.Name));
                        });
                    }
                }

                _searchInProgress = false;
                if (_eveCharacters != null)
                    return _eveCharacters;
                else
                    return null;
            }

            catch (Exception ex)
            {
                Logger.LogError(ex, MSG_SEARCH_ADMIN_ERROR);
                Snackbar.Add(MSG_SEARCH_ADMIN_ERROR, Severity.Error);
                return null;
            }
        }

        private IEnumerable<string> ValidateAccess(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _searchInProgress = false;
                yield return "The alliance,coorporation or character name is required";
                yield break;
            }

            if (value.Length < 3)
            {
                _searchInProgress = false;
                yield return "Please enter 3 or more characters";
                yield break;
            }

            
            if (_eveCharacterEntities == null || _eveCharacterEntities.Where(x => x.EveEntityName.ToLower() == value.ToLower()).FirstOrDefault() == null)
            {
                _searchInProgress = false;
                yield return "Bad alliance,coorporation or character name";
                yield break;
            }

        }

        private IEnumerable<string> ValidateAdmin(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _searchInProgress = false;
                yield return "The character name is required";
                yield break;
            }

            if (value.Length < 3)
            {
                _searchInProgress = false;
                yield return "Please enter 3 or more characters";
                yield break;
            }


            if (_eveCharacters == null || _eveCharacters.Where(x => x.EveCharacterName.ToLower() == value.ToLower()).FirstOrDefault() == null)
            {
                _searchInProgress = false;
                yield return "Bad character name";
                yield break;
            }

        }

        private async Task SubmitAccess()
        {
            await _formAccess.Validate();

            if (_formAccess.IsValid)
            {
                try
                {
                    if(await DbWHAccesses.Create(_searchResultAccess) !=null)
                    {
                        await Restore();
                        _searchResultAccess = null!;
                    }
                    else
                    {
                        Logger.LogError(MSG_NO_ACCESS_ADDED);
                        Snackbar.Add(MSG_NO_ACCESS_ADDED, Severity.Error);
                    }

                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, MSG_SUBMIT_ACCESS_ERROR);
                    Snackbar.Add(MSG_SUBMIT_ACCESS_ERROR, Severity.Error);
                }
            }
            else
            {
                Logger.LogWarning(MSG_SUBMIT_ACCESS_BAD_PARAMETER);
                Snackbar.Add(MSG_SUBMIT_ACCESS_BAD_PARAMETER, Severity.Warning);
            }
        }

        private async Task SubmitAdmin()
        {
            await _formAdmin.Validate();

            if (_formAdmin.IsValid)
            {
                try
                {
                    if (await DbWHAdmin.Create(_searchResultAdmin) != null)
                    {
                        await Restore();
                        _searchResultAdmin = null!;
                    }
                    else
                    {
                        Logger.LogError(MSG_NO_ADMIN_ADDED);
                        Snackbar.Add(MSG_NO_ADMIN_ADDED, Severity.Error);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, MSG_SUBMIT_ADMIN_ERROR);
                    Snackbar.Add(MSG_SUBMIT_ADMIN_ERROR, Severity.Error);
                }
            }
            else
            {
                Logger.LogWarning(MSG_SUBMIT_ADMIN_BAD_PARAMETER);
                Snackbar.Add(MSG_SUBMIT_ADMIN_BAD_PARAMETER, Severity.Warning);
            }
        }

        private async Task DeleteAccess(int id)
        {
            var parameters = new DialogParameters();
            parameters.Add("AccessId", id);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var dialog = DialogService.Show<Delete>("Delete WHMapper Access", parameters, options);
            DialogResult result = await dialog.Result;

            if (result!=null && !result.Canceled)
                await Restore();
        }

        private async Task DeleteAdmin(int id)
        {
            
            var parameters = new DialogParameters();
            parameters.Add("AdminId", id);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var dialog = DialogService.Show<Delete>("Delete WHMapper Admin", parameters, options);
            DialogResult result = await dialog.Result;

            if (result != null && !result.Canceled)
                await Restore();
        }
    }
}

