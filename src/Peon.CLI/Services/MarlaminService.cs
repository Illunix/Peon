using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

namespace Peon.CLI.Services
{
    internal class MarlaminService
    {
        internal async Task DownloadFile(uint fileId, string filename, string getDirectory)
        {
            var httpClient = new HttpClient();
            var battleNetService = new BattleNetService();

            var buildConfig = battleNetService.GetLastestWowBuildConfig();
            var response = await httpClient.GetAsync(($"https://wow.tools/casc/file/fdid?buildconfig={buildConfig}&filename={filename}&filedataid={fileId}"));

            var filePath = Path.GetDirectoryName(filename);
            var fullDirPath = Path.Combine(getDirectory, filePath);

            if (!Directory.Exists(fullDirPath))
            {
                Directory.CreateDirectory(fullDirPath);
            }

            var justFilename = Path.GetFileName(filename);
            var fullPathAndFilename = Path.Combine(fullDirPath, justFilename);

            using var fileStream = File.Create(fullPathAndFilename);
            using var dataStream = await response.Content.ReadAsStreamAsync();
            await dataStream.CopyToAsync(fileStream);
        }
    }
}
