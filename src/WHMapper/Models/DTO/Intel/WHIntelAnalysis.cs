namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// Deterministic analysis of the killmails of a system.
/// </summary>
/// <param name="KillCount">Killmails analysed.</param>
/// <param name="Residents">Probable residents, most likely first.</param>
/// <param name="NpcKillShare">Share of killmails made by NPCs only, from 0 to 1.</param>
/// <param name="SoloKillShare">Share of solo killmails, from 0 to 1.</param>
/// <param name="KillsPerUtcHour">Killmails per UTC hour, 24 values starting at 00:00.</param>
/// <param name="FleetHulls">Most flown attacker hulls, most flown first.</param>
/// <param name="FleetFromResidents">True when the hulls are those of the residents; false when they are those of every player attacker.</param>
/// <param name="AverageGangSize">Average number of player attackers on the killmails used for the hulls.</param>
public sealed record WHIntelAnalysis(
    int KillCount,
    IReadOnlyList<WHIntelResident> Residents,
    WHIntelActivityProfile Profile,
    double NpcKillShare,
    double SoloKillShare,
    IReadOnlyList<int> KillsPerUtcHour,
    WHIntelTimezone Timezone,
    IReadOnlyList<WHIntelHullUsage> FleetHulls,
    bool FleetFromResidents,
    double AverageGangSize);
