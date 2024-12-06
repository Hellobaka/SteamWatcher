using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetPlayerSummary
    {
        public const string BaseUrl = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={0}&steamid={1}";

        public static async Task<PlayerSummary> Get(List<string> steamId)
        {
            string url = string.Format(BaseUrl, AppConfig.WebAPIKey, string.Join(",", steamId));
            using HttpClient client = new();
            var result = await client.GetAsync(url);
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PlayerSummary>(json);
        }

        public PlayerSummary response { get; set; }

        public class PlayerSummary
        {
            public Player[] players { get; set; }
        }

        public class Player
        {
            public string steamid { get; set; }
            public int communityvisibilitystate { get; set; }
            public int profilestate { get; set; }
            public string personaname { get; set; }
            public int commentpermission { get; set; }
            public string profileurl { get; set; }
            public string avatar { get; set; }
            public string avatarmedium { get; set; }
            public string avatarfull { get; set; }
            public string avatarhash { get; set; }
            public int lastlogoff { get; set; }
            public int personastate { get; set; }
            public string primaryclanid { get; set; }
            public int timecreated { get; set; }
            public int personastateflags { get; set; }
            public string loccountrycode { get; set; }
        }

    }
}
