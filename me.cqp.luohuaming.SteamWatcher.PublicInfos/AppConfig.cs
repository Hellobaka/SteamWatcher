using System.Collections.Generic;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos
{
    public class AppConfig : ConfigBase
    {
        public AppConfig(string path)
            : base(path)
        {
            LoadConfig();
            Instance = this;
        }

        public static AppConfig Instance { get; private set; }

        public static string WebAPIKey { get; set; } = "";

        public static string ReplyNotPlaying { get; set; } = "";
       
        public static string ReplyPlaying { get; set; } = "";
      
        public static string ReplyPlayingChanged { get; set; } = "";
      
        public static string ReplyGetAchievement { get; set; } = "";

        public static string CustomFont { get; set; } = "";

        public static string CustomFontPath { get; set; } = "";

        public static List<string> MonitorPlayers { get; set; } = [];

        public static List<MonitorConfigItem> NoticeGroups { get; set; } = [];

        public static List<NickNameItem> NickNames { get; set; } = [];

        public static int QueryInterval { get; set; } = 60;

        public static int NoticeInterval { get; set; } = 10;

        public static bool EnableDraw { get; set; } = true;

        public static bool EnableAchievementNotice { get; set; } = true;

        public static bool EnableSessionDurationNotice { get; set; } = true;

        public static bool HideIfOfflineStatus { get; set; } = true;

        public static string AppInfoLanguage { get; set; } = "schinese";

        public static List<string> GameNameFilter { get; set; } = [];

        public override void LoadConfig()
        {
            WebAPIKey = GetConfig("WebAPIKey", "");
            ReplyNotPlaying = GetConfig("ReplyNotPlaying", "{0} 不玩 {1} 了{2}");
            ReplyPlaying = GetConfig("ReplyPlaying", "{0} 开始玩 {1} 了");
            ReplyPlayingChanged = GetConfig("ReplyPlayingChanged", "{0} 改玩 {1} 了");
            ReplyGetAchievement = GetConfig("ReplyGetAchievement", "🏆 {0} 解锁了成就 {1}");
            MonitorPlayers = GetConfig("MonitorPlayers", new List<string>());
            GameNameFilter = GetConfig("GameNameFilter", new List<string>());
            NoticeGroups = GetConfig("NoticeGroups", new List<MonitorConfigItem>());
            NickNames = GetConfig("NickNames", new List<NickNameItem>());
            QueryInterval = GetConfig("QueryInterval", 60);
            NoticeInterval = GetConfig("NoticeInterval", 10);
            EnableDraw = GetConfig("EnableDraw", true);
            HideIfOfflineStatus = GetConfig("HideIfOfflineStatus", true);
            EnableAchievementNotice = GetConfig("EnableAchievementNotice", true);
            EnableSessionDurationNotice = GetConfig("EnableSessionDurationNotice", true);
            CustomFont = GetConfig("CustomFont", "微软雅黑");
            CustomFontPath = GetConfig("CustomFontPath", "");
            AppInfoLanguage = GetConfig("AppInfoLanguage", "schinese");
        }
    }
}