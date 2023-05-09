using System;
using System.IO.Compression;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.SDE;
using WHMapper.Services.Anoik;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WHMapper.Services.SDE
{
	public class SDEServices : ISDEServices
	{
        private readonly ILogger _logger;

        private const string _sdeZipPath = @"./Resources/SDE/sde.zip";
        private const string _sdeUniverseDirectoryPath= @"./Resources/SDE/universe";
        private const string _default_solarsystem_static_filename = "solarsystem.staticdata";


        public SDEServices(ILogger<SDEServices> logger)
		{
            _logger = logger;

            if (!Directory.Exists(_sdeUniverseDirectoryPath))
            {

                _logger.LogInformation("Extrat Eve SDE files");
                using (ZipArchive archive = ZipFile.OpenRead(_sdeZipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string extractPath = Path.Combine(_sdeUniverseDirectoryPath, entry.FullName);
                        string extractDirectoryName = Path.GetDirectoryName(extractPath);

                        if (!Directory.Exists(extractDirectoryName))
                            Directory.CreateDirectory(extractDirectoryName);


                        entry.ExtractToFile(extractPath, true);
                    }
                }
            }
        }


        public async Task<IEnumerable<SDESolarSystem>> SearchSystem(string value)
        {
            if (Directory.Exists(_sdeUniverseDirectoryPath) && !String.IsNullOrEmpty(value) && value.Length>2)
            {
                HashSet<SDESolarSystem>  results = new HashSet<SDESolarSystem>();
                var directories = (IEnumerable<string>)Directory.GetDirectories(_sdeUniverseDirectoryPath, $"{value}*", SearchOption.AllDirectories);


                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                    foreach (string directoryPath in directories)
                    {
                        var sdeFiles = Directory.GetFiles(directoryPath);
                        if (sdeFiles.Count() > 0 && sdeFiles.Any(x => x.Contains(_default_solarsystem_static_filename)))
                        {
                            using (TextReader text_reader = File.OpenText(sdeFiles[0]))
                            {
                                try
                                {
                                    var res = deserializer.Deserialize<SDESolarSystem>(text_reader);
                                    res.Name = Path.GetFileName(directoryPath);
                                    results.Add(res);
                                }
                                catch(Exception ex)
                                {
                                    _logger.LogError(ex,String.Format("Parsing sdefiles {0} Error", sdeFiles[0]));
                                }
                            }
                        }
                    }
                

                return results;
            }
            else
                return null;
        }
    }
}

