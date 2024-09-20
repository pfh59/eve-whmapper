

using Microsoft.AspNetCore.Components;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveAPI.Location;
using WHMapper.Services.EveAPI;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Repositories.WHMaps;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHSystems;
using MudBlazor;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHSystemLinks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Models.DTO;
using WHMapper.Services.EveOnlineUserInfosProvider;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;
using System.Data;
using Microsoft.AspNetCore.Components.Web;
using WHMapper.Services.WHSignature;
using WHMapper.Services.EveMapper;
using Blazor.Diagrams.Core.Behaviors;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Repositories.WHJumpLogs;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal;
using BlazorContextMenu;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.WHColor;
using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Pages.Mapper;

[Authorize(Policy = "Access")]
public partial class Overview : ComponentBase, IAsyncDisposable
{
    private List<WHMap> WHMaps { get; set; } = new List<WHMap>();

    private int _selectedWHMapIndex = 0;
    private int SelectedWHMapIndex
    {
        get
        {
            return _selectedWHMapIndex;
        }
        set
        {
            if (_selectedWHMapIndex != value)
            {
                _selectedWHMapIndex = value;
            }
        }
    }

    private bool _loading = true;

    [Inject]
    public ILogger<Overview> Logger { get; set; } = null!;
    [Inject]
    public ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private  AuthenticationStateProvider _authenticationStateProvider {get;set;} = null!;
    [Inject]
    private IAuthorizationService _authorizationService { get; set; } = null!;

    [Inject]
    IEveMapperRealTimeService? RealTimeService {get;set;} = null!;

    [Inject]
    IWHMapRepository DbWHMaps { get; set; } = null!;

    [Inject]
    private IEveMapperTracker TrackerServices { get; set; } = null!;

    [Inject]
    private IPasteServices PasteServices { get; set; } = null!;


    private WHMap? _selectedWHMap = null!;


    protected override async Task OnInitializedAsync()
    {
        if (!await RestoreMaps())
        {
            Snackbar?.Add("Mapper Initialization error", Severity.Error);
        }
        if(!await InitRealTimeService())
        {
            Snackbar?.Add("RealTimeService Initialization error", Severity.Error);
        }

    
        _loading = false;
        await base.OnInitializedAsync();
    }


    private void InitPasteServices()
    {
        //PasteServices.Pasted += OnPasted;
    }

    private async Task<bool> RestoreMaps()
    {
        var allMaps = await DbWHMaps.GetAll();
        if (allMaps == null || !allMaps.Any())
        {
            _selectedWHMap = await DbWHMaps.Create(new WHMap("Default Maps"));
            if (_selectedWHMap != null)
            {
                await RestoreMaps();
            }
        }
        else
        {
            WHMaps.Clear();
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
    
            foreach (var map in allMaps)
            {
                var authorizationResult = await _authorizationService.AuthorizeAsync(authState.User, map.Id, "Map");
                if (authorizationResult.Succeeded)
                {
                    WHMaps.Add(map);
                }
            }

            _selectedWHMap = WHMaps.FirstOrDefault();
        }

        if(allMaps!=null && allMaps.Any() && _selectedWHMap==null)
            return true;
        else
            return _selectedWHMap != null;
    }

    public async ValueTask DisposeAsync()
    {
        if (TrackerServices != null)
        {
            await TrackerServices.StopTracking();

        }

        if(RealTimeService!=null)
        {
            await RealTimeService.Stop();
            await RealTimeService.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private async Task<bool> InitRealTimeService()
    {
        try
        {
            if(RealTimeService==null) 
            {
                Logger.LogError("RealTimeService is null");
                return false;
            }

            RealTimeService.UserConnected += OnUserConnected;
            RealTimeService.UserDisconnected+=OnUserDisconnected;

            return await RealTimeService.Start();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "InitRealTimeService error");
            return false;
        }
    } 

    #region RealTimeService User Events
    

    private async Task OnUserConnected(string user)
    {
        try
        {
            Snackbar?.Add($"{user} are connected", Severity.Info);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyUserConnected error");
        }
    }

    private async Task OnUserDisconnected(string user)
    {
        
        try
        {
            Snackbar?.Add($"{user} are disconnected", Severity.Info);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "On NotifyUserDisconnected error");
        }
    }

    #endregion


    /*
    private async Task OnPasted(string? data)
    {
        if((_selectedWHMap!=null) && (_selectedSystemNode!=null))
        {
            try
            {
                string scanUser = await UserInfos.GetUserName();
                if (await SignatureHelper.ImportScanResult(scanUser, _selectedSystemNode.IdWH, data,false))
                {
                    await WHSignaturesView.Restore();
                    Snackbar?.Add("Signatures successfully added/updated", Severity.Success);
                    await NotifyWormholeSignaturesChanged(_selectedWHMap.Id, _selectedSystemNode.IdWH);
                }
                else
                    Snackbar?.Add("No signatures added/updated", Severity.Error);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Handle Custom Paste error");
                Snackbar?.Add(ex.Message, Severity.Error);
            }
        }
    }*/

}