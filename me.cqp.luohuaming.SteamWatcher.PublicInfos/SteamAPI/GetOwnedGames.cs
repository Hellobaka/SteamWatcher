using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetOwnedGames
    {
        public const string BaseUrl = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={0}&steamid={1}&include_appinfo=true&skip_unvetted_apps=false&language={2}&include_extended_appinfo=true&include_played_free_games=true";

        public static async Task<GetOwnedGames> Get(string steamId)
        {
            try
            {
                string url = string.Format(BaseUrl, AppConfig.WebAPIKey, steamId, AppConfig.AppInfoLanguage);
                using HttpClient client = new();
                var result = await client.GetAsync(url);
                var json = await result.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetOwnedGames>(json);
            }
            catch (Exception ex)
            {
                MainSave.CQLog?.Error("GetOwnedGames", ex.Message + ex.StackTrace);
                return null;
            }
        }

        public static string GetGamePictureUrl(int appId, bool isVertical) => $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/{(isVertical ? "library_600x900" : "header")}.jpg";

        [JsonProperty("response")]
        public Response Result { get; set; }

        public class Response
        {
            public int game_count { get; set; }

            public Game[] games { get; set; }
        }

        public class Game
        {
            public int appid { get; set; }

            public string name { get; set; }

            public int playtime_forever { get; set; }

            public string img_icon_url { get; set; }

            public string capsule_filename { get; set; }

            public bool has_workshop { get; set; }

            public bool has_market { get; set; }

            public bool has_dlc { get; set; }

            public int[] content_descriptorids { get; set; }

            public string sort_as { get; set; }

            public bool has_community_visible_stats { get; set; }

            public bool has_leaderboards { get; set; }

            public int playtime_2weeks { get; set; }
        }
    }
}