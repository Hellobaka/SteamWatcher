using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.EventArgs;

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

        private static byte[] BackgroundImageBuffer {  get; set; }

        private static Font Font { get; set; } 

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
            string backgroundFilePath = Path.Combine(MainSave.AppDirectory, "Assets", "Frame.png");
            string avatarPath = Path.Combine(MainSave.ImageDirectory, "SteamWatcher", "Avatar", $"{SteamID}.png");
            if (!File.Exists(backgroundFilePath)
                || !File.Exists(avatarPath))
            {
                MainSave.CQLog.Warning("绘制图片", $"由于无法找到图片，无法进行绘制");
                return null;
            }
            if (BackgroundImageBuffer  == null || BackgroundImageBuffer.Length == 0)
            {
                BackgroundImageBuffer = File.ReadAllBytes(backgroundFilePath);
            }
            Directory.CreateDirectory(Path.Combine(MainSave.ImageDirectory, "SteamWatcher"));
            using MemoryStream memoryStream = new(BackgroundImageBuffer);
            using Bitmap img = (Bitmap)Image.FromStream(memoryStream);
            using Graphics g = Graphics.FromImage(img);
            using Bitmap avatar = (Bitmap)Image.FromFile(avatarPath);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = AppConfig.DrawMethod ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAliasGridFit;

            Font ??= new(AppConfig.CustomFont, 14 * 72f / g.DpiY);

            g.DrawImage(avatar, new Rectangle(new Point(13, 16), new Size(55, 55)));
            g.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#59bf40")), new Rectangle(new Point(68, 16), new Size(3, 55)));

            g.DrawString(PlayerName, Font, new SolidBrush(ColorTranslator.FromHtml("#d8f4ba")), new PointF(85, 14));
            g.DrawString("正在玩", Font, new SolidBrush(ColorTranslator.FromHtml("#969696")), new PointF(85, 36));
            g.DrawString(GameName, Font, new SolidBrush(ColorTranslator.FromHtml("#91c257")), new PointF(85, 56));
            
            string filePath = Path.Combine("SteamWatcher", $"{DateTime.Now:yyyyMMddHHmmss}.png");
            img.Save(Path.Combine(MainSave.ImageDirectory, filePath));

            return filePath;
        }
    }
}
