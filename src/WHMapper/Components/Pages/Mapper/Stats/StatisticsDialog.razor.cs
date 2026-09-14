using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.Db;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.Statistics;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHActivityLogs;
using WHMapper.Services.WHStatistics;

namespace WHMapper.Components.Pages.Mapper.Stats;

/// <summary>
/// Dialog showing the Top Probers and Top Explorers leaderboards of an instance, side by side for every period.
/// </summary>
/// <remarks>
/// This component is not routed, so the <see cref="AuthorizeAttribute"/> is not evaluated: authorization is
/// enforced by <see cref="IWHStatisticsService"/>.
/// </remarks>
[Authorize(Policy = "Access")]
public partial class StatisticsDialog
{
    /// <summary>
    /// Periods displayed, in display order.
    /// </summary>
    private static readonly WHStatisticsPeriod[] PERIODS = Enum.GetValues<WHStatisticsPeriod>();

    /// <summary>
    /// Map selector value meaning "every map the viewer can open". Map ids start at 1.
    /// </summary>
    private const int ALL_MAPS_ID = 0;

    private const string ALL_MAPS_LABEL = "All maps";
    private const string OWN_ROW_CLASS = "whm-stats-own-row";

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject]
    private ILogger<StatisticsDialog> Logger { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IWHStatisticsService StatisticsService { get; set; } = null!;

    [Inject]
    private IWHActivityLogService ActivityLogService { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    private int? _viewerCharacterId;
    private IReadOnlyList<WHInstance> _instances = Array.Empty<WHInstance>();
    private IReadOnlyList<WHMap> _maps = Array.Empty<WHMap>();
    private int _selectedInstanceId;
    private int _selectedMapId = ALL_MAPS_ID;
    private bool _loading = true;
    private IReadOnlyDictionary<WHStatisticsPeriod, WHStatisticsReport>? _reports;

    private Dictionary<WHStatisticsPeriod, ChartData> _proberCharts = new();
    private readonly StackedBarChartOptions _proberChartOptions = new()
    {
        XAxisLabelRotation = 45,
        YAxisFormat = "0",
        YAxisTitle = "Signatures",
        ShowValues = true
    };

    private Dictionary<WHStatisticsPeriod, ChartData> _explorerCharts = new();
    private readonly BarChartOptions _explorerChartOptions = new()
    {
        XAxisLabelRotation = 45,
        YAxisFormat = "0",
        YAxisTitle = "Systems",
        ShowLegend = false,
        ShowValues = true
    };

    /// <summary>
    /// Labels and series drawn on one chart.
    /// </summary>
    private sealed record ChartData(string[] Labels, List<ChartSeries<double>> Series);

    private string RetentionCaption => ActivityLogService.RetentionDays > 0
        ? $"History is kept for {ActivityLogService.RetentionDays} days."
        : "History is kept indefinitely.";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId ?? string.Empty);
            if (primaryAccount != null)
            {
                _viewerCharacterId = primaryAccount.Id;
                _instances = await StatisticsService.GetAccessibleInstancesAsync(primaryAccount.Id);
                if (_instances.Count > 0)
                    _selectedInstanceId = _instances[0].Id;
            }
            else
            {
                Logger.LogWarning("Statistics dialog opened without a primary account");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while loading the instances of the statistics dialog");
            Snackbar.Add("Statistics could not be loaded", Severity.Error);
        }

        await ReloadAsync(reloadMaps: true);
    }

    private async Task OnInstanceChangedAsync(int instanceId)
    {
        _selectedInstanceId = instanceId;
        await ReloadAsync(reloadMaps: true);
    }

    private async Task OnMapChangedAsync(int mapId)
    {
        _selectedMapId = mapId;
        await ReloadAsync(reloadMaps: false);
    }

    /// <summary>
    /// Reloads the leaderboards for the current selection.
    /// </summary>
    /// <remarks>
    /// The previous reports are cleared first, so stale rows are never displayed while loading.
    /// </remarks>
    /// <param name="reloadMaps">True to reload the maps of the selected instance and reset the map selector.</param>
    private async Task ReloadAsync(bool reloadMaps)
    {
        _loading = true;
        _reports = null;

        try
        {
            if (!_viewerCharacterId.HasValue || _instances.Count == 0)
            {
                ShowEmptyReports();
                return;
            }

            if (reloadMaps)
            {
                _selectedMapId = ALL_MAPS_ID;
                _maps = await StatisticsService.GetAccessibleMapsAsync(_viewerCharacterId.Value, _selectedInstanceId);
            }

            int? mapId = _selectedMapId == ALL_MAPS_ID ? null : _selectedMapId;
            var reports = await StatisticsService.GetLeaderboardsAsync(_viewerCharacterId.Value, _selectedInstanceId, mapId);
            ShowReports(reports);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while loading the statistics of instance {InstanceId}", _selectedInstanceId);
            Snackbar.Add("Statistics could not be loaded", Severity.Error);
            ShowEmptyReports();
        }
    }

    private void ShowEmptyReports() => ShowReports(PERIODS.ToDictionary(x => x, _ => WHStatisticsReport.Empty));

    /// <summary>
    /// Displays the report of every period and builds the data of its charts.
    /// </summary>
    private void ShowReports(IReadOnlyDictionary<WHStatisticsPeriod, WHStatisticsReport> reports)
    {
        _proberCharts = reports.ToDictionary(x => x.Key, x => new ChartData(
            x.Value.Probers.Select(p => p.CharacterName).ToArray(),
            new List<ChartSeries<double>>
            {
                new() { Name = "Created", Data = x.Value.Probers.Select(p => (double)p.Created).ToArray() },
                new() { Name = "Updated", Data = x.Value.Probers.Select(p => (double)p.Updated).ToArray() }
            }));

        _explorerCharts = reports.ToDictionary(x => x.Key, x => new ChartData(
            x.Value.Explorers.Select(e => e.CharacterName).ToArray(),
            new List<ChartSeries<double>>
            {
                new() { Name = "Systems opened", Data = x.Value.Explorers.Select(e => (double)e.SystemsOpened).ToArray() }
            }));

        _reports = reports;
        _loading = false;
    }

    private string FormatInstanceName(int instanceId)
    {
        return _instances.FirstOrDefault(x => x.Id == instanceId)?.Name ?? string.Empty;
    }

    private string FormatMapName(int mapId)
    {
        if (mapId == ALL_MAPS_ID)
            return ALL_MAPS_LABEL;

        return _maps.FirstOrDefault(x => x.Id == mapId)?.Name ?? string.Empty;
    }

    private static string FormatPeriod(WHStatisticsPeriod period) => period switch
    {
        WHStatisticsPeriod.Last7Days => "Last 7 days",
        WHStatisticsPeriod.Last30Days => "Last 30 days",
        WHStatisticsPeriod.AllTime => "All time",
        _ => period.ToString()
    };

    private string GetProberRowClass(WHProberRank rank, int index) => GetRowClass(rank.CharacterId);

    private string GetExplorerRowClass(WHExplorerRank rank, int index) => GetRowClass(rank.CharacterId);

    /// <summary>
    /// Returns the CSS class highlighting the viewer's own row, or an empty string for any other character.
    /// </summary>
    private string GetRowClass(int characterId) => characterId == _viewerCharacterId ? OWN_ROW_CLASS : string.Empty;

    private void Close() => MudDialog.Close();
}
