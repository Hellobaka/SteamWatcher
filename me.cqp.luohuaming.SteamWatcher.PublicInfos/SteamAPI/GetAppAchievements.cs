using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetAppAchievements
    {
        public const string BaseUrl = "https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={0}&appid={1}&l={2}";

        private static Dictionary<string, GetAppAchievements> Caches { get; set; } = [];

        public static async Task<Achievement?> Get(string appId, string achievementId)
        {
            try
            {
                if (Caches.TryGetValue(appId, out var cache))
                {
                    if (cache == null
                        || cache.game == null
                        || cache.game.availableGameStats == null
                        || cache.game.availableGameStats.achievements == null)
                    {
                        Caches.Remove(appId);
                    }
                    else
                    {
                        return cache.game.availableGameStats.achievements.FirstOrDefault(x=>x.name == achievementId);
                    }
                }
                string url = string.Format(BaseUrl, AppConfig.WebAPIKey, appId, AppConfig.AppInfoLanguage);
                using HttpClient client = new();
                var result = await client.GetAsync(url);
                result.EnsureSuccessStatusCode();
                var json = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<GetAppAchievements>(json);
                Caches[appId] = response;
                return response.game.availableGameStats.achievements.FirstOrDefault(x => x.name == achievementId);
            }
            catch (Exception ex)
            {
                MainSave.CQLog?.Error("GetAppAchievements", ex.Message + ex.StackTrace);
                return null;
            }
        }

        public Game game { get; set; }

        public class Game
        {
            public string gameName { get; set; }

            public string gameVersion { get; set; }

            public Availablegamestats availableGameStats { get; set; }
        }

        public class Availablegamestats
        {
            public Achievement[] achievements { get; set; }
        }

        public class Achievement
        {
            public string name { get; set; }

            public int defaultvalue { get; set; }

            public string displayName { get; set; }

            public int hidden { get; set; }

            public string? description { get; set; }

            public string icon { get; set; }

            public string icongray { get; set; }
        }
    }
}