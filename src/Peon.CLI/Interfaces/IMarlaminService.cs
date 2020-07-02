using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peon.CLI.Interfaces
{
    public interface IMarlaminService
    {
        Task DownloadFiles(string model, IReadOnlyList<uint> fileIds, string path, string buildConfig = "");

        IReadOnlyList<string> GetDownloadedFilePaths();
    }
}