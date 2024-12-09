using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.Interface;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System;
using System.IO;
using System.Reflection;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp;

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
            if (string.IsNullOrEmpty(AppConfig.WebAPIKey))
            {
                e.CQLog.Warning("初始化", "WebAPIKey无效，请前往 https://steamcommunity.com/dev/apikey 申请");
                return;
            }
            Monitors monitors = new();
            monitors.PlayingChanged += Monitors_PlayingChanged;
            monitors.StartCheckTimer();
        }

        private void Monitors_PlayingChanged(List<MonitorNoticeItem> notices)
        {
            foreach (var item in AppConfig.NoticeGroups)
            {
                StringBuilder sb = new();
                foreach (var notice in notices.Where(x => item.TargetId.Any(o => o == x.SteamID)))
                {
                    sb.AppendLine(notice.ToString());
                    if (!string.IsNullOrEmpty(notice.ImagePath))
                    {
                        sb.AppendLine(CQApi.CQCode_Image(notice.ImagePath).ToSendString());
                    }
                }
                sb.RemoveNewLine();
                string info = sb.ToString();
                if (!string.IsNullOrEmpty(info))
                {
                    MainSave.CQApi.SendGroupMessage(item.GroupId, sb.ToString());

                    Thread.Sleep(TimeSpan.FromSeconds(AppConfig.NoticeInterval));
                }
            }
        }
    }
}
