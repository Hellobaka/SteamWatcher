using Another_Mirai_Native.Abstractions.Services;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos
{
    public static class MainSave
    {
        public static IPluginApi API { get; set; }
        public static ILogger Logger => API?.Logger;
        public static string AppDirectory { get; set; }
        public static string ImageDirectory { get; set; }
    }
}
