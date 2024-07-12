using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using WHMapper.Shared.Models.DTO.SDE;
using WHMapper.Shared.Services.Cache;

namespace WHMapper.Shared.Services.SDE
{
    public class SDEService : ISDEService
    {
        private readonly ILogger<SDEService> _logger;
        private readonly ICacheService _cacheService;

        public SDEService(ILogger<SDEService> logger, ICacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<SDESolarSystem>?> GetSolarSystemList()
        {
            try
            {
                var results = await _cacheService.Get<IEnumerable<SDESolarSystem>?>(SDEConstants.REDIS_SDE_SOLAR_SYSTEMS_KEY);
                if (results == null)
                    return new List<SDESolarSystem>();
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSolarSystemList");
                return null;
            }
        }

        public async Task<IEnumerable<SolarSystemJump>?> GetSolarSystemJumpList()
        {
            try
            {
                var results = await _cacheService.Get<IEnumerable<SolarSystemJump>?>(SDEConstants.REDIS_SOLAR_SYSTEM_JUMPS_KEY);
                if (results == null)
                    return new List<SolarSystemJump>();

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSolarSystemJumpList");
                return null;
            }
        }

        public async Task<SDESolarSystem?> SearchSystemById(int value)
        {
            try
            {
                var SDESystems = await GetSolarSystemList();
                if (SDESystems == null)
                {
                    _logger.LogError("Impossible to searchSystem, Empty SDE solar system list.");
                    return null;
                }

                var result = SDESystems.Where(x => x.SolarSystemID == value).FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchSystem");
                return null;
            }
        }

        public async Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value)
        {
            try
            {
                var SDESystems = await GetSolarSystemList();
                if (SDESystems == null)
                {
                    _logger.LogError("Impossible to searchSystem, Empty SDE solar system list.");
                    return null;
                }


                if (!string.IsNullOrEmpty(value) && value.Length > 2)
                {
                    var results = new BlockingCollection<SDESolarSystem>();
                    SDESystems.AsParallel().Where(x => x.Name.ToLower().Contains(value.ToLower())).ForAll(x => results.Add(x));
                    return results.OrderBy(x => x.Name);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchSystem");
                return null;
            }
        }
    }
}