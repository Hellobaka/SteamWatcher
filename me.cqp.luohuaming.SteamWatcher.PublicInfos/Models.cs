using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.EventArgs;
using SkiaSharp;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos
{
    public interface IOrderModel
    {
        bool ImplementFlag { get; set; }
        /// <summary>
        /// 优先级，越高越优先处理
        /// </summary>
        int Priority { get; set; }
        string GetCommand();
        bool CanExecute(string destStr);
        FunctionResult Execute(CQGroupMessageEventArgs e);
        FunctionResult Execute(CQPrivateMessageEventArgs e);
    }

    public class MonitorConfigItem
    {
        public long GroupId { get; set; }

        public List<string> TargetId { get; set; }
    }

    public enum NoticeType
    {
        Playing,

        PlayChanged,

        NotPlayed,

        GetAchievement
    }

    public class MonitorNoticeItem
    {
        public string SteamID { get; set; }

        public string AppID { get; set; }

        public string PlayerName { get; set; }

        public string GameName { get; set; }

        public string Extra { get; set; }

        public NoticeType NoticeType { get; set; }

        public string AvatarUrl { get; set; }

        public string ImagePath { get; set; }

        private static byte[] BackgroundImageBuffer { get; set; }

        public override string ToString()
        {
            return NoticeType switch
            {
                NoticeType.Playing => string.Format(AppConfig.ReplyPlaying, PlayerName, GameName),
                NoticeType.PlayChanged => string.Format(AppConfig.ReplyPlayingChanged, PlayerName, GameName),
                NoticeType.NotPlayed => string.Format(AppConfig.ReplyNotPlaying, PlayerName, GameName),
                _ => ""
            };
        }

        public bool DownloadAvatar()
        {
            if (string.IsNullOrEmpty(AvatarUrl))
            {
                return false;
            }
            string baseDirectory = NoticeType switch
            {
                NoticeType.GetAchievement => Path.Combine(MainSave.ImageDirectory, "SteamWatcher", AppID, "Achievement"),
                _ => Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Avatar")
            };
            Directory.CreateDirectory(baseDirectory);
            var t = CommonHelper.DownloadFile(AvatarUrl, $"{SteamID}.png", baseDirectory, true);
            return t;
        }

        public string GetAvatarPath()
        {
            return NoticeType switch
            {
                NoticeType.GetAchievement => Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Achievement", AppID, $"{SteamID}.png"),
                _ => Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Avatar", $"{SteamID}.png")
            };
        }

        public string? Draw()
        {
            if (!AppConfig.EnableDraw)
            {
                return null;
            }
            string backgroundFilePath = Path.Combine(MainSave.AppDirectory, "Assets", "Frame.png");
            string avatarPath = GetAvatarPath();
            if (!File.Exists(backgroundFilePath)
                || !File.Exists(avatarPath))
            {
                MainSave.CQLog.Warning("绘制图片", $"由于无法找到图片，无法进行绘制");
                return null;
            }
            if (BackgroundImageBuffer == null || BackgroundImageBuffer.Length == 0)
            {
                BackgroundImageBuffer = File.ReadAllBytes(backgroundFilePath);
            }
            Directory.CreateDirectory(Path.Combine(MainSave.ImageDirectory, "SteamWatcher"));

            return NoticeType switch
            {
                NoticeType.GetAchievement => DrawAchievementStat(),
                _ => DrawPlayingStat()
            };
        }

        private string DrawAchievementStat()
        {
            Painting painting = new(353, 87);
            painting.DrawImage(painting.LoadImageFromBuffer(BackgroundImageBuffer), new(0, 0, 353, 87));
            painting.DrawImage(painting.LoadImage(GetAvatarPath()), new SKRect() { Location = new(13, 16), Size = new(55, 55) });
            painting.DrawText($"🏆 {PlayerName}", new() { Left = 85, Right = 330 }, new SKPoint(85, 13), SKColor.Parse("#FFFFFF"), 14);
            painting.DrawText(string.IsNullOrEmpty(GameName) ? "已解锁成就" : GameName, new() { Left = 85, Right = 330 }, new SKPoint(85, 33), SKColor.Parse("#969696"), 14, wrap: string.IsNullOrEmpty(Extra));
            if (!string.IsNullOrEmpty(Extra))
            {
                painting.DrawText(Extra, new() { Left = 85, Right = 330 }, new SKPoint(85, 55), SKColor.Parse("#969696"), 14);
            }
            string filePath = Path.Combine("SteamWatcher", $"{Guid.NewGuid()}.png");
            painting.Save(Path.Combine(MainSave.ImageDirectory, filePath));
            return filePath;
        }

        private string DrawPlayingStat()
        {
            Painting painting = new(353, 87);
            painting.DrawImage(painting.LoadImageFromBuffer(BackgroundImageBuffer), new(0, 0, 353, 87));
            painting.DrawImage(painting.LoadImage(GetAvatarPath()), new SKRect() { Location = new(13, 16), Size = new(55, 55) });
            painting.DrawRectangle(new() { Location = new(68, 16), Size = new(3, 55) }, SKColor.Parse("#59bf40"), SKColors.Black, 0);
            painting.DrawText(PlayerName, Painting.Anywhere, new SKPoint(85, 13), SKColor.Parse("#d8f4ba"), 14);
            painting.DrawText("正在玩", Painting.Anywhere, new SKPoint(85, 33), SKColor.Parse("#969696"), 14);
            painting.DrawText(GameName, new() { Left = 85, Right = 330 }, new SKPoint(85, 55), SKColor.Parse("#91c257"), 14);

            string filePath = Path.Combine("SteamWatcher", $"{Guid.NewGuid()}.png");
            painting.Save(Path.Combine(MainSave.ImageDirectory, filePath));
            return filePath;
        }

        public class MonitorItem
        {
            public string SteamId { get; set; }

            public string AppId { get; set; }

            public string GameName { get; set; }

            public DateTime StartTime { get; set; }

            public GetPlayerAchievement.Achievement[] Achievements { get; set; } = [];
        }
    }
}
