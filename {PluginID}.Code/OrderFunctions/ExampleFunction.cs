using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using {PluginID}.Sdk.Cqp.EventArgs;
using {PluginID}.PublicInfos;

namespace {PluginID}.Code.OrderFunctions
{
    public class ExampleFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;
        
        public string GetCommand() => "这里输入触发指令";

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

            sendText.MsgToSend.Add("这里输入需要发送的文本");
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
            
            sendText.MsgToSend.Add("这里输入需要发送的文本");
            return result;
        }
    }
}
