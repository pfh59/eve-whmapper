using WHMapper.Models.DTO.MapLayout;

namespace WHMapper.Services.MapLayout;

/// <summary>
/// Browser-side persistence of map panel layouts.
/// </summary>
/// <remarks>
/// Layout is per browser on purpose: the arrangement depends on the screen, so it must not follow
/// the account across devices the way <c>WHUserSetting</c> does.
/// </remarks>
public interface IMapLayoutStorage
{
    /// <summary>
    /// Reads the stored layout of a map.
    /// </summary>
    /// <returns><c>null</c> when nothing is stored or the payload cannot be read.</returns>
    Task<MapLayoutDto?> GetAsync(int mapId);

    /// <summary>
    /// Writes the layout of a map, replacing any previous value.
    /// </summary>
    Task SetAsync(int mapId, MapLayoutDto layout);

    /// <summary>
    /// Removes the stored layout of a map, so that defaults apply again.
    /// </summary>
    Task RemoveAsync(int mapId);
}
