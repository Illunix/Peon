using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peon.CLI.Interfaces
{
    public interface IListfileService
    {
        Task Initialize();

        Task GenerateNew(string listfile, IReadOnlyList<uint> fileIds, bool verbose);

        string GetFilenameById(uint id);
    }
}