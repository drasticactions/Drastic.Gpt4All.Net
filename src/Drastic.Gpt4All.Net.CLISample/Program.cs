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
        this.root.Add(new Option<string>("--prompt", "Initial Prompt to use with GPT4All"));
        this.root.Add(new Option<bool>("--interactive", "Interactive chat session"));
    }

    private async Task LocalFiles(string model, string? prompt, bool interactive)
    {
        model = await this.GetModelPrompt(model);
        var chatSession = new ChatSession(model);
        

        do
        {
            prompt ??= Prompt.Input<string>("Enter a prompt");
            await chatSession.PromptAsync(prompt);
            prompt = null;
        } while (interactive);
    }

    public class ChatSession : IPromptFormatter
    {
        private IGpt4AllModel model;
        private List<RequestResponse> requestResponses = new List<RequestResponse>();
        
        public ChatSession(string model)
        {
            var modelFactory = new Gpt4AllModelFactory();
            this.model = modelFactory.LoadModel(model);
            this.model.PromptFormatter = this;
        }

        public async Task PromptAsync(string prompt)
        {
            var result = await this.model.GetStreamingPredictionAsync(
                prompt,
                PredictRequestOptions.Defaults);

            var aiAnswer = string.Empty;
            await foreach (var token in result.GetPredictionStreamingAsync())
            {
                Console.Write(token);
                aiAnswer += token;
            }
            
            Console.WriteLine(Environment.NewLine);
            requestResponses.Add(new RequestResponse(prompt, aiAnswer));
        }

        public string FormatPrompt(string prompt)
        {
            var result = $"""
        ### Instruction: 
        You are a chat bot where the user will talk to you.  You will respond to the user's input.
        {this.requestResponses.Aggregate(string.Empty, (current, item) => current + item.ToString())}
        ### Prompt:
        {prompt}
        ### Response:
        """;
            return result;
        }
        
        private class RequestResponse
        {
            public string Request { get; }
            
            public string Response { get; }

            public RequestResponse(string request, string response)
            {
                this.Request = request;
                this.Response = response;
            }

            public override string ToString()
            {
                return $"""
        ### Prompt:
        {this.Request}
        ### Response:
        {this.Response}
        """;
            }
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
        program = new MainProgram(args);
        return await program.RunAsync();
    }
}