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

        private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
        private const string SDE_TARGET_DIRECTORY= @"./Resources/SDE/universe";
        private const string SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE = "solarsystem.staticdata";


        public SDEServices(ILogger<SDEServices> logger)
		{
            _logger = logger;

            if (!Directory.Exists(SDE_TARGET_DIRECTORY))
            {
                _logger.LogInformation("Extrat Eve SDE files");
                using (ZipArchive archive = ZipFile.OpenRead(SDE_ZIP_PATH))
                {
                    archive.ExtractToDirectory(SDE_TARGET_DIRECTORY);
                }
            }
        }


        public IEnumerable<SDESolarSystem>? SearchSystem(string value)
        {
            if (Directory.Exists(SDE_TARGET_DIRECTORY) && !String.IsNullOrEmpty(value) && value.Length > 2)
            {
                HashSet<SDESolarSystem> results = new HashSet<SDESolarSystem>();
                var directories = (IEnumerable<string>)Directory.GetDirectories(SDE_TARGET_DIRECTORY, $"{value}*", SearchOption.AllDirectories);


                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                foreach (string directoryPath in directories)
                {
                    var sdeFiles = Directory.GetFiles(directoryPath);
                    if (sdeFiles.Count() > 0 && sdeFiles.Any(x => x.Contains(SDE_DEFAULT_SOLARSYSTEM_STATIC_FILEMANE)))
                    {
                        using (TextReader text_reader = File.OpenText(sdeFiles[0]))
                        {
                            try
                            {
                                var res = deserializer.Deserialize<SDESolarSystem>(text_reader);
                                res.Name = Path.GetFileName(directoryPath);
                                results.Add(res);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, String.Format("Parsing sdefiles {0} Error", sdeFiles[0]));
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

