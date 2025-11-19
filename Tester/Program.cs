using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainSave.ImageDirectory = "";
            MainSave.AppDirectory = "";
            AppConfig appConfig = new("Config.json");
            appConfig.LoadConfig();
            //TestGameGrid();
            TestPlayerSummary();
        }

        private static void TestPlayerSummary()
        {
            string steamId = "76561199028130480";
            string appId = "2358720";
            GetPlayerSummary.PlayerSummary summary;
            GetAppInfo.AppInfo appInfo;
            if (File.Exists("summary.json"))
            {
                summary = JsonConvert.DeserializeObject<GetPlayerSummary.PlayerSummary>(File.ReadAllText("summary.json"));
                appInfo = JsonConvert.DeserializeObject<GetAppInfo.AppInfo>(File.ReadAllText("appInfo.json"));
            }
            else
            {
                summary = GetPlayerSummary.Get([steamId]).Result;
                appInfo = GetAppInfo.Get(appId).Result;
                File.WriteAllText("summary.json", JsonConvert.SerializeObject(summary, Formatting.Indented));
                File.WriteAllText("appInfo.json", JsonConvert.SerializeObject(appInfo, Formatting.Indented));
            }
            var player = summary.players[0];
            MonitorNoticeItem item = new MonitorNoticeItem()
            {
                GameName = appInfo.data.name,
                PlayerName = player.personaname,
                SteamID = player.steamid,
                AvatarUrl = player.avatarfull,
                AppID = appId,
                NoticeType = NoticeType.Playing
            };
            item.DownloadAvatar();
            string path = item.Draw();
            Console.WriteLine(path);
        }

        private static void TestGameGrid()
        {
            string steamId = "76561199028130480";

            GetPlayerSummary.PlayerSummary summary;
            GetOwnedGames.Response games;
            GetTopAchievementsForGames.Game[] achievements;
            if (File.Exists("summary.json"))
            {
                summary = JsonConvert.DeserializeObject<GetPlayerSummary.PlayerSummary>(File.ReadAllText("summary.json"));
                games = JsonConvert.DeserializeObject<GetOwnedGames.Response>(File.ReadAllText("games.json"));
                achievements = JsonConvert.DeserializeObject<GetTopAchievementsForGames.Game[]>(File.ReadAllText("achievements.json"));
            }
            else
            {
                summary = GetPlayerSummary.Get([steamId]).Result;
                games = GetOwnedGames.Get(steamId).Result.Result;
                achievements = GetTopAchievementsForGames.Get(steamId, games.games.Select(x => x.appid).ToArray()).Result;
                File.WriteAllText("summary.json", JsonConvert.SerializeObject(summary, Formatting.Indented));
                File.WriteAllText("games.json", JsonConvert.SerializeObject(games, Formatting.Indented));
                File.WriteAllText("achievements.json", JsonConvert.SerializeObject(achievements, Formatting.Indented));
            }
            GridLayout layout = new(summary.players[0], GridItem.Parse(games.games, achievements));
            string path = layout.Draw();
            Console.WriteLine(path);
        }
    }
}
