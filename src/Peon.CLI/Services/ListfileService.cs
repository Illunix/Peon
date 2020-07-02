using Peon.CLI.Interfaces;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Peon.CLI.Services
{
    public class ListfileService : IListfileService
    {
        private IHttpClientFactory _httpFactory { get; set; }

        private static Dictionary<uint, string> _fileDataPair = new Dictionary<uint, string>();

        public ListfileService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task Initialize()
        {
            var listfile = "listfile.csv";

            if (!File.Exists(listfile))
            {
                var client = _httpFactory.CreateClient();
                var response = await client.GetAsync("https://wow.tools/casc/listfile/download/csv/unverified");

                var fullPathAndFilename = Path.Combine(Assembly.GetEntryAssembly().Location, @"..\", listfile);

                using var fileStream = File.Create(fullPathAndFilename);
                using var dataStream = await response.Content.ReadAsStreamAsync();

                await dataStream.CopyToAsync(fileStream);
            }
            else
            {
                using var reader = new StreamReader(listfile);

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var array = line.Split(';');

                    _fileDataPair.Add(uint.Parse(array[0]), array[1]);
                }
            }
        }

        public async Task GenerateNew(string listfile, IReadOnlyList<uint> fileIds, bool verbose)
        {
            File.WriteAllText(listfile, "");

            using var stream = File.Open(listfile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using var writer = new StreamWriter(stream);

            foreach (var fileId in fileIds)
            {
                var filename = GetFilenameById(fileId);

                if (verbose)
                {
                    Log.Debug($"Writing: {fileId};{filename}");
                }

                await writer.WriteLineAsync($"{fileId};{filename}");
            }

            writer.Close();
        }

        public string GetFilenameById(uint id)
        {
            if (_fileDataPair.TryGetValue(id, out string filename))
            {
                return filename;
            }

            return null;
        }
    }
}