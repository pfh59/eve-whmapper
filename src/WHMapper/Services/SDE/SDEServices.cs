using System.Collections.Concurrent;
using System.IO.Compression;
using WHMapper.Models.DTO.SDE;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using WHMapper.Services.Cache;

namespace WHMapper.Services.SDE
{
	public class SDEServices : ISDEServices
	{
        private const string SDE_URL_CHECKSUM = "https://eve-static-data-export.s3-eu-west-1.amazonaws.com/tranquility/checksum";
        private const string SDE_CHECKSUM_FILE = @"./Resources/SDE/checksum";
        private const string SDE_CHECKSUM_CURRENT_FILE = @"./Resources/SDE/currentchecksum";

        private const string SDE_DIRECTORY =@"./Resources/SDE/";
        private const string SDE_ZIP_URL= "https://eve-static-data-export.s3-eu-west-1.amazonaws.com/tranquility/sde.zip";
        private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
        private const string SDE_TARGET_DIRECTORY= @"./Resources/SDE/universe";

        private const string SDE_EVE_TARGET_DIRECTORY= @"./Resources/SDE/universe/sde/fsd/universe/eve";
        private const string SDE_WORMHOLE_TARGET_DIRECTORY= @"./Resources/SDE/universe/sde/fsd/universe/wormhole";
        private const string SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE = "solarsystem.staticdata";

        private const string REDIS_SOLAR_SYSTEM_JUMPS_KEY = "solarysystemjumps:list";
        private const string REDIS_SDE_SOLAR_SYSTEMS_KEY = "solarsystems:list";

        private readonly ILogger _logger;
        private readonly ParallelOptions _options;
        private readonly IDeserializer _deserializer;
        private readonly EnumerationOptions _directorySearchOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true };
        private static Mutex mut = new Mutex();

        private readonly ICacheService _cacheService;

        public bool ExtractSuccess 
        {
            get
            {
                return Directory.Exists(SDE_TARGET_DIRECTORY);
            }
        }
       
        public SDEServices(ILogger<SDEServices> logger,ICacheService cacheService)
		{
            _logger = logger;
            _cacheService=cacheService;
            _options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build(); 
        }
        public Task<bool> IsNewSDEAvailable()
        {
            try
            {
                if (!Directory.Exists(SDE_DIRECTORY))
                    Directory.CreateDirectory(SDE_DIRECTORY);

                using (var client = new HttpClient())
                {
                    using (var s = client.GetStreamAsync(SDE_URL_CHECKSUM))
                    {
                        using (var fs = new FileStream(SDE_CHECKSUM_FILE, FileMode.OpenOrCreate))
                        {
                            s.Result.CopyTo(fs);
                        }
                    }
                }

                if (!File.Exists(SDE_CHECKSUM_CURRENT_FILE))
                {
                    File.Move(SDE_CHECKSUM_FILE, SDE_CHECKSUM_CURRENT_FILE);
                    return Task.FromResult(true);
                }
                else
                {
                    bool isSame = !File.ReadLines(SDE_CHECKSUM_CURRENT_FILE).SequenceEqual(File.ReadLines(SDE_CHECKSUM_FILE));
                    File.Delete(SDE_CHECKSUM_CURRENT_FILE);
                    File.Move(SDE_CHECKSUM_FILE, SDE_CHECKSUM_CURRENT_FILE);
                    return Task.FromResult(isSame);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check SDE");
                return Task.FromResult(false);
            }
        }

        public Task<bool> ClearSDERessources()
        {
            try
            {
                if (Directory.Exists(SDE_DIRECTORY))
                {
                    _logger.LogInformation("Delete old Eve SDE files");
                    Directory.Delete(SDE_DIRECTORY, true);
                }

                return Task.FromResult(true);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "ClearSDERessources");
                return Task.FromResult(false);
            }
        }

