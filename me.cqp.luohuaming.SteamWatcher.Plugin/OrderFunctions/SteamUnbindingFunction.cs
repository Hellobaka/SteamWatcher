using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin.OrderFunctions
{
    public class SteamUnbindingFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;

        public string GetCommand() => AppConfig.SteamUnbindingCommand;

        public bool CanExecute(string destStr) => destStr.Replace("＃", "#").StartsWith(GetCommand());

        public async Task<EventHandleResult> ExecuteAsync(GroupMessageContext e)
        {
            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ.Id);
            if (existingBinding == null)
            {
                await e.SendMessageAsync("您还没有绑定Steam账号，请先使用 " + AppConfig.SteamBindingCommand + " 进行绑定");
                return EventHandleResult.Block;
            }

            // 删除绑定
            AppConfig.SteamBinding.Remove(existingBinding);

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            await e.SendMessageAsync("取消绑定成功！");
            return EventHandleResult.Block;
        }

        public async Task<EventHandleResult> ExecuteAsync(PrivateMessageContext e)
        {
            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ.Id);
            if (existingBinding == null)
            {
                await e.SendMessageAsync("您还没有绑定Steam账号，请先使用 " + AppConfig.SteamBindingCommand + " 进行绑定");
                return EventHandleResult.Block;
            }

            // 删除绑定
            AppConfig.SteamBinding.Remove(existingBinding);

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            await e.SendMessageAsync("取消绑定成功！");
            return EventHandleResult.Block;
        }
    }
}
