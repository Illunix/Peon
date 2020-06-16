using Peon.CLI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CommandLine;
using System.IO;
using System.Reflection;

namespace Peon.CLI
{
    [Verb("get")]
    internal class GetOptions
    {
        [Option("m2", Required = false)]
        public string M2 { get; set; }

        [Option("textures", Required = false)]
        public bool GetTextures { get; set; }

        [Option("path", Required = false)]
        public string Path { get; set; }
    }

    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.Write("Waking up peon...\n");

            return await Parser.Default.ParseArguments<GetOptions>(args)
            .MapResult(
               (GetOptions options) => RunOptions(options),
               errs => Task.FromResult(0)
            );
        }

        private static async Task<int> RunOptions(GetOptions options)
        {
            if (options.GetTextures)
            {
                if (!string.IsNullOrWhiteSpace(options.M2))
                {
                    if (!File.Exists(options.M2))
                    {
                        Console.WriteLine("M2 does not exist, check if path is correct");
                    }
                    else
                    {
                        var m2Service = new M2Service();

                        var textures = m2Service.GetAllTextures(options.M2);

                        var marlaminService = new MarlaminService();

                        var listfileService = new ListfileService();

                        listfileService.Initialize();

                        Console.WriteLine("Work work...");

                        if (string.IsNullOrWhiteSpace(options.Path))
                        {
                            foreach (var texture in textures)
                            {
                                var fileId = texture.FileDataId;
                                var filename = listfileService.GetFilenameById(fileId);

                                await marlaminService.DownloadFile(fileId, filename, Path.Combine(Assembly.GetEntryAssembly().Location, @"..\Work"));
                            }

                            Console.WriteLine("Jobs done!");
                        }
                        else
                        {
                            foreach (var texture in textures)
                            {
                                var fileId = texture.FileDataId;
                                var filename = listfileService.GetFilenameById(fileId);

                                await marlaminService.DownloadFile(fileId, filename, options.Path);
                            }

                            Console.WriteLine("Jobs done!");
                        }
                    }
                }
            }

            return 0;
        }
    }
}
