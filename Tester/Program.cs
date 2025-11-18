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
            AppConfig appConfig = new("Config.json");
            appConfig.LoadConfig();
            string steamId = "76561198895471080";

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
