using Drastic.Gpt4All.Net.MauiSample.Services;
using Drastic.Gpt4All.Net.UI.ViewModels;
using Drastic.Tools;

namespace Drastic.Gpt4All.Net.MauiSample;

public partial class MainPage : ContentPage
{
    private IServiceProvider provider;

    public MainPage(IServiceProvider serviceProvider)
	{
		InitializeComponent();
		this.provider = serviceProvider;
        this.BindingContext = this.Vm = this.provider.GetRequiredService<DebugPageViewModel>();
	}

    public DebugPageViewModel Vm;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        this.Vm.OnLoad().FireAndForgetSafeAsync();
    }

    private void OnCounterClicked(object sender, EventArgs e)
	{
        var platform = new DefaultMauiPlatformServices(this.Window);
        platform.OpenInModalAsync(new ModelDownloadPage(this.provider));
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result != null)
            {
                //if (result.FileName.EndsWith("bin", StringComparison.OrdinalIgnoreCase))
                //{
                //    this.Vm.ModelService.SelectedModel = new UI.Models.Gpt4AllWebModel() { FileLocation = result.FullPath, Filename = Path.GetFileName(result.FullPath) };
                //}
            }
        }
        catch (Exception ex)
        {
        }
    }
}

