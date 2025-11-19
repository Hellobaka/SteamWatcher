using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage
{
    public class GridItem
    {
        public int AppId { get; set; }

        public string Name { get; set; } = "";

        public double PlaytimeMinutes { get; set; }

        public string ImageUrl { get; set; } = "";

        public string ImageUrlBackup { get; set; } = "";

        public bool AllAchievements { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public SKImage Image { get; set; }

        public int SizeLevel { get; set; } = 1;

        public string GetGamePicturePath()
        {
            string path = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "GameGrid", "Cached");
            string fileName = AppConfig.GameGridVerticalImage ? $"{AppId}_Vertical.jpg" : $"{AppId}_Horizonal.jpg";

            return Path.Combine(path, fileName);
        }

        public void DownloadGamePicture()
        {
            bool canOverwrite = true;
            string path = GetGamePicturePath();
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                canOverwrite = fileInfo.Length < 1024 ||(DateTime.Now - fileInfo.CreationTime).TotalDays > 7;
            }
            var t = CommonHelper.DownloadFile(ImageUrl, path, canOverwrite);
            if (!t)
            {
                t = CommonHelper.DownloadFile(ImageUrlBackup, path, canOverwrite);
            }
            if (!t)
            {
                MainSave.CQLog?.Warning("下载游戏封面", $"{Name} 游戏封面下载失败");
                return;
            }
        }

        public void Draw()
        {
            using var painting = AppConfig.GameGridVerticalImage ? new Painting(600, 900) : new Painting(460, 215);
            painting.Clear(SKColors.Transparent);
            string path = GetGamePicturePath();
            if (!File.Exists(path))
            {
                DrawDefault(painting);
            }
            else
            {
                try
                {
                    painting.RadiusBorder(30);
                    painting.DrawImage(path, new SKRect
                    {
                        Location = new(),
                        Size = new SKSize(painting.Width, painting.Height)
                    });
                }
                catch
                {
                    DrawDefault(painting);
                }
            }
            if (AllAchievements)
            {
                if (AppConfig.GameGridVerticalImage)
                {
                    painting.DrawText("🏆", Painting.Anywhere, new SKPoint(painting.Width - 140, 30), SKColor.Parse("#FFFFFF"), 96);
                    painting.DrawRainbowGradientBorder(15, 30);
                }
                else
                {
                    painting.DrawText("🏆", Painting.Anywhere, new SKPoint(painting.Width - 100, 10), SKColor.Parse("#FFFFFF"), 64);
                    painting.DrawRainbowGradientBorder(15, 30);
                }
            }
            Image = painting.SnapShot();
        }

        private void DrawDefault(Painting painting)
        {
            painting.Clear(SKColor.Parse("#141A21"));
            painting.DrawText(Name, new SKRect
            {
                Location = new(),
                Size = new(painting.Width, painting.Height)
            }, new SKPoint(0, painting.Height / 4), SKColors.White, 96, wrap: true, align: Painting.TextAlign.Center);
        }

        public static List<GridItem> Parse(GetOwnedGames.Game[] games, GetTopAchievementsForGames.Game[] achievements)
        {
            return games.Where(x => x.playtime_forever >= AppConfig.GameGridFilterGameTime).Select(game => new GridItem
            {
                AppId = game.appid,
                Name = game.name,
                PlaytimeMinutes = game.playtime_forever,
                ImageUrl = GetOwnedGames.GetGamePictureUrl(game.appid, game.capsule_filename, AppConfig.GameGridVerticalImage),
                ImageUrlBackup = GetOwnedGames.GetGamePictureUrl_CDN2(game.appid, AppConfig.GameGridVerticalImage),
                AllAchievements = game.has_community_visible_stats
                        && achievements.FirstOrDefault(a => a.appid == game.appid) is GetTopAchievementsForGames.Game ga
                        && ga.total_achievements > 0
                        && ga.achievements != null
                        && ga.total_achievements == ga.achievements.Length
            }).ToList();
        }
    }
}
