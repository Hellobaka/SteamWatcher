using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.Interface;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System;
using System.IO;
using System.Reflection;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System.Threading;

namespace me.cqp.luohuaming.SteamWatcher.Code
{
    public class Event_StartUp : ICQStartup
    {
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            MainSave.AppDirectory = e.CQApi.AppDirectory;
            MainSave.CQApi = e.CQApi;
            MainSave.CQLog = e.CQLog;
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();
            foreach (var item in Assembly.GetAssembly(typeof(Event_GroupMessage)).GetTypes())
            {
                if (item.IsInterface)
                    continue;
                foreach (var instance in item.GetInterfaces())
                {
                    if (instance == typeof(IOrderModel))
                    {
                        IOrderModel obj = (IOrderModel)Activator.CreateInstance(item);
                        if (obj.ImplementFlag == false)
                            continue;
                        MainSave.Instances.Add(obj);
                    }
                }
            }

            e.CQLog.Info("初始化", "加载配置");
            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();

            Monitors monitors = new();
            monitors.PlayingChanged += Monitors_PlayingChanged;
            monitors.StartCheckTimer();
        }

        private void Monitors_PlayingChanged(string msg, string pic)
        {
            foreach (var group in AppConfig.NoticeGroups)
            {
                MainSave.CQApi.SendGroupMessage(group, msg + pic);
                Thread.Sleep(10000);
            }
        }
    }
}
