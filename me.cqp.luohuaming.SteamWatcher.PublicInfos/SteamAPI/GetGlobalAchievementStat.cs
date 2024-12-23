using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetGlobalAchievementStat
    {
        public const string BaseUrl = "https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2?gameid={0}";

        public static async Task<float> Get(string appId, string achievementId)
        {
            try
            {
                string url = string.Format(BaseUrl, appId);
                using HttpClient client = new();
                var result = await client.GetAsync(url);
                result.EnsureSuccessStatusCode();
                var json = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<GetGlobalAchievementStat>(json);
                return response.achievementpercentages.achievements.FirstOrDefault(x => x.name == achievementId).percent;
            }
            catch (Exception ex)
            {
                MainSave.CQLog.Error("GetGlobalAchievementStat", ex.Message + ex.StackTrace);
                return -1;
            }
        }

        public Achievementpercentages achievementpercentages { get; set; }

        public class Achievementpercentages
        {
            public Achievement[] achievements { get; set; }
        }

        public class Achievement
        {
            public string name { get; set; }

            public float percent { get; set; }
        }
    }
}