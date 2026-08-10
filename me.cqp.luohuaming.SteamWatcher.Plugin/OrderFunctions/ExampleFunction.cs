using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin.OrderFunctions
{
    public class ExampleFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = false;

        public int Priority { get; set; } = 10;

        public string GetCommand() => "这里输入触发指令";

        public bool CanExecute(string destStr) => destStr.Replace("＃", "#").StartsWith(GetCommand());

        public async Task<EventHandleResult> ExecuteAsync(GroupMessageContext e)
        {
            await e.SendMessageAsync("这里输入需要发送的文本");
            return EventHandleResult.Block;
        }

        public async Task<EventHandleResult> ExecuteAsync(PrivateMessageContext e)
        {
            await e.SendMessageAsync("这里输入需要发送的文本");
            return EventHandleResult.Block;
        }
    }
}
