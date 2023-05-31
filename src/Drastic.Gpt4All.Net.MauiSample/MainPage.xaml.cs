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
}

