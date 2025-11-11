using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using SkiaSharp;
using System;
using System.IO;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage
{
    public class GridItem
    {
        public int AppId { get; set; }
      
        public string Name { get; set; } = "";
      
        public double PlaytimeHours { get; set; }
      
        public string ImageUrl { get; set; } = "";
      
        public bool AllAchievements { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public string SizeType { get; set; } = "Small";

        public float Height { get; set; }

        public SKImage? Image { get; set; }

        public void Draw()
        {
            string path = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "GameGrid", "Cached");
            bool canOverwrite = false;
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                canOverwrite = (DateTime.Now - fileInfo.CreationTime).TotalDays > 7;
            }
            var t = CommonHelper.DownloadFile(GetOwnedGames.ConvertGamePicture(AppId), $"{AppId}.jpg", path, canOverwrite);
            if (!t)
            {
                MainSave.CQLog?.Warning("下载游戏封面", $"{Name} 游戏封面下载失败");
                // 加载默认失败图片
                return;
            }
            var painting = new Painting(600, 900);
            painting.DrawImage(Path.Combine(path, $"{AppId}.jpg"), Painting.Anywhere);
            if (AllAchievements)
            {
                // draw trophy
                // draw gradient rainbow in border
            }
            Image = painting.SnapShot();
            painting.Dispose();
        }
    }
}
