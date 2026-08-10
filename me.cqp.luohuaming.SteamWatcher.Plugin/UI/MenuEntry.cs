using Another_Mirai_Native.Abstractions.Attributes;
using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Handlers;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace me.cqp.luohuaming.SteamWatcher.UI
{
    [Menu("控制台")]
    public class MenuEntry : IMenuHandler
    {
        private MainWindow window;
        private Thread uiThread;

        public void OnMenu(MenuContext e)
        {
            try
            {
                if (window == null)
                {
                    using var ready = new ManualResetEventSlim(false);
                    uiThread = new Thread(() =>
                    {
                        // 创建 Application 实例，保证 WPF 资源可用
                        _ = new Application();
                        window = new MainWindow();
                        window.Closing += (s, ev) =>
                        {
                            ev.Cancel = true;
                            window.Hide();
                        };
                        ready.Set();
                        Dispatcher.Run();
                    })
                    {
                        IsBackground = true,
                    };
                    uiThread.SetApartmentState(ApartmentState.STA);
                    uiThread.Start();
                    ready.Wait();
                }

                window.Dispatcher.Invoke(() => window.Show());
            }
            catch (Exception exc)
            {
                e.API.Logger.Info("Error", exc.Message + exc.StackTrace);
            }
        }
    }
}
