using System.Collections.Concurrent;
using System.IO.Compression;
using WHMapper.Models.DTO.SDE;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using WHMapper.Services.Cache;
using System.IO.Abstractions;

namespace WHMapper.Services.SDE
{
    public class SDEServices : ISDEServices
    {
        private const string SDE_CHECKSUM_CURRENT_FILE = @"./Resources/SDE/currentchecksum";

        private const string SDE_DIRECTORY = @"./Resources/SDE/";
        private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
        private const string SDE_TARGET_DIRECTORY = @"./Resources/SDE/universe";

        private const string SDE_EVE_TARGET_DIRECTORY = @"./Resources/SDE/universe/universe/eve";
        private const string SDE_WORMHOLE_TARGET_DIRECTORY = @"./Resources/SDE/universe/universe/wormhole";
        private const string SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE = "solarsystem.yaml";

        private readonly ILogger _logger;
        private readonly ParallelOptions _options;
        private readonly IDeserializer _deserializer;
        private readonly EnumerationOptions _directorySearchOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true };
        private static Mutex mut = new Mutex();
        private readonly ICacheService _cacheService;
        private readonly IFileSystem _IFileSystem;
        private readonly ISDEDataSupplier _dataSupplier;

        public SDEServices(ILogger<SDEServices> logger, ICacheService cacheService, IFileSystem fileSystem, ISDEDataSupplier dataSupplier)
        {
            _logger = logger;
            _cacheService = cacheService;
            _options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            _dataSupplier = dataSupplier;
            _IFileSystem = fileSystem;
        }

        public bool IsExtractionSuccesful()
        {
            return _IFileSystem.Directory.Exists(SDE_TARGET_DIRECTORY);
        }

