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

        private int Gap { get; }

        private List<GridItem> LayoutGames { get; set; } = [];

        private List<GridItem> Games { get; set; } = [];

        private GetPlayerSummary.Player Player { get; set; }

        private float MinWidth { get; set; }

        private float MinHeight { get; set; }

        public GridLayout(GetPlayerSummary.Player player, List<GridItem> games, int canvasWidth = 1600, int gap = 5)
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
            CanvasWidth = canvasWidth;
            BaseWidth = AppConfig.GameGridVerticalImage ? 600 : 460;
            BaseHeight = AppConfig.GameGridVerticalImage ? 900 : 215;
            Gap = gap;

            MinWidth = (BaseWidth / AppConfig.GameGridMaxSizeLevel) + Gap * 2;
            MinHeight = (BaseHeight / AppConfig.GameGridMaxSizeLevel) + Gap * 2;

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
                    x += MinWidth + Gap * 2;
                    if (x + w > CanvasWidth)
                    {
                        x = 0;
                        y += MinHeight + Gap * 2;
                    }
                }
            }
        }

        private bool CheckCollision(SKPoint currentPoint, GridItem game, GridItem other)
        {
            return !(currentPoint.X + game.Width < other.X ||
                     currentPoint.X > other.X + other.Width ||
                     currentPoint.Y + game.Height < other.Y ||
                     currentPoint.Y > other.Y + other.Height);
        }

        public string Draw()
        {
            using Painting painting = new(CanvasWidth, (int)(Games.Max(g => g.Y + g.Height) + Gap));
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
                    Size = new SKSize(game.Width, game.Height)
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
            return (BaseWidth * (sizeLevel / (float)AppConfig.GameGridMaxSizeLevel) - Gap * 2, BaseHeight * (sizeLevel / (float)AppConfig.GameGridMaxSizeLevel) - Gap * 2);
        }
    }
}