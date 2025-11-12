using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage
{
    public class GridLayout
    {
        public int CanvasWidth { get; }

        public int BaseWidth { get; }

        public int BaseHeight { get; }

        public int Gap { get; }

        public List<GridItem> Games { get; set; } = [];

        public GetPlayerSummary.Player Player { get; set; }

        public GridLayout(GetPlayerSummary.Player player, List<GridItem> games, int canvasWidth = 1600, int baseWidth = 600, int baseHeight = 900, int gap = 6)
        {
            if (Player == null)
            {
                throw new ArgumentNullException("玩家信息不可为空");
            }
            if (games.Count == 0)
            {
                throw new ArgumentNullException("游戏列表不可为空");
            }
            Player = player;
            Games = [];
            CanvasWidth = canvasWidth;
            BaseWidth = baseWidth;
            BaseHeight = baseHeight;
            Gap = gap;
        }

        public void CalculateLayout()
        {
            if (Games == null || Games.Count == 0)
            {
                return;
            }

            Games = Games.OrderByDescending(g => g.PlaytimeHours).ToList();
            AssignSizeTypes();

            // 贪心布局
            float x = Gap, y = Gap;
            float currentRowHeight = 0;

            foreach (var game in Games)
            {
                var (w, h) = GetImageSize(game.SizeLevel);

                // 换行检查
                if (x + w + Gap > CanvasWidth)
                {
                    // 换行
                    x = Gap;
                    y += currentRowHeight + Gap;
                    currentRowHeight = 0;
                }
                game.X = x;
                game.Y = y;
                game.Width = w;
                game.Height = h;
                x += w + Gap;
                currentRowHeight = Math.Max(currentRowHeight, h);
            }
        }

        public string Draw()
        {
            using Painting painting = new(CanvasWidth, (int)(Games.Max(g => g.Y + g.Height) + Gap));
            // 于此处实现详细绘制，注意控制内存
            string baseDirectory = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "GameGrid");
            string fileName = $"{Player.steamid}.png";
            painting.Save(Path.Combine(baseDirectory, fileName));

            return Path.Combine("SteamWatcher", "GameGrid", fileName);
        }

        private void AssignSizeTypes()
        {
            int n = Games.Count;
            for (int i = 0; i < n; i++)
            {
                double rank = (double)i / n;
                Games[i].SizeLevel = (int)Math.Ceiling(rank * 10);
            }
        }

        private (float w, float h) GetImageSize(int sizeLevel)
        {
            return (BaseWidth * (1.0f / sizeLevel), BaseHeight * (1.0f / sizeLevel));
        }
    }
}