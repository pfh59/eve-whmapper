using System.Collections.Concurrent;
using System.IO.Abstractions;
using Testably.Abstractions;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.Cache;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WHMapper.Services.SDE
{
    public class SDEServiceManager : ISDEServiceManager
    {
        private const string SDE_CHECKSUM_FILE = @"./Resources/SDE/checksum";
        private const string SDE_DIRECTORY = @"./Resources/SDE/";
        private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
        private const string SDE_TARGET_DIRECTORY = @"./Resources/SDE/universe";
        private const string SDE_EVE_TARGET_DIRECTORY = @"./Resources/SDE/universe/universe/eve";
        private const string SDE_WORMHOLE_TARGET_DIRECTORY = @"./Resources/SDE/universe/universe/wormhole";

        private readonly Mutex mut;
        private readonly ILogger<SDEServiceManager> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISDEDataSupplier _dataSupplier;
        private readonly ICacheService _cacheService;

        public SDEServiceManager(ILogger<SDEServiceManager> logger,
            IFileSystem fileSystem,
            ISDEDataSupplier dataSupplier,
            ICacheService cacheService)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _dataSupplier = dataSupplier;
            _cacheService = cacheService;
            mut = new Mutex();
        }

        public bool IsExtractionSuccesful()
        {
            return _fileSystem.Directory.Exists(SDE_TARGET_DIRECTORY);
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
                if(!_fileSystem.File.Exists(SDE_CHECKSUM_FILE))
                {
                    return string.Empty;
                }

                var fileContent = _fileSystem.File.ReadLines(SDE_CHECKSUM_FILE);
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
                if (_fileSystem.Directory.Exists(SDE_DIRECTORY))
                {
                    _logger.LogInformation($"Deleting Eve SDE resources at {SDE_DIRECTORY}");
                    _fileSystem.Directory.Delete(SDE_DIRECTORY, true);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearSDEResources");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Downloads the SDE package to the local target folder. 
        /// </summary>
        public async Task<bool> DownloadSDE()
        {
            try
            {
                if (!_fileSystem.Directory.Exists(SDE_DIRECTORY))
                {
                    _fileSystem.Directory.CreateDirectory(SDE_DIRECTORY);
                }

                _logger.LogInformation("Start to download Eve SDE files");

                using (var sdeData = await _dataSupplier.GetSDEDataStreamAsync())
                {
                    using (Stream fs = _fileSystem.FileStream.New(SDE_ZIP_PATH, FileMode.OpenOrCreate))
                    {
                        await sdeData.CopyToAsync(fs);
                    }
                }

                var checksum = _dataSupplier.GetChecksum();
                await _fileSystem.File.WriteAllTextAsync(SDE_CHECKSUM_FILE, checksum);

                _logger.LogInformation("Eve SDE files downloaded");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Download SDE");
                return false;
            }
        }

        /// <summary>
        /// Extracts the local SDE Zip package to the SDE Target folder.
        /// </summary>
        public Task<bool> ExtractSDE()
        {
            if (mut.WaitOne())
            {
                try
                {
                    _logger.LogInformation("Start to extract Eve SDE files");

                    //Check if ZIP exists
                    if (!_fileSystem.File.Exists(SDE_ZIP_PATH))
                    {
                        _logger.LogError("Impossible to extract SDE, no zip file");
                        return Task.FromResult(false);
                    }

                    //Get a stream that reads the ZIP from the filesystem
                    using (var archiveStream = _fileSystem.FileStream.New(SDE_ZIP_PATH, FileMode.Open))
                    {
                        //Extract the stream to the SDE target directory 
                        using (var archive = _fileSystem.ZipArchive().New(archiveStream))
                        {
                            archive.ExtractToDirectory(SDE_TARGET_DIRECTORY);
                        }
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

        /// <summary>
        /// Clears the SDE entries from the cache service.
        /// </summary>
        public async Task<bool> ClearCache()
        {
            try
            {
                await _cacheService.Remove(SDEConstants.REDIS_SDE_SOLAR_SYSTEMS_KEY);
                await _cacheService.Remove(SDEConstants.REDIS_SOLAR_SYSTEM_JUMPS_KEY);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearCache");
                return false;
            }
        }

        /// <summary>
        /// Imports the local SDE files into the cache. 
        /// </summary>
                public async Task<bool> BuildCache()
        {
            try
            {
                if (!this.IsExtractionSuccesful())
                {
                    _logger.LogError("Impossible to import SDE, bad SDE extract.");
                    return false;
                }

                var collectionOfFiles = await FindSDEFilesAsync();
                if (collectionOfFiles.Count == 0)
                {
                    _logger.LogError("Impossible to rebuild solar system cache, no files were found: probably a bad SDE extract.");
                    return false;
                }

                var collectionOfSolarSystems = await DeserializeSolarSystemsAsync(collectionOfFiles);
                var collectionOfJumps = BuildSolarSystemJumps(collectionOfSolarSystems);

                await UpdateCacheAsync(collectionOfSolarSystems, collectionOfJumps);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import SolarSystemJumpList and SolarSystemList");
                return false;
            }
        }

        private async Task<List<string>> FindSDEFilesAsync()
        {
            var searchPaths = new[] {
                Path.GetFullPath(SDE_WORMHOLE_TARGET_DIRECTORY),
                Path.GetFullPath(SDE_EVE_TARGET_DIRECTORY)
            };

            var collectionOfFiles = new List<string>();
            foreach (var path in searchPaths)
            {
                var result = _fileSystem.Directory.GetFiles(path, "solarsystem.yaml", SearchOption.AllDirectories);
                collectionOfFiles.AddRange(result);
            }

            return collectionOfFiles;
        }

        private async Task<BlockingCollection<SDESolarSystem>> DeserializeSolarSystemsAsync(List<string> collectionOfFiles)
        {
            var collectionOfSolarSystems = new BlockingCollection<SDESolarSystem>();
            Parallel.ForEach(collectionOfFiles, (file) =>
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                using (var textReader = File.OpenText(file))
                {
                    var solarSystem = deserializer.Deserialize<SDESolarSystem>(textReader);
                    var systemName = Path.GetFileName(Path.GetDirectoryName(file));
                    solarSystem.Name = string.IsNullOrEmpty(systemName) ? "Unknown" : systemName;

                    collectionOfSolarSystems.Add(solarSystem);
                }
            });

            return collectionOfSolarSystems;
        }

        private List<SolarSystemJump> BuildSolarSystemJumps(BlockingCollection<SDESolarSystem> collectionOfSolarSystems)
        {
            var collectionOfJumps = new List<SolarSystemJump>();
            foreach (var system in collectionOfSolarSystems)
            {
                var jumpSystemList = system.Stargates?.Values
                    .Select(stargate => collectionOfSolarSystems.FirstOrDefault(x => x.Stargates.ContainsKey(stargate.Destination)))
                    .Where(s => s != null)
                    .Select(s => new SolarSystem(s.SolarSystemID, s.Security))
                    .ToList() ?? new List<SolarSystem>();

                var result = new SolarSystemJump(system.SolarSystemID, system.Security, jumpSystemList);
                collectionOfJumps.Add(result);
            }

            return collectionOfJumps;
        }

        private async Task UpdateCacheAsync(BlockingCollection<SDESolarSystem> collectionOfSolarSystems, List<SolarSystemJump> collectionOfJumps)
        {
            await ClearCache();
            await _cacheService.Set(SDEConstants.REDIS_SDE_SOLAR_SYSTEMS_KEY, collectionOfSolarSystems);
            await _cacheService.Set(SDEConstants.REDIS_SOLAR_SYSTEM_JUMPS_KEY, collectionOfJumps);
        }
    }
}