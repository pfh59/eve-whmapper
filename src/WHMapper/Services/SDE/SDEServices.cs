using System;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.IIS.Core;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        private const string SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE = "solarsystem.staticdata";

        private readonly ILogger _logger;
        private readonly ParallelOptions _options;
        private readonly IDeserializer _deserializer;
        private readonly EnumerationOptions _directorySearchOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true };
        private static Mutex mut = new Mutex();

        public bool ExtractSuccess {get; private set;}
        
        public SDEServices(ILogger<SDEServices> logger)
		{
            _logger = logger;
            _options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            ExtractSuccess= false;
            if(mut.WaitOne())
            {
                try
                {
                    if(IsNewSDEAvailable())
                    {
                        if (Directory.Exists(SDE_TARGET_DIRECTORY))
                            Directory.Delete(SDE_TARGET_DIRECTORY,true);
                        DownloadSDE();
                    }

                    if (!Directory.Exists(SDE_TARGET_DIRECTORY))
                    {
                        _logger.LogInformation("Extrat Eve SDE files");
                        using (ZipArchive archive = ZipFile.OpenRead(SDE_ZIP_PATH))
                        {
                            archive.ExtractToDirectory(SDE_TARGET_DIRECTORY);
                        }
                    }
                    ExtractSuccess= true;
                }
                catch(Exception ex)
                {
                    logger.LogError("SDEServices",ex);
                    ExtractSuccess= false;
                }
                finally
                {
                    mut.ReleaseMutex();
                }
            }
        }
        public bool IsNewSDEAvailable()
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
                    return true;
                }
                else
                {
                    return !File.ReadLines(SDE_CHECKSUM_CURRENT_FILE).SequenceEqual(File.ReadLines(SDE_CHECKSUM_FILE));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check SDE");
                return false;
            }
        }
        
        private void DownloadSDE()
        {
            using (var client = new HttpClient())
            {
                using (var s = client.GetStreamAsync(SDE_ZIP_URL))
                {
                    using (var fs = new FileStream(SDE_ZIP_PATH, FileMode.OpenOrCreate))
                    {
                        s.Result.CopyTo(fs);
                    }
                }
            }
        }
        
        public async Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value)
        {
            if(mut.WaitOne())
            {
                try
                {
                    if(!ExtractSuccess)
                    {
                        _logger.LogError("Impossible to searchSystem, bad extract.");
                        return null;
                    }

                    if (Directory.Exists(SDE_TARGET_DIRECTORY) && !String.IsNullOrEmpty(value) && value.Length > 2)
                    {
                        HashSet<SDESolarSystem> results = new HashSet<SDESolarSystem>();
                        var directories = (IEnumerable<string>)Directory.EnumerateDirectories(SDE_TARGET_DIRECTORY, $"{value}*", _directorySearchOptions) ;


                        Parallel.ForEach(directories.Take(20), _options, async (directoryPath, token) =>
                        {
                            var sdeFiles = Directory.GetFiles(directoryPath);
                            if (sdeFiles.Count() > 0 && sdeFiles.Any(x => x.Contains(SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE)))
                            {
                                using (TextReader text_reader = File.OpenText(sdeFiles[0]))
                                {
                                    try
                                    {
                                        var res = _deserializer.Deserialize<SDESolarSystem>(text_reader);
                                        res.Name = Path.GetFileName(directoryPath);
                                        results.Add(res);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, String.Format("Parsing sdefiles {0} Error", sdeFiles[0]));
                                    }
                                }
                            }
                           await Task.Yield();
                        });

                        return results;
                    }
                    else
                        return null;
                }
                catch(Exception ex)
                {
                    _logger.LogError("SearchSystem",ex);
                    return null;
                }
                finally
                {
                    mut.ReleaseMutex();
                }
            }
            return null;
        }
    }
}

