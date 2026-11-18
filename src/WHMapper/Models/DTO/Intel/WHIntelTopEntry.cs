namespace WHMapper.Models.DTO.Intel;

/// <summary>
/// Entry of a zKillboard top list.
/// </summary>
public sealed record WHIntelTopEntry(int Rank, long Id, string Name, int Kills);
