using Peon.CLI.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Peon.CLI.Services
{
    public class MarlaminService : IMarlaminService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IModelReader _modelReader;
        private readonly IListfileService _listfileService;
        private readonly IBattleNetService _battleNetService;

        private List<string> _downloadedFilePaths = new List<string>();

        public MarlaminService(IHttpClientFactory httpFactory, IModelReader modelReader, IListfileService listfileService, IBattleNetService battleNetService)
        {
            _httpFactory = httpFactory;
            _modelReader = modelReader;
            _listfileService = listfileService;
            _battleNetService = battleNetService;
        }

        public async Task DownloadFiles(string model, IReadOnlyList<uint> fileIds, string path, string buildConfig)
        {
            if (fileIds.Any())
            {
                await DownloadFiles(fileIds, path, buildConfig);

                if (model.EndsWith(".wmo") || model.EndsWith(".adt"))
                {
                    await DownloadFiles(model, path, buildConfig);
                }
            }
        }

        public IReadOnlyList<string> GetDownloadedFilePaths()
        {
            return _downloadedFilePaths;
        }

        private async Task DownloadFiles(IReadOnlyList<uint> fileIds, string path, string buildConfig)
        {
            foreach (var fileId in fileIds)
            {
                var filename = _listfileService.GetFilenameById(fileId);

                Log.Debug($"Downloading: {fileId};{filename}");

                await DownloadFile(fileId, filename, path, buildConfig);
            }
        }

        private async Task DownloadFiles(string model, string path, string buildConfig)
        {
            var fileIds = _modelReader.GetModelModelsFilesIds(model, _downloadedFilePaths);

            if (fileIds.Any())
            {
                await DownloadFiles(fileIds, path, buildConfig);
            }
        }

        private async Task DownloadFile(uint fileId, string filename, string getDirectory, string buildConfig = "")
        {
            if (string.IsNullOrWhiteSpace(buildConfig))
            {
                buildConfig = await _battleNetService.GetLatestWowBuildConfig();
            }

            var client = _httpFactory.CreateClient();
            var response = await client.GetAsync($"https://wow.tools/casc/file/fdid?buildconfig={buildConfig}&filename={filename}&filedataid={fileId}");

            var filePath = Path.GetDirectoryName(filename);

            if (getDirectory is null)
            {
                getDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Work");
            }

            var fullDirPath = Path.Combine(getDirectory, filePath);

            if (!Directory.Exists(fullDirPath))
            {
                Directory.CreateDirectory(fullDirPath);
            }

            var justFilename = Path.GetFileName(filename);
            var fullPathAndFilename = Path.Combine(fullDirPath, justFilename);

            _downloadedFilePaths.Add(fullPathAndFilename);
            using var fileStream = File.Create(fullPathAndFilename);
            using var dataStream = await response.Content.ReadAsStreamAsync();
            await dataStream.CopyToAsync(fileStream);
        }
    }
}