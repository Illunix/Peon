using Peon.CLI.Interfaces;
using Peon.CLI.Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peon.CLI
{
    public class App
    {
        private readonly IModelReader _modelReader;
        private readonly IMarlaminService _marlaminService;
        private readonly IListfileService _listfileService;

        public App(IModelReader modelReader, IMarlaminService marlaminService, IListfileService listfileService)
        {
            _modelReader = modelReader;
            _marlaminService = marlaminService;
            _listfileService = listfileService;
        }

        public async Task Run(string[] args)
        {
            var levelSwitch = new LoggingLevelSwitch();

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Console(
                    outputTemplate: "{Message:lj}{NewLine}{Exception}");

            var rootCommand = BuildRootCommand(levelSwitch, loggerConfiguration);

            await rootCommand.InvokeAsync(args);
        }

        private RootCommand BuildRootCommand(LoggingLevelSwitch levelSwitch, LoggerConfiguration loggerConfiguration)
        {
            var rootCommand = new RootCommand();
            rootCommand.Add(BuildGetCommand(levelSwitch, loggerConfiguration));
            rootCommand.Add(BuildGenerateCommand(levelSwitch, loggerConfiguration));

            return rootCommand;
        }

        private Command BuildGetCommand(LoggingLevelSwitch levelSwitch, LoggerConfiguration loggerConfiguration)
        {
            try
            {
                var command = new Command("get")
                {
                    new Option<string?>("--model", () => null, "Path to model"),
                    new Option<string?>("--models-list", () => null, "Path to models list"),
                    new Option<string?>("--models-dir", () => null, "Path to models directory"),
                    new Option<string?>("--build-config", () => "Latest wow build config", "Set build config for casc storage from what files will be downloaded"),
                    new Option<string?>("--path", () => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Work"), "Path to directory where files will be saved"),
                    new Option<bool>("--verbose", () => false, "Set output to be verbose"),
                    new Option<string?>("--log-file", () => null, "Path to log file"),
                };

                command.Handler = CommandHandler.Create(async (GetCommandOptions options) =>
                {
                    if (options.Verbose)
                    {
                        levelSwitch.MinimumLevel = LogEventLevel.Debug;
                    }

                    if (!string.IsNullOrWhiteSpace(options.LogFile))
                    {
                        File.WriteAllText(options.LogFile, "");

                        loggerConfiguration
                            .WriteTo.File(options.LogFile!);
                    }

                    Log.Logger = loggerConfiguration.CreateLogger();

                    Log.Information("Waking up peon...");

                    if (string.IsNullOrWhiteSpace(options.Model) && string.IsNullOrWhiteSpace(options.ModelsList) && string.IsNullOrWhiteSpace(options.ModelsDir))
                    {
                        if (!string.IsNullOrWhiteSpace(options.BuildConfig))
                        {
                            Log.Error("You can't use build config arg without any model provided");
                        }
                        else if (!string.IsNullOrWhiteSpace(options.Path))
                        {
                            Log.Error("You can't use path arg without any model provided");
                        }

                        return;
                    }

                    await _listfileService.Initialize();

                    Log.Information("Work work...");

                    if (!string.IsNullOrWhiteSpace(options.Model))
                    {
                        if (!File.Exists(options.Model))
                        {
                            Console.WriteLine("Model does not exist, check your path is correct");
                        }

                        _modelReader.Read(options.Model);

                        var fileIds = _modelReader.GetFileIds();

                        await _marlaminService.DownloadFiles(options.Model, fileIds, options.Path, options.BuildConfig);
                    }

                    if (!string.IsNullOrWhiteSpace(options.ModelsList))
                    {
                        var models = await File.ReadAllLinesAsync(options.ModelsList, Encoding.UTF8);
                        var modelsList = models.ToList();

                        foreach (var model in modelsList)
                        {
                            if (!File.Exists(model) || !model.EndsWith(".m2") || !model.EndsWith(".wmo") || !model.EndsWith(".adt"))
                            {
                                var skippingModel = false;

                                do
                                {
                                    var response = new ConsoleKey();
                                    do
                                    {
                                        Log.Information($"Model: {model} does not exist, do you want continue?");
                                        response = Console.ReadKey(false).Key;
                                        if (response != ConsoleKey.Enter)
                                        {
                                            Console.WriteLine("");
                                        }
                                    } while (response != ConsoleKey.Y && response != ConsoleKey.N);

                                    skippingModel = response == ConsoleKey.Y;
                                } while (!skippingModel);

                                if (skippingModel)
                                {
                                    if (options.Verbose)
                                    {
                                        Log.Warning($"Skipping: {model}");
                                        continue;
                                    }
                                }

                                modelsList.RemoveAll(x => x == model);
                            }

                            _modelReader.Read(model);

                            var fileIds = _modelReader.GetFileIds();

                            await _marlaminService.DownloadFiles(options.ModelsList, fileIds, options.Path, options.BuildConfig);
                        }
                    }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(options.ModelsDir))
                        {
                            var models = Directory.EnumerateFiles(options.ModelsDir, "*.*", SearchOption.AllDirectories)
                                .Where(model => model.EndsWith(".m2") || model.EndsWith(".wmo") || model.EndsWith(".adt"));

                            if (!models.Any())
                            {
                                Log.Error("Models Directory does not contain any models");
                            }
                            else
                            {
                                foreach (var model in models)
                                {
                                    _modelReader.Read(model);

                                    var fileIds = _modelReader.GetFileIds();

                                    await _marlaminService.DownloadFiles(model, fileIds, options.Path, options.BuildConfig);
                                }
                            }
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Log.Error("Models directory does not exist, check your path is correct");
                    }
                });

                return command;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return null;
        }

        private Command BuildGenerateCommand(LoggingLevelSwitch levelSwitch, LoggerConfiguration loggerConfiguration)
        {
            try
            {
                var command = new Command("generate")
                {
                    new Option<string?>("--listfile", () => null, "Path to listfile"),
                    new Option<string?>("--model", () => null, "Path to model"),
                    new Option<string?>("--models-list", () => null, "Path to models list"),
                    new Option<string?>("--models-dir", () => null, "Path to models directory"),
                    new Option<bool>("--verbose", () => false, "Set output to be verbose"),
                    new Option<string?>("--log-file", () => null, "Path to log file"),
                };

                command.Handler = CommandHandler.Create(async (GenerateCommandOptions options) =>
                {
                    if (options.Verbose)
                    {
                        levelSwitch.MinimumLevel = LogEventLevel.Debug;
                    }

                    if (!string.IsNullOrWhiteSpace(options.LogFile))
                    {
                        loggerConfiguration
                            .WriteTo.File(options.LogFile!);
                    }

                    Log.Logger = loggerConfiguration.CreateLogger();

                    Log.Information("Waking up peon...");

                    if (string.IsNullOrWhiteSpace(options.Model) && string.IsNullOrWhiteSpace(options.ModelsList) && string.IsNullOrWhiteSpace(options.ModelsDir))
                    {
                        if (!string.IsNullOrWhiteSpace(options.Listfile))
                        {
                            Log.Error("You can't use listfile arg without any model provided");
                        }

                        return;
                    }

                    await _listfileService.Initialize();

                    Log.Information("Work work...");

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(options.Model))
                        {
                            if (!File.Exists(options.Model))
                            {
                                Console.WriteLine("Model does not exist, check your path is correct");
                            }

                            _modelReader.Read(options.Model);

                            var fileIds = _modelReader.GetFileIds();

                            await _listfileService.GenerateNew(options.Listfile, fileIds, options.Verbose);
                        }

                        if (!string.IsNullOrWhiteSpace(options.ModelsList))
                        {
                            var models = await File.ReadAllLinesAsync(options.ModelsList, Encoding.UTF8);
                            var modelsList = models.ToList();

                            foreach (var model in modelsList)
                            {
                                if (!File.Exists(model))
                                {
                                    if (!model.EndsWith(".m2") || !model.EndsWith(".wmo") || !model.EndsWith(".adt"))
                                    {
                                        var skippingModel = false;

                                        do
                                        {
                                            var response = new ConsoleKey();
                                            do
                                            {
                                                Log.Information($"Model: {model} does not exist, do you want continue?");
                                                response = Console.ReadKey(false).Key;
                                                if (response != ConsoleKey.Enter)
                                                {
                                                    Console.WriteLine("");
                                                }
                                            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

                                            skippingModel = response == ConsoleKey.Y;
                                        } while (!skippingModel);

                                        if (skippingModel)
                                        {
                                            if (options.Verbose)
                                            {
                                                Log.Warning($"Skipping: {model}");
                                                continue;
                                            }
                                        }

                                        modelsList.RemoveAll(x => x == model);
                                    }
                                }

                                _modelReader.Read(model);

                                var fileIds = _modelReader.GetFileIds();

                                await _listfileService.GenerateNew(options.Listfile, fileIds, options.Verbose);
                            }
                        }

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(options.ModelsDir))
                            {
                                var models = Directory.EnumerateFiles(options.ModelsDir, "*.*", SearchOption.AllDirectories)
                                    .Where(model => model.EndsWith(".m2") || model.EndsWith(".wmo") || model.EndsWith(".adt"));

                                if (!models.Any())
                                {
                                    Log.Error("Models Directory does not contain any models");
                                }
                                else
                                {
                                    foreach (var model in models)
                                    {
                                        _modelReader.Read(model);

                                        var fileIds = _modelReader.GetFileIds();

                                        await _listfileService.GenerateNew(options.Listfile, fileIds, options.Verbose);
                                    }
                                }
                            }
                        }
                        catch (DirectoryNotFoundException)
                        {
                            Log.Error("Models directory does not exist, check your path is correct");
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Error(ex.Message);
                    }
                });

                return command;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return null;
        }
    }
}