using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.Intel;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHIntel;

namespace WHMapper.Components.Pages.Mapper.Intel;

/// <summary>
/// Dialog showing the zKillboard activity of a system, its analysis and, when configured, an AI assessment.
/// </summary>
/// <remarks>
/// This component is not routed, so the <see cref="AuthorizeAttribute"/> is not evaluated: authorization is
/// enforced by <see cref="IWHIntelService"/>.
/// </remarks>
[Authorize(Policy = "Access")]
public partial class IntelDialog : IDisposable
{
    private const string ZKILLBOARD_SYSTEM_URL = "https://zkillboard.com/system/{0}/";
    private const string SHIP_ICON_URL = "https://images.evetech.net/types/{0}/icon?size=32";

    private const double BILLION = 1_000_000_000;
    private const double MILLION = 1_000_000;
    private const double THOUSAND = 1_000;

    private static readonly string[] HOUR_LABELS = Enumerable.Range(0, 24)
        .Select(x => x.ToString("00", CultureInfo.InvariantCulture))
        .ToArray();

    private const int MIN_KILLS_FOR_ANALYSIS = WHIntelAnalyzer.MIN_KILLS_FOR_ANALYSIS;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter, EditorRequired]
    public int MapId { get; set; }

    [Parameter, EditorRequired]
    public int SolarSystemId { get; set; }

    [Parameter, EditorRequired]
    public string SystemName { get; set; } = string.Empty;

    [Inject]
    private IWHIntelService IntelService { get; set; } = null!;

    [Inject]
    private IEveMapperUserManagementService UserManagement { get; set; } = null!;

    [Inject]
    private ClientUID UID { get; set; } = null!;

    [Inject]
    private ILogger<IntelDialog> Logger { get; set; } = null!;

    private readonly CancellationTokenSource _cancellation = new();
    private readonly BarChartOptions _hourlyChartOptions = new()
    {
        YAxisFormat = "0",
        YAxisTitle = "Kills",
        ShowLegend = false
    };

    private bool _loading = true;
    private bool _aiLoading;
    private WHIntelReport? _report;
    private WHIntelAiReport? _aiReport;
    private List<ChartSeries<double>> _hourlySeries = new();

    private string ZKillboardSystemUrl => string.Format(CultureInfo.InvariantCulture, ZKILLBOARD_SYSTEM_URL, SolarSystemId);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var primaryAccount = await UserManagement.GetPrimaryAccountAsync(UID.ClientId ?? string.Empty);
            if (primaryAccount == null)
            {
                Logger.LogWarning("Intel dialog opened without a primary account");
                ShowReport(WHIntelReport.Empty(WHIntelStatus.Unauthorized, SolarSystemId, SystemName));
                return;
            }

            ShowReport(await IntelService.GetIntelAsync(primaryAccount.Id, MapId, SolarSystemId, SystemName, _cancellation.Token));

            // The zKillboard data is shown first; the AI report never blocks it.
            if (!IntelService.IsAiEnabled || _report!.Status != WHIntelStatus.Available)
                return;

            _aiLoading = true;
            StateHasChanged();
            _aiReport = await IntelService.GetAiReportAsync(primaryAccount.Id, MapId, _report, _cancellation.Token);
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
        {
            // The dialog was closed while loading.
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while loading the intel of system {SolarSystemId}", SolarSystemId);
            _report ??= WHIntelReport.Empty(WHIntelStatus.Unavailable, SolarSystemId, SystemName);
            _aiReport = WHIntelAiReport.WithStatus(WHIntelAiStatus.Unavailable);
        }
        finally
        {
            _loading = false;
            _aiLoading = false;
        }
    }

    private void ShowReport(WHIntelReport report)
    {
        _hourlySeries = report.Analysis == null
            ? new()
            : new() { new() { Name = "Kills", Data = report.Analysis.KillsPerUtcHour.Select(x => (double)x).ToArray() } };
        _report = report;
        _loading = false;
    }

    private void Close() => MudDialog.Close();

    private static string GetShipIconUrl(int shipTypeId) =>
        string.Format(CultureInfo.InvariantCulture, SHIP_ICON_URL, shipTypeId);

    private static Color GetProfileColor(WHIntelActivityProfile profile) => profile switch
    {
        WHIntelActivityProfile.PVP => Color.Error,
        WHIntelActivityProfile.PVE => Color.Success,
        _ => Color.Warning
    };

    private static string FormatTimezone(WHIntelTimezone timezone) => timezone switch
    {
        WHIntelTimezone.EU => "EU",
        WHIntelTimezone.US => "US",
        WHIntelTimezone.AUCN => "AU/CN",
        _ => "Unknown"
    };

    private static string FormatPercent(double share) => share.ToString("P0", CultureInfo.InvariantCulture);

    private static string FormatIsk(double value) => value switch
    {
        >= BILLION => (value / BILLION).ToString("0.##", CultureInfo.InvariantCulture) + "B",
        >= MILLION => (value / MILLION).ToString("0.#", CultureInfo.InvariantCulture) + "M",
        >= THOUSAND => (value / THOUSAND).ToString("0.#", CultureInfo.InvariantCulture) + "K",
        _ => value.ToString("0", CultureInfo.InvariantCulture)
    };

    public void Dispose()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
    }
}
