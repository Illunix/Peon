using System.Collections.Generic;

namespace Peon.CLI.Interfaces
{
    public interface IModelReader
    {
        void Read(string model);

        IReadOnlyList<uint> GetModelModelsFilesIds(string file, IReadOnlyList<string> downloadedFilePaths);

        IReadOnlyList<uint> GetFileIds();

        void ClearFileIds();
    }
}