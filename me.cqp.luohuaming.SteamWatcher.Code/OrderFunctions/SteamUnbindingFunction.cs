using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;

namespace me.cqp.luohuaming.SteamWatcher.Code.OrderFunctions
{
    public class SteamUnbindingFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;

        public string GetCommand() => AppConfig.SteamUnbindingCommand;

        public bool CanExecute(string destStr) => destStr.Replace("＃", "#").StartsWith(GetCommand());//这里判断是否能触发指令

        public FunctionResult Execute(CQGroupMessageEventArgs e)//群聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromGroup,
            };
            result.SendObject.Add(sendText);

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ);
            if (existingBinding == null)
            {
                sendText.MsgToSend.Add("您还没有绑定Steam账号");
                return result;
            }

            // 删除绑定
            AppConfig.SteamBinding.Remove(existingBinding);

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            sendText.MsgToSend.Add("取消绑定成功！");
            return result;
        }

        public FunctionResult Execute(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromQQ,
            };
            result.SendObject.Add(sendText);

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ);
            if (existingBinding == null)
            {
                sendText.MsgToSend.Add("您还没有绑定Steam账号");
                return result;
            }

            // 删除绑定
            AppConfig.SteamBinding.Remove(existingBinding);

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            sendText.MsgToSend.Add("取消绑定成功！");
            return result;
        }
    }
}