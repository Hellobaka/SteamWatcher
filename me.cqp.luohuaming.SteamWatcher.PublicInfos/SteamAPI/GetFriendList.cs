using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class GetFriendList
    {
        public const string BaseUrl = "http://api.steampowered.com/ISteamUser/GetFriendList/v1/?key={0}&steamid={1}";

        public static async Task<Friendslist> Get(string steamId)
        {
            string url = string.Format(BaseUrl, AppConfig.WebAPIKey, steamId);
            using HttpClient client = new();
            var result = await client.GetAsync(url);
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Friendslist>(json);
        }

        public Friendslist friendslist { get; set; }

        public class Friendslist
        {
            public Friend[] friends { get; set; }
        }

        public class Friend
        {
            public string steamid { get; set; }
           
            public string relationship { get; set; }
          
            public int friend_since { get; set; }
        }
    }
}
