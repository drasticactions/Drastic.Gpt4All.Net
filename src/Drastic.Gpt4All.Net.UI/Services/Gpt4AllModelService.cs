using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Drastic.Gpt4All.Net.UI.Models;
using Drastic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Drastic.Gpt4All.Net.UI.Services;

public class Gpt4AllModelService : INotifyPropertyChanged
{
    private const string modelListUrl = "https://gpt4all.io/models/models.json";
    private HttpClient httpClient;
    private IAppDispatcher dispatcher;
    private Gpt4AllWebModel? selectedModel;
    private string defaultPath;
    private bool isInitialized;

    public Gpt4AllModelService(IServiceProvider provider, string? defaultPath = null)
    {
        this.dispatcher = provider.GetRequiredService<IAppDispatcher>();
        this.httpClient = new HttpClient();
        this.defaultPath = defaultPath ?? Gpt4AllStatic.DefaultPath;
        this.UpdateAvailableModels();
    }
    
    public ObservableCollection<Gpt4AllWebModel> AllModels { get; } = new ObservableCollection<Gpt4AllWebModel>();

    public ObservableCollection<Gpt4AllWebModel> AvailableModels { get; } = new ObservableCollection<Gpt4AllWebModel>();
    
    public Gpt4AllWebModel? SelectedModel
    {
        get
        {
            return this.selectedModel;
        }

        set
        {
            this.SetProperty(ref this.selectedModel, value);
            this.OnUpdatedSelectedModel?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsInitialized {
        get {
            return this.isInitialized;
        }

        set {
            this.SetProperty(ref this.isInitialized, value);
        }
    }
    
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public event EventHandler? OnUpdatedSelectedModel;

    public event EventHandler? OnAvailableModelsUpdate;

    public async Task InitializeFromCacheAsync(string? modelsJsonPath = null)
    {
        string? defaultJson = null;
        modelsJsonPath ??= Path.Combine(this.defaultPath, "models.json");
        if (File.Exists(modelsJsonPath))
        {
            defaultJson = await File.ReadAllTextAsync(Path.Combine(this.defaultPath, "models.json"), Encoding.UTF8);
        }

        await InitializeAsync(defaultJson);
    }
    
    public async Task InitializeAsync(string? defaultJson = null)
    {
        string json = defaultJson ?? await this.httpClient.GetStringAsync(modelListUrl);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(this.defaultPath, "models.json"), json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e);
        }
        var models = JsonSerializer.Deserialize<List<Gpt4AllWebModel>>(json)!;
        foreach (var model in models)
        {
            model.FileLocation = Path.Combine(this.defaultPath, model.Filename);
            this.AllModels.Add(model);
        }
        this.UpdateAvailableModels();
    }
    
    public void UpdateAvailableModels()
    {
        lock (this)
        {
            this.dispatcher.Dispatch(() =>
            {
                this.AvailableModels.Clear();
                var models = this.AllModels.Where(n => n.Exists);
                foreach (var model in models)
                {
                    this.AvailableModels.Add(model);
                }

                if (this.SelectedModel is not null && !this.AvailableModels.Contains(this.SelectedModel))
                {
                    this.SelectedModel = null;
                }

                this.SelectedModel ??= this.AvailableModels.FirstOrDefault();
            });
        }

        this.OnAvailableModelsUpdate?.Invoke(this, EventArgs.Empty);
        this.IsInitialized = true;
    }

    
    /// <summary>
    /// On Property Changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        this.dispatcher?.Dispatch(() =>
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

#pragma warning disable SA1600 // Elements should be documented
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        onChanged?.Invoke();
        this.OnPropertyChanged(propertyName);
        return true;
    }
}