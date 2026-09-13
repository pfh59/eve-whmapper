namespace WHMapper.Models.DTO.Statistics;

/// <summary>
/// Position of a character in the Top Probers leaderboard.
/// </summary>
/// <param name="Rank">Position in the leaderboard, starting at 1.</param>
/// <param name="CharacterId">EVE character.</param>
/// <param name="CharacterName">Current name of the character, or its id when the name cannot be resolved.</param>
/// <param name="Created">Number of signatures created.</param>
/// <param name="Updated">Number of signature updates.</param>
/// <param name="Total">Sum of <paramref name="Created"/> and <paramref name="Updated"/>.</param>
public record WHProberRank(int Rank, int CharacterId, string CharacterName, int Created, int Updated, int Total);
