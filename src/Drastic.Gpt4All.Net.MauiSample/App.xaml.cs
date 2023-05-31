namespace Drastic.Gpt4All.Net.MauiSample;

public partial class App : Application
{
	public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();

		MainPage = new MainPage(serviceProvider);
	}
}
