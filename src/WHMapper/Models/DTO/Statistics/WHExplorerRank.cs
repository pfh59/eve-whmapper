namespace WHMapper.Models.DTO.Statistics;

/// <summary>
/// Position of a character in the Top Explorers leaderboard.
/// </summary>
/// <param name="Rank">Position in the leaderboard, starting at 1.</param>
/// <param name="CharacterId">EVE character.</param>
/// <param name="CharacterName">Current name of the character, or its id when the name cannot be resolved.</param>
/// <param name="SystemsOpened">Number of systems the character added to a map.</param>
public record WHExplorerRank(int Rank, int CharacterId, string CharacterName, int SystemsOpened);
