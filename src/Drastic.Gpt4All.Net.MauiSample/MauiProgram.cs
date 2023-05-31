using Drastic.Gpt4All.Net.MauiSample.Services;
using Drastic.Gpt4All.Net.UI.Services;
using Drastic.Gpt4All.Net.UI.ViewModels;
using Drastic.Services;
using Microsoft.Extensions.Logging;
using Xe.AcrylicView;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using WinUIEx;
#endif
namespace Drastic.Gpt4All.Net.MauiSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if MACCATALYST
        DrasticForbiddenControls.CatalystControls.AllowsUnsupportedMacIdiomBehavior();
        Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("ButtonChange", (handler, view) =>
        {
            handler.PlatformView.PreferredBehavioralStyle = UIKit.UIBehavioralStyle.Pad;
            handler.PlatformView.Layer.CornerRadius = 5;
            handler.PlatformView.ClipsToBounds = true;
        });
#elif IOS
        Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("ButtonChange", (handler, view) =>
        {
            handler.PlatformView.Layer.CornerRadius = 5;
            handler.PlatformView.ClipsToBounds = true;
        });
#endif
        var builder = MauiApp.CreateBuilder();

        builder.Services
          .AddSingleton<IAppDispatcher, MauiAppDispatcherService>()
          .AddSingleton<IErrorHandlerService, DebugErrorHandler>()
          .AddSingleton<Gpt4AllModelService>()
          .AddSingleton<Gpt4AllModelDownloadViewModel>()
          .AddSingleton<DebugPageViewModel>();

        builder
            .UseMauiApp<App>()
            .UseAcrylicView()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
#if WINDOWS

            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(wndLifeCycleBuilder =>
                {
                    wndLifeCycleBuilder.OnWindowCreated(window =>
                    {
                        var manager = WinUIEx.WindowManager.Get(window);
                        manager.Backdrop = new WinUIEx.MicaSystemBackdrop();
                        manager.MinWidth = 640;
                        manager.MinHeight = 480;
                    });
                });
            });
#endif
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
