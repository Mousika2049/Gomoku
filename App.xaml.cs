#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace GomokuAI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new StartPage());
        }

        // 重写 CreateWindow 来设置 Windows 平台的窗口属性
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            // 仅针对 Windows 调整窗口大小和标题
#if WINDOWS
            window.Created += (s, e) =>
            {
                var handle = WinRT.Interop.WindowNative.GetWindowHandle(window.Handler.PlatformView);
                var id = Win32Interop.GetWindowIdFromWindow(handle);
                var appWindow = AppWindow.GetFromWindowId(id);

                if (appWindow != null)
                {
                    // 设置为 800x900 的窗口，适合五子棋竖屏或方形布局
                    appWindow.Resize(new SizeInt32(1000, 1600));
                    appWindow.Title = "五子棋 AI 大战";
                }
            };
#endif

            return window;
        }
    }
}