        /// <summary>
        /// Checks if the local SDE Package is different from the package that is made available by CCP. 
        /// </summary>
        //TODO: This should be async and not Task<T> because it is IO bound.
        public Task<bool> IsNewSDEAvailable()
        {
            try
            {
                var checksumOfAvailableSDEPackage = _dataSupplier.GetChecksum();
                if (string.IsNullOrEmpty(checksumOfAvailableSDEPackage))
                {
                    _logger.LogWarning("Couldn't retrieve checksum");
                    return Task.FromResult(false);
                }

                var localChecksum = GetCurrentChecksum();
                return Task.FromResult(localChecksum != checksumOfAvailableSDEPackage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check SDE");
                return Task.FromResult(false);
            }
        }

        //TODO: Find out how the SDE checksum calculation works, but for now we just save the file when downloaded.
        private string GetCurrentChecksum()
        {
            try
            {
                var fileContent = _IFileSystem.File.ReadLines(SDE_CHECKSUM_CURRENT_FILE);
                return string.Join(";", fileContent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
            }

            return string.Empty;
        }

        public Task<bool> ClearSDEResources()
        {
            try
            {
                if (_IFileSystem.Directory.Exists(SDE_DIRECTORY))
                {
                    _logger.LogInformation($"Deleting Eve SDE resources at {SDE_DIRECTORY}");
                    _IFileSystem.Directory.Delete(SDE_DIRECTORY, true);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearSDEResources");
                return Task.FromResult(false);
            }
        }

        public async Task<bool> DownloadSDE()
        {
            try
            {
                if (!_IFileSystem.Directory.Exists(SDE_DIRECTORY))
                    _IFileSystem.Directory.CreateDirectory(SDE_DIRECTORY);

                _logger.LogInformation("Start to download Eve SDE files");

                using (Stream sdeData = await _dataSupplier.GetSDEDataStreamAsync())
                {
                    using (var fs = _IFileSystem.FileStream.New(SDE_ZIP_PATH, FileMode.OpenOrCreate))
                    {
                        sdeData.CopyTo(fs);
                    }
                }
                var checksum = _dataSupplier.GetChecksum();
                _IFileSystem.File.WriteAllLines(@"./Resources/SDE/checksum", [checksum]);

                _logger.LogInformation("Eve SDE files downloaded");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Download SDE");
                return false;
            }
        }

        public Task<bool> ExtractSDE()
        {
            if (mut.WaitOne())
            {
                try
                {
                    _logger.LogInformation("Start to extrat Eve SDE files");
                    if (_IFileSystem.Directory.Exists(SDE_TARGET_DIRECTORY))
                    {
                        _logger.LogInformation("Delete old Eve SDE files");
                        _IFileSystem.Directory.Delete(SDE_TARGET_DIRECTORY, true);
                    }

                    if (!File.Exists(SDE_ZIP_PATH))
                    {
                        _logger.LogError("Impossible to extract SDE, no zip file");
                        return Task.FromResult(false);
                    }

                    using (ZipArchive archive = ZipFile.OpenRead(SDE_ZIP_PATH))
                    {
                        archive.ExtractToDirectory(SDE_TARGET_DIRECTORY);
                    }
                    _logger.LogInformation("Eve SDE files extracted");

                    return Task.FromResult(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Extract SDE");
                }
                finally
                {
                    mut.ReleaseMutex();
                }
            }
            return Task.FromResult(false);
        }

        public async Task<bool> ClearCache()
        {
            try
            {
                await _cacheService.Remove(ISDEServices.REDIS_SDE_SOLAR_SYSTEMS_KEY);
                await _cacheService.Remove(ISDEServices.REDIS_SOLAR_SYSTEM_JUMPS_KEY);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearCache");
                return false;
            }
        }

        public async Task<bool> Import()
        {
            try
            {
                if (!IsExtractionSuccesful())
                {
                    _logger.LogError("Impossible to import SDE, bad SDE extract.");
                    return false;
                }

                BlockingCollection<SolarSystemJump> tmp = new BlockingCollection<SolarSystemJump>();
                BlockingCollection<SDESolarSystem> SDESystems = new BlockingCollection<SDESolarSystem>();

                var sdeFiles = _IFileSystem.Directory.GetFiles(SDE_TARGET_DIRECTORY, SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE, _directorySearchOptions);
                if (sdeFiles.Count() > 0)
                {
                    Parallel.ForEach(sdeFiles, _options, async (directoryPath, token) =>
                    {
                        if (directoryPath.Contains(SDE_EVE_TARGET_DIRECTORY) || directoryPath.Contains(SDE_WORMHOLE_TARGET_DIRECTORY))
                        {
                            using (TextReader text_reader = File.OpenText(directoryPath))
                            {
                                var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .IgnoreUnmatchedProperties()
                                    .Build();
                                SDESolarSystem solarSystem = deserializer.Deserialize<SDESolarSystem>(text_reader);
                                var systemName = Path.GetFileName(Path.GetDirectoryName(directoryPath));
                                solarSystem.Name = (string.IsNullOrEmpty(systemName)) ? "Unknown" : systemName;

                                while (!SDESystems.TryAdd(solarSystem))
                                    await Task.Delay(1);
                            }

                        }
                        await Task.Yield();
                    });

                    //after all sde system loading build SolarSystemJumps
                    Parallel.ForEach(SDESystems, _options, async (system, token) =>
                    {
                        SolarSystemJump? solarSystemJump = null;
                        if (system.Stargates == null || system.Stargates.Count == 0)
                        {
                            solarSystemJump = new SolarSystemJump(system.SolarSystemID, system.Security);
                        }
                        else
                        {
                            var JumpSystemList = new List<SolarSystem>();
                            Parallel.ForEach(system.Stargates.Values, _options, async (stargate, token) =>
                            {
                                var s = SDESystems.FirstOrDefault(x => x.Stargates.ContainsKey(stargate.Destination));
                                if (s != null)
                                    JumpSystemList.Add(new SolarSystem(s.SolarSystemID, s.Security));

                                await Task.Yield();
                            });
                            solarSystemJump = new SolarSystemJump(system.SolarSystemID, system.Security, JumpSystemList);
                        }

                        while (!tmp.TryAdd(solarSystemJump))
                            await Task.Delay(1);

                        await Task.Yield();
                    });

                    //clean old cache and add new one
                    await ClearCache();
                    await _cacheService.Set(ISDEServices.REDIS_SDE_SOLAR_SYSTEMS_KEY, SDESystems);
                    await _cacheService.Set(ISDEServices.REDIS_SOLAR_SYSTEM_JUMPS_KEY, tmp);

                    return true;
                }
                else
                {
                    _logger.LogError("Impossible to importSolarSystemJumpList, bad SDE extract.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import SolarSystemJumpLis and SolarSystemList");
                return false;
            }
        }

        public async Task<IEnumerable<SDESolarSystem>?> GetSolarSystemList()
        {
            try
            {
                IEnumerable<SDESolarSystem>? results = await _cacheService.Get<IEnumerable<SDESolarSystem>?>(ISDEServices.REDIS_SDE_SOLAR_SYSTEMS_KEY);
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
                IEnumerable<SolarSystemJump>? results = await _cacheService.Get<IEnumerable<SolarSystemJump>?>(ISDEServices.REDIS_SOLAR_SYSTEM_JUMPS_KEY);
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
                if (!IsExtractionSuccesful())
                {
                    _logger.LogError("Impossible to searchSystem, bad SDE extract.");
                    return null;
                }

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
                if (!IsExtractionSuccesful())
                {
                    _logger.LogError("Impossible to searchSystem, bad SDE extract.");
                    return null;
                }

                var SDESystems = await GetSolarSystemList();
                if (SDESystems == null)
                {
                    _logger.LogError("Impossible to searchSystem, Empty SDE solar system list.");
                    return null;
                }


                if (!String.IsNullOrEmpty(value) && value.Length > 2)
                {
                    BlockingCollection<SDESolarSystem> results = new BlockingCollection<SDESolarSystem>();
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




