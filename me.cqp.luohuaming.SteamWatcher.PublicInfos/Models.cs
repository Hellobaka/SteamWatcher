using System.Collections.Generic;
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
    }
}
