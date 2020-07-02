using System.Threading.Tasks;

namespace Peon.CLI.Interfaces
{
    public interface IBattleNetService
    {
        Task<string> GetLatestWowBuildConfig();
    }
}