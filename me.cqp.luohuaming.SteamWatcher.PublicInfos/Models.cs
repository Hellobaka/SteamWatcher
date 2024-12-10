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

    public class MonitorItem
    {
        public long GroupId { get; set; }

        public List<string> TargetId { get; set; }
    }
    public enum NoticeType
    {
        Playing,

        PlayChanged,

        NotPlayed,
    }

    public class MonitorNoticeItem
    {
        public string SteamID { get; set; }

        public string PlayerName { get; set; }

        public string GameName { get; set; }

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
            Directory.CreateDirectory(Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Avatar"));
            var t = CommonHelper.DownloadFile(AvatarUrl, $"{SteamID}.png", Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Avatar"), true);
            return t;
        }

        public string? Draw()
        {
            if (!AppConfig.EnableDraw)
            {
                return null;
            }
            string backgroundFilePath = Path.Combine(MainSave.AppDirectory, "Assets", "Frame.png");
            string avatarPath = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Avatar", $"{SteamID}.png");
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
            using Painting painting = new(353, 87);
            painting.DrawImage(painting.LoadImageFromBuffer(BackgroundImageBuffer), new(0, 0, 353, 87));
            painting.DrawImage(painting.LoadImage(avatarPath), new SKRect() { Location = new(13, 16), Size = new(55, 55) });
            painting.DrawRectangle(new() { Location = new(68, 16), Size = new(3, 55) }, SKColor.Parse("#59bf40"), SKColors.Black, 0);
            painting.DrawText(PlayerName, Painting.Anywhere, new SKPoint(85, 13), SKColor.Parse("#d8f4ba"), 14);
            painting.DrawText("正在玩", Painting.Anywhere, new SKPoint(85, 33), SKColor.Parse("#969696"), 14);
            painting.DrawText(GameName, new() { Left = 85, Right = 340 }, new SKPoint(85, 55), SKColor.Parse("#91c257"), 14);

            string filePath = Path.Combine("SteamWatcher", $"{Guid.NewGuid()}.png");
            painting.Save(Path.Combine(MainSave.ImageDirectory, filePath));
            return filePath;
        }
    }
}
