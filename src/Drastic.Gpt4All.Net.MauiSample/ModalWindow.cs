#if WINDOWS
using WinUIEx;
#endif

namespace Drastic.Gpt4All.Net.MauiSample
{
    public class ModalWindow : Window
    {
        public ModalWindow()
        {
        }

        public ModalWindow(Page page)
            : base(page)
        {
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
#if WINDOWS
            var platform = this.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (platform == null)
            {
                return;
            }
            var manager = WinUIEx.WindowManager.Get(platform);
            manager.Width = 500;
            manager.Height = 700;
            manager.IsResizable = false;
            manager.IsAlwaysOnTop = true;
#endif
        }
    }
}
