using System.Globalization;

namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// zKillboard activity of a system and its analysis.
/// </summary>
/// <param name="RecentKills">Most recent kills, newest first.</param>
/// <param name="Analysis">Analysis of the fetched killmails; null when there are not enough of them.</param>
/// <param name="Names">Names of the characters, corporations, alliances and ship types referenced by the report.</param>
/// <param name="ShipGroupNames">Group name (e.g. "Heavy Assault Cruiser") of the fleet hulls, by ship type id.</param>
public sealed record WHIntelReport(
    WHIntelStatus Status,
    int SolarSystemId,
    string SystemName,
    IReadOnlyList<WHIntelKill> RecentKills,
    WHIntelTopLists TopLists,
    WHIntelAnalysis? Analysis,
    IReadOnlyDictionary<int, string> Names,
    IReadOnlyDictionary<int, string> ShipGroupNames)
{
    public static WHIntelReport Empty(WHIntelStatus status, int solarSystemId, string systemName) =>
        new(status, solarSystemId, systemName, [], WHIntelTopLists.Empty, null,
            new Dictionary<int, string>(), new Dictionary<int, string>());

    /// <summary>
    /// Name of an id; the id itself when unresolved, empty when the id is null.
    /// </summary>
    public string GetName(int? id)
    {
        if (!id.HasValue)
            return string.Empty;

        return Names.TryGetValue(id.Value, out var name) ? name : id.Value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Group name of a ship type; null when unknown.
    /// </summary>
    public string? GetShipGroupName(int shipTypeId) =>
        ShipGroupNames.TryGetValue(shipTypeId, out var name) ? name : null;
}
