using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetAppInfo
    {
        public const string BaseUrl = "https://store.steampowered.com/api/appdetails?appids={0}&l={1}&filters=basic";

        private static Dictionary<string, AppInfo> Caches { get; set; } = [];

        public static async Task<AppInfo?> Get(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return null;
            }
            if (Caches.TryGetValue(appId, out AppInfo appInfo))
            {
                return appInfo;
            }
            string url = string.Format(BaseUrl, appId, AppConfig.AppInfoLanguage);
            using HttpClient client = new();
            var result = await client.GetAsync(url);
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            var o = JObject.Parse(json);

            appInfo = o[appId].ToObject<AppInfo>();
            if (appInfo != null && !Caches.ContainsKey(appId))
            {
                Caches.Add(appId, appInfo);
            }
            return appInfo;
        }

        public class AppInfo
        {
            public bool success { get; set; }
            
            public Data data { get; set; }
        }

        public class Data
        {
            public string name { get; set; }
           
            public int steam_appid { get; set; }
        }
    }
}
