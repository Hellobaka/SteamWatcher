using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin.OrderFunctions
{
    public class SteamBindingFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;

        public string GetCommand() => AppConfig.SteamBindingCommand;

        public bool CanExecute(string destStr) => destStr.Replace("＃", "#").StartsWith(GetCommand());

        public async Task<EventHandleResult> ExecuteAsync(GroupMessageContext e)
        {
            string input = e.Message.Text.Replace(GetCommand(), "").Trim();

            if (string.IsNullOrEmpty(input))
            {
                await e.SendMessageAsync("请输入正确的SteamID或好友码，格式：17位SteamID或好友码");
                return EventHandleResult.Block;
            }

            // 验证SteamID格式
            if (!CommonHelper.IsValidSteamId(input))
            {
                await e.SendMessageAsync("SteamID格式不正确，请输入17位SteamID或好友码");
                return EventHandleResult.Block;
            }

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ.Id);
            if (existingBinding != null)
            {
                await e.SendMessageAsync("您已经绑定了Steam账号，请先取消绑定再重新绑定");
                return EventHandleResult.Block;
            }

            // 调用Steam API获取用户信息
            var steamId = input;
            if (input.Length < 17)
            {
                // 如果是好友码，转换为SteamID
                steamId = CommonHelper.ConvertFriendCodeToSteamId(input);
                if (string.IsNullOrEmpty(steamId))
                {
                    await e.SendMessageAsync("好友码转换失败，请检查输入是否正确");
                    return EventHandleResult.Block;
                }
            }

            // 验证SteamID是否存在
            var playerSummary = await GetPlayerSummary.Get([steamId], false);
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                await e.SendMessageAsync("无法获取Steam用户信息，请检查SteamID是否正确");
                return EventHandleResult.Block;
            }

            var player = playerSummary.players[0];

            // 添加绑定
            AppConfig.SteamBinding.Add(new QQSteamBinding
            {
                QQ = e.FromQQ.Id,
                SteamId = long.Parse(steamId)
            });

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            await e.SendMessageAsync($"绑定成功！您的Steam账号：{player.personaname}");
            return EventHandleResult.Block;
        }

        public async Task<EventHandleResult> ExecuteAsync(PrivateMessageContext e)
        {
            string input = e.Message.Text.Replace(GetCommand(), "").Trim();

            if (string.IsNullOrEmpty(input))
            {
                await e.SendMessageAsync("请输入正确的SteamID或好友码，格式：17位SteamID或好友码");
                return EventHandleResult.Block;
            }

            // 验证SteamID格式
            if (!CommonHelper.IsValidSteamId(input))
            {
                await e.SendMessageAsync("SteamID格式不正确，请输入17位SteamID或好友码");
                return EventHandleResult.Block;
            }

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ.Id);
            if (existingBinding != null)
            {
                await e.SendMessageAsync("您已经绑定了Steam账号，请先取消绑定再重新绑定");
                return EventHandleResult.Block;
            }

            // 调用Steam API获取用户信息
            var steamId = input;
            if (input.Length < 17)
            {
                // 如果是好友码，转换为SteamID
                steamId = CommonHelper.ConvertFriendCodeToSteamId(input);
                if (string.IsNullOrEmpty(steamId))
                {
                    await e.SendMessageAsync("好友码转换失败，请检查输入是否正确");
                    return EventHandleResult.Block;
                }
            }

            // 验证SteamID是否存在
            var playerSummary = await GetPlayerSummary.Get([steamId], false);
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                await e.SendMessageAsync("无法获取Steam用户信息，请检查SteamID是否正确");
                return EventHandleResult.Block;
            }

            var player = playerSummary.players[0];

            // 添加绑定
            AppConfig.SteamBinding.Add(new QQSteamBinding
            {
                QQ = e.FromQQ.Id,
                SteamId = long.Parse(steamId)
            });

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            await e.SendMessageAsync($"绑定成功！您的Steam账号：{player.personaname}");
            return EventHandleResult.Block;
        }
    }
}
