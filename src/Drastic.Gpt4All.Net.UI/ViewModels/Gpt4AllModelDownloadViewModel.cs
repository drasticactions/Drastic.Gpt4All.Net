using Drastic.Gpt4All.Net.UI.Models;
using Drastic.Gpt4All.Net.UI.Services;
using Drastic.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drastic.Gpt4All.Net.UI.ViewModels
{
    public class Gpt4AllModelDownloadViewModel
        : BaseViewModel
    {
        private Gpt4AllModelService modelService;

        public Gpt4AllModelDownloadViewModel(IServiceProvider services) : base(services)
        {
            this.modelService = services.GetService(typeof(Gpt4AllModelService)) as Gpt4AllModelService ?? throw new NullReferenceException(nameof(Gpt4AllModelService));
        }

        public Gpt4AllModelService ModelService => this.modelService;

        public ObservableCollection<Gpt4AllWebDownloader> Downloads { get; } = new ObservableCollection<Gpt4AllWebDownloader>();

        public override async Task OnLoad()
        {
            await base.OnLoad();

            if (!this.modelService.IsInitialized)
            {
                await this.UpdateModels();
            }
        }

        private async Task UpdateModels()
        {
            await this.modelService.InitializeFromCacheAsync();

            this.Dispatcher.Dispatch(() =>
            {
                this.Downloads.Clear();
                foreach (var item in this.modelService.AllModels)
                {
                    this.Downloads.Add(new Gpt4AllWebDownloader(item, this.modelService, this.Dispatcher));
                }
            });
        }
    }
}
