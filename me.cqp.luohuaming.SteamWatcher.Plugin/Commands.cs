using Another_Mirai_Native.Abstractions;
using Another_Mirai_Native.Abstractions.Attributes;
using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using Another_Mirai_Native.Abstractions.Models;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin
{
    /// <summary>
    /// 指令处理（群聊与私聊通用）。触发词通过 <see cref="DynamicCommandAttribute"/> 实时读取
    /// <see cref="AppConfig"/>，修改配置后无需重启即可生效。
    /// </summary>
    public class Commands : CommandHandlerBase
    {
        public string SteamBindingCommand => AppConfig.SteamBindingCommand;

        public string SteamUnbindingCommand => AppConfig.SteamUnbindingCommand;

        public string GameGridCommand => AppConfig.GameGridCommand;

        [DynamicCommand(nameof(SteamBindingCommand), MatchMode.StartWith, MessageScope.All)]
        public async Task<EventHandleResult> BindSteam(GroupMessageContext group, PrivateMessageContext privateMsg)
        {
            var send = GetSender(group, privateMsg);
            long qq = group?.FromQQ.Id ?? privateMsg.FromQQ.Id;
            string input = GetMessageText(group, privateMsg).Replace(SteamBindingCommand, "").Trim();

            if (string.IsNullOrEmpty(input))
            {
                await send("请输入正确的SteamID或好友码，格式：17位SteamID或好友码");
                return EventHandleResult.Block;
            }

            // 验证SteamID格式
            if (!CommonHelper.IsValidSteamId(input))
            {
                await send("SteamID格式不正确，请输入17位SteamID或好友码");
                return EventHandleResult.Block;
            }

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == qq);
            if (existingBinding != null)
            {
                await send("您已经绑定了Steam账号，请先取消绑定再重新绑定");
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
                    await send("好友码转换失败，请检查输入是否正确");
                    return EventHandleResult.Block;
                }
            }

            // 验证SteamID是否存在
            var playerSummary = await GetPlayerSummary.Get([steamId], false);
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                await send("无法获取Steam用户信息，请检查SteamID是否正确");
                return EventHandleResult.Block;
            }

            var player = playerSummary.players[0];

            // 添加绑定
            AppConfig.SteamBinding.Add(new QQSteamBinding
            {
                QQ = qq,
                SteamId = long.Parse(steamId)
            });

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            await send($"绑定成功！您的Steam账号：{player.personaname}");
            return EventHandleResult.Block;
        }

        [DynamicCommand(nameof(SteamUnbindingCommand), MatchMode.StartWith, MessageScope.All)]
        public async Task<EventHandleResult> UnbindSteam(GroupMessageContext group, PrivateMessageContext privateMsg)
        {
            var send = GetSender(group, privateMsg);
            long qq = group?.FromQQ.Id ?? privateMsg.FromQQ.Id;

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == qq);
            if (existingBinding == null)
            {
                await send("您还没有绑定Steam账号，请先使用 " + SteamBindingCommand + " 进行绑定");
                return EventHandleResult.Block;
            }

            // 删除绑定
            AppConfig.SteamBinding.Remove(existingBinding);

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            await send("取消绑定成功！");
            return EventHandleResult.Block;
        }

        [DynamicCommand(nameof(GameGridCommand), MatchMode.StartWith, MessageScope.All)]
        public async Task<EventHandleResult> GameGrid(GroupMessageContext group, PrivateMessageContext privateMsg)
        {
            var send = GetSender(group, privateMsg);
            long qq = group?.FromQQ.Id ?? privateMsg.FromQQ.Id;

            // 解析命令参数
            string[] args = GetMessageText(group, privateMsg).Split([' '], StringSplitOptions.RemoveEmptyEntries);
            string steamId = null;

            // 检查是否有直接输入的SteamID或好友码
            if (args.Length > 1)
            {
                steamId = args[1];
                // 验证是否为有效的SteamID或好友码
                if (!CommonHelper.IsValidSteamId(steamId))
                {
                    await send("无效的SteamID或好友码，请检查输入是否正确");
                    return EventHandleResult.Block;
                }

                if (steamId.Length < 17)
                {
                    // 如果是好友码，转换为SteamID
                    steamId = CommonHelper.ConvertFriendCodeToSteamId(steamId);
                    if (string.IsNullOrEmpty(steamId))
                    {
                        await send("好友码转换失败，请检查输入是否正确");
                        return EventHandleResult.Block;
                    }
                }
            }
            else
            {
                // 检查是否已绑定
                var binding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == qq);
                if (binding == null)
                {
                    await send("您还没有绑定Steam账号，请先使用 " + SteamBindingCommand + " 进行绑定，或直接在命令后输入SteamID/好友码");
                    return EventHandleResult.Block;
                }
                steamId = binding.SteamId.ToString();
            }

            await send(AppConfig.ReplyDrawGameGrid);

            // 获取玩家信息
            var playerSummary = await GetPlayerSummary.Get([steamId], false);
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                await send("无法获取Steam用户信息，请检查SteamID/好友码是否正确");
                return EventHandleResult.Block;
            }

            var player = playerSummary.players[0];

            // 获取拥有的游戏
            var ownedGames = await GetOwnedGames.Get(steamId);
            if (ownedGames == null || ownedGames.Result == null || ownedGames.Result.games == null || ownedGames.Result.games.Length == 0)
            {
                await send("未找到游戏数据，请确认Steam账号有游戏记录");
                return EventHandleResult.Block;
            }

            // 获取成就信息
            var appIds = ownedGames.Result.games.Select(g => g.appid).ToArray();
            var achievements = await GetTopAchievementsForGames.Get(steamId, appIds);

            // 解析游戏数据
            var gridItems = GridItem.Parse(ownedGames.Result.games, achievements ?? Array.Empty<GetTopAchievementsForGames.Game>());

            if (gridItems.Count == 0)
            {
                await send("未找到符合条件的游戏数据");
                return EventHandleResult.Block;
            }

            // 创建网格布局并绘制
            var gridLayout = new GridLayout(player, gridItems);
            string imagePath = gridLayout.Draw();
            await send(new MessageBuilder().Image(imagePath).Build());
            return EventHandleResult.Block;
        }

        private static string GetMessageText(GroupMessageContext group, PrivateMessageContext privateMsg)
        {
            return group?.Message.Text ?? privateMsg.Message.Text;
        }

        private static Func<string, Task> GetSender(GroupMessageContext group, PrivateMessageContext privateMsg)
        {
            return group != null ? group.SendMessageAsync : privateMsg.SendMessageAsync;
        }
    }
}
