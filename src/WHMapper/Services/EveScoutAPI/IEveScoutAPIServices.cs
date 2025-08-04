using System;
using WHMapper.Models.DTO.EveScout;

namespace WHMapper.Services.EveScoutAPI;

public interface IEveScoutAPIServices
{

    /// <summary>
    /// Fetches all Thera system entries.
    /// </summary>
    /// <returns>A list of Thera system entries.</returns>
    Task<IEnumerable<EveScoutSystemEntry>?> GetTheraSystemsAsync();

    /// <summary>
    /// Fetches all Turnur system entries.
    /// </summary>
    /// <returns>A list of Turnur system entries.</returns>
    Task<IEnumerable<EveScoutSystemEntry>> GetTurnurSystemsAsync();

}
