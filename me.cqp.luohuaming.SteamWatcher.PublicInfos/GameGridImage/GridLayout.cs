using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage
{
    public class GridLayout
    {
        private int CanvasWidth { get; }

        private float BaseWidth { get; }

        private float BaseHeight { get; }

        private List<GridItem> LayoutGames { get; set; } = [];

        private List<GridItem> Games { get; set; } = [];

        private GetPlayerSummary.Player Player { get; set; }

        private float MinWidth { get; set; }

        private float MinHeight { get; set; }

        private int Gap { get; set; } = 5;

        public GridLayout(GetPlayerSummary.Player player, List<GridItem> games)
        {
            if (player == null)
            {
                throw new ArgumentNullException("玩家信息不可为空");
            }
            if (games.Count == 0)
            {
                throw new ArgumentNullException("游戏列表不可为空");
            }
            Player = player;
            Games = games;
            BaseWidth = AppConfig.GameGridVerticalImage ? 600 : 460;
            BaseHeight = AppConfig.GameGridVerticalImage ? 900 : 215;
            CanvasWidth = (int)(BaseWidth * 3.5);

            MinWidth = (BaseWidth / AppConfig.GameGridMaxSizeLevel);
            MinHeight = (BaseHeight / AppConfig.GameGridMaxSizeLevel);

            CalculateLayout();
        }

        private void CalculateLayout()
        {
            if (Games == null || Games.Count == 0)
            {
                return;
            }

            Games = Games.OrderByDescending(g => g.PlaytimeMinutes).ToList();
            AssignSizeTypes();

            foreach (var game in Games)
            {
                FindImagePosition(game);
            }
        }

        private void FindImagePosition(GridItem game)
        {
            var (w, h) = GetImageSize(game.SizeLevel);
            game.Width = w; game.Height = h;

            bool found = false;
            float x = 0, y = 0;
            while (!found)
            {
                SKPoint currentPoint = new(x, y);

                bool collision = false;
                foreach (var other in LayoutGames)
                {
                    if (CheckCollision(currentPoint, game, other))
                    {
                        collision = true;
                        break;
                    }
                }

                if (!collision)
                {
                    game.X = currentPoint.X;
                    game.Y = currentPoint.Y;
                    LayoutGames.Add(game);
                    found = true;
                }
                else
                {
                    x += MinWidth;
                    if (x + w > CanvasWidth)
                    {
                        x = 0;
                        y += MinHeight;
                    }
                }
            }
        }

        private bool CheckCollision(SKPoint currentPoint, GridItem game, GridItem other)
        {
            if (currentPoint.X + game.Width <= other.X) return false; // 左
            if (currentPoint.X >= other.X + other.Width) return false; // 右
            if (currentPoint.Y + game.Height <= other.Y) return false; // 上
            if (currentPoint.Y >= other.Y + other.Height) return false; // 下
            return true;
        }

        public string Draw()
        {
            using Painting gameGrid = new(CanvasWidth, (int)(Games.Max(g => g.Y + g.Height)));
            using Painting painting = new(CanvasWidth, (int)(gameGrid.Height + 240));// 160 + 80
            gameGrid.Clear(SKColors.Black);
            painting.Clear(SKColors.Black);
            string avatarPath = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Avatars", $"{Player.steamid}.png");
            if (CommonHelper.DownloadFile(Player.avatarfull, avatarPath))
            {
                // 头像 (20, 20) 130x130
                painting.DrawRoundedImage(avatarPath, new SKRect(20, 20, 150, 150), 10);
                // 昵称 (180, 55)
                painting.DrawText($"{Player.personaname} [{long.Parse(Player.steamid) - 76561197960265728}]", new SKRect(180, 20, painting.Width - 20, 150), new SKPoint(120, 55), SKColors.White, 48, isBold: true);
                // 游戏总数和总时长 (一半宽度起, 55)
                string gameStat = $"拥有游戏数量: {Games.Count} 总游戏时长: {Games.Sum(g => g.PlaytimeMinutes) / 60.0:f1} 小时";
                painting.DrawText(gameStat, new SKRect(painting.Width / 2, 20, painting.Width - 20, 150), new SKPoint(0, 55), SKColors.White, 48, isBold: true, align: Painting.TextAlign.Right);
            }
            else
            {
                MainSave.CQLog?.Warning("头像下载", "头像下载失败，跳过绘制");
            }
            Parallel.ForEach(Games, new ParallelOptions
            {
                MaxDegreeOfParallelism = 24
            }, (game) =>
            {
                game.DownloadGamePicture();
                game.Draw();
                gameGrid.DrawImage(game.Image, new SKRect()
                {
                    Location = new SKPoint(game.X + Gap, game.Y + Gap),
                    Size = new SKSize(game.Width - Gap * 2, game.Height - Gap * 2)
                });
                game.Image.Dispose();
            });
            var gameGridImage = gameGrid.SnapShot();
            gameGrid.Dispose();
            painting.DrawImage(gameGridImage, new SKRect(0, 160, gameGrid.Width, gameGrid.Height + 160));
            painting.DrawText("Powered By @噗噗个噗Bot", new SKRect
            {
                Location = new(0, painting.Height - 80),
                Size = new(painting.Width, 80)
            }, new SKPoint(0, painting.Height - 70), SKColors.White, 48, isBold: false, align: Painting.TextAlign.Center);
           
            string baseDirectory = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "GameGrid");
            string fileName = $"{Player.steamid}.png";
            string path = Path.Combine(baseDirectory, fileName);
            painting.Save(path);

            return Path.Combine("SteamWatcher", "GameGrid", fileName);
        }

        private void AssignSizeTypes()
        {
            var arr = Games.Select(x => x.PlaytimeMinutes).ToList();
            var levels = SizeLevelGenerators.ComputeLogQuantileLevels(arr, AppConfig.GameGridMaxSizeLevel);
            for (int i = 0; i < arr.Count; i++)
            {
                Games[i].SizeLevel = levels[i];
            }
        }

        private (float w, float h) GetImageSize(int sizeLevel)
        {
            return (BaseWidth * (sizeLevel / (float)AppConfig.GameGridMaxSizeLevel), BaseHeight * (sizeLevel / (float)AppConfig.GameGridMaxSizeLevel));
        }
    }
}