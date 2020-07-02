using Microsoft.Extensions.DependencyInjection;
using Peon.CLI.Interfaces;
using Peon.CLI.Services;
using Serilog;
using System.Threading.Tasks;

namespace Peon.CLI
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            await serviceProvider.GetService<App>().Run(args);

            Log.Information("Jobs Done!");
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<App>();
            services.AddHttpClient();
            services.AddSingleton<IModelReader, ModelReader>();
            services.AddSingleton<IMarlaminService, MarlaminService>();
            services.AddSingleton<IBattleNetService, BattleNetService>();
            services.AddSingleton<IListfileService, ListfileService>();

            return services;
        }
    }
}