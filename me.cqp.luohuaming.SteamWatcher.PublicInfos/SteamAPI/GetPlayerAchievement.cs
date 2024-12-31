using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetPlayerAchievement
    {
        public const string BaseUrl = "http://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v0001/?steamid={0}&appid={1}&l={2}&key={3}";

        public static async Task<GetPlayerAchievement> Get(string steamId, string appId)
        {
            try
            {
                string url = string.Format(BaseUrl, steamId, appId, AppConfig.AppInfoLanguage, AppConfig.WebAPIKey);
                using HttpClient client = new();
                var result = await client.GetAsync(url);
                var json = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<GetPlayerAchievement>(json);
                if (response == null || response.playerstats == null || !response.playerstats.success)
                {
                    return null;
                }
                return response;
            }
            catch (Exception ex)
            {
                MainSave.CQLog.Error("GetPlayerAchievement", ex.Message + ex.StackTrace);
                return null;
            }
        }

        public Playerstats playerstats { get; set; }

        public class Playerstats
        {
            public string steamID { get; set; }

            public string gameName { get; set; }

            public Achievement[] achievements { get; set; }

            public bool success { get; set; }
        }

        public class Achievement
        {
            public string apiname { get; set; }

            public int achieved { get; set; }

            public int unlocktime { get; set; }

            public string name { get; set; }

            public string description { get; set; }
        }
    }
}