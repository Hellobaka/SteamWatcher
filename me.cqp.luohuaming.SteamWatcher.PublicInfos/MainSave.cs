using Another_Mirai_Native.Abstractions.Services;
using System.Collections.Generic;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos
{
    public static class MainSave
    {
        /// <summary>
        /// 保存各种事件的数组
        /// </summary>
        public static List<IOrderModel> Instances { get; set; } = new List<IOrderModel>();
        public static IPluginApi API { get; set; }
        public static ILogger Logger => API?.Logger;
        public static string AppDirectory { get; set; }
        public static string ImageDirectory { get; set; }
    }
}
