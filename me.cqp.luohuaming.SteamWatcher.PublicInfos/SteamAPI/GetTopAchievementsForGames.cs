using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetTopAchievementsForGames
    {
        public const string BaseUrl = "https://api.steampowered.com/IPlayerService/GetTopAchievementsForGames/v1/?key={0}&steamid={1}&language={2}&max_achievements={3}&{4}";

        public static async Task<Game[]> Get(string steamId, int[] appIds, int maxAchievementCount = 500)
        {
            try
            {
                List<Game> allGames = [];
                int batchSize = 50;
                for (int i = 0; i < Math.Ceiling(appIds.Length / batchSize * 1.0); i++)
                {
                    int index = 0;
                    string appIdsParam = string.Join("&", appIds.Skip(i * batchSize).Take(batchSize).Select(id => $"appids[{index++}]={id}"));
                    string url = string.Format(BaseUrl, AppConfig.WebAPIKey, steamId, AppConfig.AppInfoLanguage, maxAchievementCount, appIdsParam);
                    using HttpClient client = new();
                    var result = await client.GetAsync(url);
                    var json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GetTopAchievementsForGames>(json);
                    if (response != null && response.Result != null && response.Result.games != null)
                    {
                        allGames.AddRange(response.Result.games);
                    }
                }

                return allGames.ToArray();
            }
            catch (Exception ex)
            {
                MainSave.CQLog.Error("GetTopAchievementsForGames", ex.Message + ex.StackTrace);
                return null;
            }
        }

        [JsonProperty("response")]
        public Response Result { get; set; }

        public class Response
        {
            public Game[] games { get; set; }
        }

        public class Game
        {
            public int appid { get; set; }

            public int total_achievements { get; set; }

            public Achievement[] achievements { get; set; }
        }

        public class Achievement
        {
            public int statid { get; set; }

            public int bit { get; set; }

            public string name { get; set; }

            public string desc { get; set; }

            public string icon { get; set; }

            public string icon_gray { get; set; }

            public bool hidden { get; set; }

            public string player_percent_unlocked { get; set; }
        }
    }
}