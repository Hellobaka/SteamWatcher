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
            CanvasWidth = (int)(BaseWidth * 2.5);

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

            Games = Games.OrderByDescending(g => g.PlaytimeHours).ToList();
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
            using Painting painting = new(CanvasWidth, (int)(Games.Max(g => g.Y + g.Height)));
            painting.Clear(SKColors.Black);
            Parallel.ForEach(Games, new ParallelOptions
            {
                MaxDegreeOfParallelism = 24
            }, (game) =>
            {
                game.DownloadGamePicture();
                game.Draw();
                painting.DrawImage(game.Image, new SKRect()
                {
                    Location = new SKPoint(game.X + Gap, game.Y + Gap),
                    Size = new SKSize(game.Width - Gap * 2, game.Height - Gap * 2)
                });
                game.Image.Dispose();
            });

            string baseDirectory = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "GameGrid");
            string fileName = $"{Player.steamid}.png";
            painting.Save(Path.Combine(baseDirectory, fileName));

            return Path.Combine("SteamWatcher", "GameGrid", fileName);
        }

        private void AssignSizeTypes()
        {
            var arr = Games.Select(x => x.PlaytimeHours).ToList();
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