        public async Task<bool> DownloadSDE()
        {
            try
            {
                if(!Directory.Exists(SDE_DIRECTORY))
                    Directory.CreateDirectory(SDE_DIRECTORY);

                _logger.LogInformation("Start to download Eve SDE files");
                HttpClient client = new HttpClient();
                Stream stream = await client.GetStreamAsync(SDE_ZIP_URL);
                
                FileStream fs = new FileStream(SDE_ZIP_PATH, FileMode.OpenOrCreate);
                stream.CopyTo(fs);
                _logger.LogInformation("Eve SDE files downloaded");
                return true;
            }
            catch(Exception ex)
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
                    if (Directory.Exists(SDE_TARGET_DIRECTORY))
                    {
                        _logger.LogInformation("Delete old Eve SDE files");
                        Directory.Delete(SDE_TARGET_DIRECTORY, true);
                    }

                    if(!File.Exists(SDE_ZIP_PATH))
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
                    return Task.FromResult(false);
                }
                finally
                {
                    mut.ReleaseMutex();
                }
            }
            return Task.FromResult(false);
        }

        public  async Task<bool> ClearCache()
        {
            try
            {
                 await _cacheService.Remove(REDIS_SDE_SOLAR_SYSTEMS_KEY);
                 await _cacheService.Remove(REDIS_SOLAR_SYSTEM_JUMPS_KEY);
                
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"ClearCache");
                return false;
            }
        }

        public async Task<bool> Import()
        {
            try
            {
                if(!ExtractSuccess)
                {
                    _logger.LogError("Impossible to import SDE, bad SDE extract.");
                    return false;;
                }

             
                BlockingCollection<SolarSystemJump> tmp = new BlockingCollection<SolarSystemJump>();
                BlockingCollection<SDESolarSystem> SDESystems = new BlockingCollection<SDESolarSystem>();
                var sdeFiles = Directory.GetFiles(SDE_TARGET_DIRECTORY, SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE, _directorySearchOptions);
                if (sdeFiles.Count() > 0)
                {
                    Parallel.ForEach(sdeFiles, _options, async (directoryPath, token) =>
                    {
                        if(directoryPath.Contains(SDE_EVE_TARGET_DIRECTORY) || directoryPath.Contains(SDE_WORMHOLE_TARGET_DIRECTORY))
                        {
                            TextReader text_reader = File.OpenText(directoryPath);
                            var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                .IgnoreUnmatchedProperties()
                                .Build(); 
                            SDESolarSystem solarSystem = deserializer.Deserialize<SDESolarSystem>(text_reader);
                            solarSystem.Name = Path.GetFileName(Path.GetDirectoryName(directoryPath));
                            
                            while(!SDESystems.TryAdd(solarSystem))
                                await Task.Delay(1);   

                        }
                        await Task.Yield();
                    });



                    //after all sde system loading build SolarSystemJumps
                    Parallel.ForEach(SDESystems, _options, async (system, token) =>
                    {
                        SolarSystemJump solarSystemJump = null;
                        if(system.Stargates==null || system.Stargates.Count==0)
                            solarSystemJump = new SolarSystemJump(system.SolarSystemID,system.Security);
                        else
                        {
                            var JumpSystemList = new List<SolarSystem>();
                            Parallel.ForEach(system.Stargates.Values, _options, async (stargate, token) =>
                            {
                                var s =  SDESystems.FirstOrDefault(x=>x.Stargates.ContainsKey(stargate.Destination));
                                if(s!=null)
                                    JumpSystemList.Add(new SolarSystem(s.SolarSystemID,s.Security));
                                
                                await Task.Yield();
                            });
                            solarSystemJump = new SolarSystemJump(system.SolarSystemID,system.Security,JumpSystemList);
                        }

                        while(!tmp.TryAdd(solarSystemJump))
                            await Task.Delay(1);  

                        await Task.Yield();

                    });

                    
                    //clean old cache and add new one
                    await ClearCache();
                    await _cacheService.Set(REDIS_SDE_SOLAR_SYSTEMS_KEY,SDESystems);
                    await _cacheService.Set(REDIS_SOLAR_SYSTEM_JUMPS_KEY,tmp);
                   
                    return true;
                }
                else
                {
                    _logger.LogError("Impossible to importSolarSystemJumpList, bad SDE extract.");
                    return false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Import SolarSystemJumpLis and SolarSystemList");
                return false;
            }
        }

        public async Task<IEnumerable<SDESolarSystem>?> GetSolarSystemList() 
        {
            try
            {
                IEnumerable<SDESolarSystem>? results = await _cacheService.Get<IEnumerable<SDESolarSystem>?>(REDIS_SDE_SOLAR_SYSTEMS_KEY);
                if(results==null)
                    return new List<SDESolarSystem>();

                return results;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"GetSolarSystemList");
                return null;
            }
        }

        public async Task<IEnumerable<SolarSystemJump>?> GetSolarSystemJumpList() 
        {
            try
            {
                IEnumerable<SolarSystemJump>? results = await _cacheService.Get<IEnumerable<SolarSystemJump>?>(REDIS_SOLAR_SYSTEM_JUMPS_KEY);
                if(results==null)
                    return new List<SolarSystemJump>();

                return results;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"GetSolarSystemJumpList");
                return null;
            }
        }  

        public async Task<SDESolarSystem?> SearchSystemById(int value)
        {
            try
            {
                if(!ExtractSuccess)
                {
                    _logger.LogError("Impossible to searchSystem, bad SDE extract.");
                    return null;
                }

                var SDESystems = await GetSolarSystemList();
                if(SDESystems==null)
                {
                    _logger.LogError("Impossible to searchSystem, Empty SDE solar system list.");
                    return null;
                }

                var result = SDESystems.Where(x => x.SolarSystemID==value).FirstOrDefault();
                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"SearchSystem");
                return null;
            }
        }


        public async Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value)
        {
            try
            {
                if(!ExtractSuccess)
                {
                    _logger.LogError("Impossible to searchSystem, bad SDE extract.");
                    return null;
                }

                var SDESystems = await GetSolarSystemList();
                if(SDESystems==null)
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
                    return null;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"SearchSystem");
                return null;
            }
        }
    }
}




