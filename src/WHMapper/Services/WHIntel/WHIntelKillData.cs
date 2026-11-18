using WHMapper.Models.DTO.ZKillboard;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Killmails of a system with the names they reference, cached as one entry.
/// </summary>
public sealed record WHIntelKillData(
    IReadOnlyList<ZKillmail> Killmails,
    Dictionary<int, string> Names,
    Dictionary<int, string> ShipGroupNames);
