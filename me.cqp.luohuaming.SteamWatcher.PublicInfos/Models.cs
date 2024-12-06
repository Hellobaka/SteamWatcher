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
}
