using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Gpt4All.Net.UI.Models;
using Drastic.Gpt4All.Net.UI.Services;
using Drastic.Services;
using Gpt4All;
using Gpt4All.LibraryLoader;
using Microsoft.Extensions.DependencyInjection;
using Sharprompt;

internal class MainProgram
{
    private RootCommand root;
    private string driverType;
    private string[] args;
    private Gpt4AllModelService modelService;
    
    public MainProgram(string[] args)
    {
        this.driverType = "Generic";
        this.args = args;
        
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<IErrorHandlerService>(new BasicErrorHandler())
                .AddSingleton<IAppDispatcher>(new AppDispatcher())
                .AddSingleton<Gpt4AllModelService>()
                .BuildServiceProvider());

        this.modelService = Ioc.Default.GetRequiredService<Gpt4AllModelService>();
        
        this.root = new RootCommand
        {
            Handler = CommandHandler.Create(LocalFiles),
        };
        
        this.root.Add(new Option<string>("--model", "GPT4All Model"));
        this.root.Add(new Option<string>("--prompt", "Prompt to use with GPT4All"));
    }

    private async Task LocalFiles(string model, string prompt)
    {
        model = await this.GetModelPrompt(model);
        prompt ??= Prompt.Input<string>("Enter a prompt");
        var modelFactory = new Gpt4AllModelFactory();
        using var loadedModel = modelFactory.LoadModel(model);
        var result = await loadedModel.GetStreamingPredictionAsync(
            prompt,
            PredictRequestOptions.Defaults);

        await foreach (var token in result.GetPredictionStreamingAsync())
        {
            Console.Write(token);
        }
    }
    
    private async Task<string> GetModelPrompt(string? modelPath = "")
    {
        if (File.Exists(modelPath))
        {
            return modelPath;
        }

        var model = Prompt.Select("Select a model", this.modelService.AllModels, textSelector: (item) => item.Filename);
        if (model.Exists)
        {
            return model.FileLocation;
        }

        Console.WriteLine("Downloading Model...");
        var gpt4AllWebDownloader =
            new Gpt4AllWebDownloader(model, this.modelService, Ioc.Default.GetRequiredService<IAppDispatcher>());
        gpt4AllWebDownloader.DownloadService.DownloadProgressChanged += (s, e) =>
        {
            //progressBar.Update((int)e.ProgressPercentage);
        };
        await gpt4AllWebDownloader.DownloadCommand.ExecuteAsync();

        return model.FileLocation;
    }
    
    public async Task<int> RunAsync()
    {
        await this.modelService.InitializeFromCacheAsync();
        return await this.root.InvokeAsync(this.args);
    }
    
    internal class BasicErrorHandler : IErrorHandlerService
    {
        public void HandleError(Exception ex)
        {
        }
    }

    internal class AppDispatcher : IAppDispatcher
    {
        public bool Dispatch(Action action)
        {
            action.Invoke();
            return true;
        }
    }
}
internal class Program
{
    internal static MainProgram? program;
    
    private static async Task<int> Main(string[] args)
    {
        NativeLibraryLoader.LoadNativeLibrary();
        
        program = new MainProgram(args);
        return await program.RunAsync();
    }
}