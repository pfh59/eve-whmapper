using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.EveMapper;

namespace WHMapper.Pages.Mapper.Administration.Map;

[Authorize(Policy = "Admin")]
public partial class AddAccess : ComponentBase
{
    private MudForm _form = null!;
    private bool _success = false;

    private IEnumerable<WHAccess> _accesses = null!;
    private WHAccess _selectedAccess = null!;
    private IEnumerable<WHAccess>? _selectedAccesses  = new HashSet<WHAccess>();
    
    private Func<WHAccess,string> _listConverter = a => a?.EveEntityName ?? string.Empty;
    
    [Inject]
    private ILogger<AddAccess> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;


    [Inject]
    private IWHAccessRepository DbWHAccesses { get; set; } = null!;

    [Inject]
    private IWHMapRepository DbWHMaps { get; set; } = null!;


    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public int MapId { get; set; }

    private WHMap _map = null!;

    

    protected override async Task OnInitializedAsync()
    {
        _form = new MudForm();

        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
    
        var accesses = await DbWHAccesses.GetAll();
        _map = await DbWHMaps.GetById(MapId);
        if(_map == null)
        {
            Logger.LogError("Map not found");
            Snackbar.Add("Map not found", Severity.Error);
            MudDialog.Cancel();
            return;
        }

        if(accesses == null)
        {
            Logger.LogError("No accesses found");
            Snackbar.Add("No accesses found", Severity.Error);
            MudDialog.Cancel();
            return;
        }

        if(_map.WHAccesses.Any())
        {
            _accesses = accesses.Where(a => !_map.WHAccesses.Any(ma => ma.Id == a.Id));
        }
        else
        {
            _accesses = accesses;
        }

        await base.OnParametersSetAsync();
    }




    private async Task Submit()
    {
        await _form.Validate();

        if (_form.IsValid)
        {
            if(_selectedAccesses == null)
            {
                Logger.LogError("No access selected");
                Snackbar.Add("No access selected", Severity.Error);
                return;
            }


            foreach(var access in _selectedAccesses)
            {
                _map.WHAccesses.Add(access);
            }

            if(await DbWHMaps.Update(_map.Id,_map)!=null)
            {
                _success = true;
                Snackbar.Add("Access added", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Logger.LogError("Failed to add access");
                Snackbar.Add("Failed to add access", Severity.Error);
                MudDialog.Close(DialogResult.Cancel);
            }
        }
        else
        {
            Logger.LogError("Error while validating form");
            Snackbar?.Add("Error while validating form", Severity.Error);
            MudDialog.Close(DialogResult.Cancel);
        }
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }



}
