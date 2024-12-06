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

        public static List<string> MonitorPlayers { get; set; } = [];

        public static List<long> NoticeGroups { get; set; } = [];

        public static int QueryInterval { get; set; } = 60;

        public override void LoadConfig()
        {
            WebAPIKey = GetConfig("WebAPIKey", "#菜单");
            MonitorPlayers = GetConfig("MonitorPlayers", new List<string>());
            NoticeGroups = GetConfig("NoticeGroups", new List<long>());
            QueryInterval = GetConfig("QueryInterval", 60);
        }
    }
}