using Peon.CLI.Interfaces;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Peon.CLI.Services
{
    public class BattleNetService : IBattleNetService
    {
        private readonly IHttpClientFactory _httpFactory;

        public BattleNetService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<string> GetLatestWowBuildConfig()
        {
            var client = _httpFactory.CreateClient();
            var response = await client.GetStringAsync($"http://eu.patch.battle.net:1119/wow_beta/versions");

            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(response));
            using var reader = new StreamReader(stream);

            // Read useless lines.
            for (var i = 0; i < 2; ++i)
            {
                await reader.ReadLineAsync();
            }

            var line = reader.ReadLine();
            var array = line.Split('|');

            var buildConfig = array[1];

            return buildConfig;
        }
    }
}