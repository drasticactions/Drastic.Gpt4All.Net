using Drastic.Gpt4All.Net.UI.ViewModels;
using Drastic.Tools;
#if IOS || MACCATALYST
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
#endif

namespace Drastic.Gpt4All.Net.MauiSample;

public partial class ModelDownloadPage : ContentPage
{
	public ModelDownloadPage(IServiceProvider provider)
	{
        InitializeComponent();
        this.BindingContext = this.Vm = provider.GetRequiredService<Gpt4AllModelDownloadViewModel>();
#if IOS || MACCATALYST
        On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
#endif
    }

    public Gpt4AllModelDownloadViewModel Vm;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        this.Vm.OnLoad().FireAndForgetSafeAsync();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
#if WINDOWS
        Application.Current?.CloseWindow(this.Window);
#else
        this.Navigation.PopModalAsync();
#endif
    }
}