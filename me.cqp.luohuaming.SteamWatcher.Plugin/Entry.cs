using Another_Mirai_Native.Abstractions;
using Another_Mirai_Native.Abstractions.Attributes;
using Another_Mirai_Native.Abstractions.Models;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin
{
    [PluginInfo(
        appId: "me.cqp.luohuaming.SteamWatcher",
        name: "Steam视奸机",
        version: "2.0.0",
        description: "玩啥呢 不叫我",
        author: "落花茗")]
    public class Entry : PluginBase
    {
        private Monitors monitors;

        public override async Task OnEnableAsync(CancellationToken ct)
        {
            MainSave.API = API;
            MainSave.AppDirectory = API.AppApi.GetAppDirectory();
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();

            EnsureAssets();

            API.Logger.Info("初始化", "加载配置");
            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();
            if (string.IsNullOrEmpty(AppConfig.WebAPIKey))
            {
                API.Logger.Warn("初始化", "WebAPIKey无效，请前往 https://steamcommunity.com/dev/apikey 申请");
                return;
            }

            monitors = new Monitors();
            monitors.PlayingChanged += Monitors_PlayingChanged;
            monitors.StartCheckTimer();
        }

        public override async Task OnDisableAsync(CancellationToken ct)
        {
            monitors?.StopCheckTimer();
            monitors = null;
        }

        private void EnsureAssets()
        {
            try
            {
                string assetDir = Path.Combine(MainSave.AppDirectory, "Assets");
                string framePath = Path.Combine(assetDir, "Frame.png");
                if (!File.Exists(framePath))
                {
                    var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                        .FirstOrDefault(x => x.EndsWith("Assets.Frame.png"));
                    if (resourceName == null)
                    {
                        API.Logger.Warn("初始化", "未找到内嵌资源 Assets/Frame.png，请手动放置到插件数据目录");
                        return;
                    }
                    Directory.CreateDirectory(assetDir);
                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                    using var file = File.Create(framePath);
                    stream.CopyTo(file);
                    API.Logger.Info("初始化", "已自动生成 Assets/Frame.png");
                }
            }
            catch (Exception ex)
            {
                API.Logger.Warn("初始化", $"生成 Assets 失败: {ex.Message}");
            }
        }

        private async void Monitors_PlayingChanged(List<MonitorNoticeItem> notices)
        {
            foreach (var item in AppConfig.NoticeGroups)
            {
                StringBuilder sb = new();
                foreach (var notice in notices.Where(x => item.TargetId.Any(o => o == x.SteamID)))
                {
                    // 处理昵称
                    notice.NickName = "";
                    var nickName = AppConfig.NickNames.FirstOrDefault(x => x.SteamID == notice.SteamID);
                    var groupNick = nickName?.Groups.FirstOrDefault(x => x.GroupID == item.GroupId);
                    if (groupNick != null)
                    {
                        notice.NickName = groupNick.NickName;
                    }

                    sb.AppendLine(notice.ToString());
                    // 绘制，筛选开始玩与获得成就
                    if (AppConfig.EnableDraw && (notice.NoticeType == NoticeType.Playing || notice.NoticeType == NoticeType.GetAchievement))
                    {
                        if (notice.DownloadAvatar())
                        {
                            string filePath = notice.Draw();
                            if (!string.IsNullOrEmpty(filePath) && File.Exists(Path.Combine(MainSave.ImageDirectory, filePath)))
                            {
                                sb.AppendLine(new MessageBuilder().Image(filePath).Build());
                            }
                        }
                        else
                        {
                            API.Logger.Warn("下载头像", $"下载 {notice.PlayerName}[{notice.SteamID}] 用户头像时失败");
                        }
                    }
                }
                sb.RemoveNewLine();
                string info = sb.ToString();
                if (!string.IsNullOrEmpty(info))
                {
                    await API.MessageApi.SendGroupMessageAsync(item.GroupId, info);
                    await Task.Delay(TimeSpan.FromSeconds(AppConfig.NoticeInterval));
                }
            }
        }
    }
}
