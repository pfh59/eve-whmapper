using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Emit;
using Blazor.Diagrams.Core.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveAPI.Alliance;
using WHMapper.Models.DTO.EveAPI.Character;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Models.DTO.SDE;
using WHMapper.Pages.Mapper.Signatures;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHAdmins;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Search;
using WHMapper.Services.EveMapper;
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

        private AEveEntity _searchResultAccess = null!;
        private CharactereEntity _searchResultAdmin = null!;

        private HashSet<WHAccess> _eveCharacterEntities = new HashSet<WHAccess>();
        private HashSet<WHAdmin> _eveCharacters = new HashSet<WHAdmin>();



      

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public ILogger<Overview> Logger { get; set; } = null!;

        [Inject]
        private IEveMapperSearch EveMapperSearch { get; set; } = null!;

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
        private async Task SubmitAccess()
        {
            await _formAccess.Validate();

            if (_formAccess.IsValid)
            {
                try
                {
                    WHAccess newAccess = null!;
                    switch(_searchResultAccess.EntityType)
                    {
                        case EveEntityEnums.Alliance:
                            newAccess = new WHAccess(_searchResultAccess.Id, _searchResultAccess.Name, WHAccessEntity.Alliance);
                            break;
                        case EveEntityEnums.Corporation:
                            newAccess = new WHAccess(_searchResultAccess.Id, _searchResultAccess.Name, WHAccessEntity.Corporation);
                            break;
                        case EveEntityEnums.Character:
                            newAccess = new WHAccess(_searchResultAccess.Id, _searchResultAccess.Name, WHAccessEntity.Character);
                            break;
                    }

                    if(await DbWHAccesses.Create(newAccess) != null)
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
                    if (await DbWHAdmin.Create(new WHAdmin(_searchResultAdmin.Id,_searchResultAdmin.Name)) != null)
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

