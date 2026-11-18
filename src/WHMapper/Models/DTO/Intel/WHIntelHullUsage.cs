namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// Hull flown by attackers, with the number of pilots seen in it.
/// </summary>
public sealed record WHIntelHullUsage(int ShipTypeId, int Count);
