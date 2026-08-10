using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using Another_Mirai_Native.Abstractions.Models;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin.OrderFunctions
{
    public class GameGridFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;

        public string GetCommand() => AppConfig.GameGridCommand;

        public bool CanExecute(string destStr) => destStr.Replace("＃", "#").StartsWith(GetCommand());

        public async Task<EventHandleResult> ExecuteAsync(GroupMessageContext e)
        {
            // 解析命令参数
            string[] args = e.Message.Text.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            string steamId = null;

            // 检查是否有直接输入的SteamID或好友码
            if (args.Length > 1)
            {
                steamId = args[1];
                // 验证是否为有效的SteamID或好友码
                if (!CommonHelper.IsValidSteamId(steamId))
                {
                    await e.SendMessageAsync("无效的SteamID或好友码，请检查输入是否正确");
                    return EventHandleResult.Block;
                }

                if (steamId.Length < 17)
                {
                    // 如果是好友码，转换为SteamID
                    steamId = CommonHelper.ConvertFriendCodeToSteamId(steamId);
                    if (string.IsNullOrEmpty(steamId))
                    {
                        await e.SendMessageAsync("好友码转换失败，请检查输入是否正确");
                        return EventHandleResult.Block;
                    }
                }
            }
            else
            {
                // 检查是否已绑定
                var binding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ.Id);
                if (binding == null)
                {
                    await e.SendMessageAsync("您还没有绑定Steam账号，请先使用 " + AppConfig.SteamBindingCommand + " 进行绑定，或直接在命令后输入SteamID/好友码");
                    return EventHandleResult.Block;
                }
                steamId = binding.SteamId.ToString();
            }

            await e.FromGroup.SendGroupMessageAsync(AppConfig.ReplyDrawGameGrid);

            // 获取玩家信息
            var playerSummary = await GetPlayerSummary.Get([steamId], false);
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                await e.SendMessageAsync("无法获取Steam用户信息，请检查SteamID/好友码是否正确");
                return EventHandleResult.Block;
            }

            var player = playerSummary.players[0];

            // 获取拥有的游戏
            var ownedGames = await GetOwnedGames.Get(steamId);
            if (ownedGames == null || ownedGames.Result == null || ownedGames.Result.games == null || ownedGames.Result.games.Length == 0)
            {
                await e.SendMessageAsync("未找到游戏数据，请确认Steam账号有游戏记录");
                return EventHandleResult.Block;
            }

            // 获取成就信息
            var appIds = ownedGames.Result.games.Select(g => g.appid).ToArray();
            var achievements = await GetTopAchievementsForGames.Get(steamId, appIds);

            // 解析游戏数据
            var gridItems = GridItem.Parse(ownedGames.Result.games, achievements ?? Array.Empty<GetTopAchievementsForGames.Game>());

            if (gridItems.Count == 0)
            {
                await e.SendMessageAsync("未找到符合条件的游戏数据");
                return EventHandleResult.Block;
            }

            // 创建网格布局并绘制
            var gridLayout = new GridLayout(player, gridItems);
            string imagePath = gridLayout.Draw();
            await e.SendMessageAsync(new MessageBuilder().Image(imagePath).Build());
            return EventHandleResult.Block;
        }

        public async Task<EventHandleResult> ExecuteAsync(PrivateMessageContext e)
        {
            // 解析命令参数
            string[] args = e.Message.Text.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            string steamId = null;

            // 检查是否有直接输入的SteamID或好友码
            if (args.Length > 1)
            {
                steamId = args[1];
                // 验证是否为有效的SteamID或好友码
                if (!CommonHelper.IsValidSteamId(steamId))
                {
                    await e.SendMessageAsync("无效的SteamID或好友码，请检查输入是否正确");
                    return EventHandleResult.Block;
                }
            }
            else
            {
                // 检查是否已绑定
                var binding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ.Id);
                if (binding == null)
                {
                    await e.SendMessageAsync("您还没有绑定Steam账号，请先使用 " + AppConfig.SteamBindingCommand + " 进行绑定，或直接在命令后输入SteamID/好友码");
                    return EventHandleResult.Block;
                }
                steamId = binding.SteamId.ToString();
            }

            await e.FromQQ.SendPrivateMessageAsync(AppConfig.ReplyDrawGameGrid);

            // 获取玩家信息
            var playerSummary = await GetPlayerSummary.Get([steamId], false);
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                await e.SendMessageAsync("无法获取Steam用户信息，请检查SteamID/好友码是否正确");
                return EventHandleResult.Block;
            }

            var player = playerSummary.players[0];

            // 获取拥有的游戏
            var ownedGames = await GetOwnedGames.Get(steamId);
            if (ownedGames == null || ownedGames.Result == null || ownedGames.Result.games == null || ownedGames.Result.games.Length == 0)
            {
                await e.SendMessageAsync("未找到游戏数据，请确认Steam账号有游戏记录");
                return EventHandleResult.Block;
            }

            // 获取成就信息
            var appIds = ownedGames.Result.games.Select(g => g.appid).ToArray();
            var achievements = await GetTopAchievementsForGames.Get(steamId, appIds);

            // 解析游戏数据
            var gridItems = GridItem.Parse(ownedGames.Result.games, achievements ?? Array.Empty<GetTopAchievementsForGames.Game>());

            if (gridItems.Count == 0)
            {
                await e.SendMessageAsync("未找到符合条件的游戏数据");
                return EventHandleResult.Block;
            }

            // 创建网格布局并绘制
            var gridLayout = new GridLayout(player, gridItems);
            string imagePath = gridLayout.Draw();
            await e.SendMessageAsync(new MessageBuilder().Image(imagePath).Build());
            return EventHandleResult.Block;
        }
    }
}
