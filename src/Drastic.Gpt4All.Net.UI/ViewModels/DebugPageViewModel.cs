using Drastic.Gpt4All.Net.UI.Services;
using Drastic.Tools;
using Drastic.ViewModels;
using Gpt4All;

namespace Drastic.Gpt4All.Net.UI.ViewModels
{
    public class DebugPageViewModel
        : BaseViewModel, IDisposable
    {
        private Gpt4AllModelService modelService;
        private string prompt = string.Empty;
        private string generatedText = string.Empty;
        private bool disposedValue;
        private IGpt4AllModel? model;

        public DebugPageViewModel(IServiceProvider services) : base(services)
        {
            this.modelService = services.GetService(typeof(Gpt4AllModelService)) as Gpt4AllModelService ?? throw new NullReferenceException(nameof(Gpt4AllModelService));

            this.modelService.OnUpdatedSelectedModel += this.ModelServiceOnUpdatedSelectedModel;
            this.modelService.OnAvailableModelsUpdate += this.ModelServiceOnAvailableModelsUpdate;

            this.GenerateCommand = new AsyncCommand(this.GenerateTextAsync, () => this.CanGenerateText, this.Dispatcher, this.ErrorHandler);
        }

        public bool CanGenerateText => this.modelService.SelectedModel != null && this.modelService.SelectedModel.Exists && !string.IsNullOrEmpty(this.Prompt);

        public Gpt4AllModelService ModelService => this.modelService;

        public AsyncCommand GenerateCommand { get; }

        /// <summary>
        /// Gets or sets the Prompt.
        /// </summary>
        public string Prompt {
            get { return this.prompt; }
            set { this.SetProperty(ref this.prompt, value); this.RaiseCanExecuteChanged(); }
        }

        /// <summary>
        /// Gets or sets the Generated Text.
        /// </summary>
        public string GeneratedText {
            get { return this.generatedText; }
            set { this.SetProperty(ref this.generatedText, value); }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private async Task GenerateTextAsync()
        {
            if (this.modelService.SelectedModel == null)
            {
                return;
            }

            this.GeneratedText = string.Empty;
            var modelFactory = new Gpt4AllModelFactory();
            if (this.model is not null)
                this.model.Dispose();

            this.model = modelFactory.LoadModel(this.modelService.SelectedModel.FileLocation);
            var result = await this.model.GetStreamingPredictionAsync(
               prompt,
               PredictRequestOptions.Defaults);

            await foreach (var token in result.GetPredictionStreamingAsync())
            {
                if (token is null) continue;
                this.GeneratedText += token;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.modelService.OnUpdatedSelectedModel -= this.ModelServiceOnUpdatedSelectedModel;
                }

                disposedValue = true;
            }
        }

        public override async Task OnLoad()
        {
            await base.OnLoad();

            if (!this.modelService.IsInitialized)
            {
                await this.modelService.InitializeFromCacheAsync();
            }
        }

        private void ModelServiceOnUpdatedSelectedModel(object? sender, EventArgs e)
        {
            this.RaiseCanExecuteChanged();
        }

        private void ModelServiceOnAvailableModelsUpdate(object? sender, EventArgs e)
        {
            this.RaiseCanExecuteChanged();
        }

        public override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.GenerateCommand.RaiseCanExecuteChanged();
        }
    }
}
