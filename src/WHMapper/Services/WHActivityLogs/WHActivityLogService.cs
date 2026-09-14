using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Repositories.WHActivityLogs;
using WHMapper.Repositories.WHMaps;

namespace WHMapper.Services.WHActivityLogs;

/// <summary>
/// Default implementation of <see cref="IWHActivityLogService"/>.
/// </summary>
public class WHActivityLogService : IWHActivityLogService
{
    private readonly IWHActivityLogRepository _activityLogRepository;
    private readonly IWHMapRepository _mapRepository;
    private readonly ILogger<WHActivityLogService> _logger;

    public WHActivityLogService(IWHActivityLogRepository activityLogRepository, IWHMapRepository mapRepository, ILogger<WHActivityLogService> logger)
    {
        _activityLogRepository = activityLogRepository;
        _mapRepository = mapRepository;
        _logger = logger;
    }

    public async Task RecordAsync(int characterId, int activityTypeId, int mapId, int occurrences = 1)
    {
        if (occurrences <= 0)
            return;

        try
        {
            if (mapId <= 0)
            {
                _logger.LogWarning("Activity {ActivityTypeId} of character {CharacterId} not recorded: invalid map id {MapId}", activityTypeId, characterId, mapId);
                return;
            }

            int? instanceId = await _mapRepository.GetInstanceIdAsync(mapId);
            var activities = Enumerable.Range(0, occurrences)
                .Select(_ => new WHActivityLog(characterId, activityTypeId, instanceId, mapId))
                .ToList();

            if (!await _activityLogRepository.CreateRange(activities))
                _logger.LogWarning("Activity {ActivityTypeId} of character {CharacterId} on map {MapId} not recorded", activityTypeId, characterId, mapId);
        }
        catch (Exception ex)
        {
            // Recording is best effort: a failure must never interrupt the map operation that triggered it.
            _logger.LogError(ex, "Error while recording activity {ActivityTypeId} of character {CharacterId} on map {MapId}", activityTypeId, characterId, mapId);
        }
    }

    public async Task RecordSignatureImportAsync(int characterId, int mapId, WHSignatureImportResult importResult)
    {
        await RecordAsync(characterId, WHActivityTypeIds.SignatureCreated, mapId, importResult.FullyScannedCreatedCount);
        await RecordAsync(characterId, WHActivityTypeIds.SignatureUpdated, mapId, importResult.FullyScannedChangedCount);
    }
}
