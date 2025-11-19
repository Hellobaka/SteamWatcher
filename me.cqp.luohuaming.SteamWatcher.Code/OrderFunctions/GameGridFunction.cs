using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp;

namespace me.cqp.luohuaming.SteamWatcher.Code.OrderFunctions
{
    public class GameGridFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;

        public string GetCommand() => AppConfig.GameGridCommand;

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
                    sendText.MsgToSend.Add("无效的SteamID或好友码，请检查输入是否正确");
                    return result;
                }

                if (steamId.Length < 17)
                {
                    // 如果是好友码，转换为SteamID
                    steamId = CommonHelper.ConvertFriendCodeToSteamId(steamId);
                    if (string.IsNullOrEmpty(steamId))
                    {
                        sendText.MsgToSend.Add("好友码转换失败，请检查输入是否正确");
                        return result;
                    }
                }
            }
            else
            {
                // 检查是否已绑定
                var binding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ);
                if (binding == null)
                {
                    sendText.MsgToSend.Add("您还没有绑定Steam账号，请先使用 " + AppConfig.SteamBindingCommand + " 进行绑定，或直接在命令后输入SteamID/好友码");
                    return result;
                }
                steamId = binding.SteamId.ToString();
            }

            e.FromGroup.SendGroupMessage(AppConfig.ReplyDrawGameGrid);

            // 获取玩家信息
            var playerSummary = GetPlayerSummary.Get([steamId], false).Result;
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                sendText.MsgToSend.Add("无法获取Steam用户信息，请检查SteamID/好友码是否正确");
                return result;
            }

            var player = playerSummary.players[0];

            // 获取拥有的游戏
            var ownedGames = GetOwnedGames.Get(steamId).Result;
            if (ownedGames == null || ownedGames.Result == null || ownedGames.Result.games == null || ownedGames.Result.games.Length == 0)
            {
                sendText.MsgToSend.Add("未找到游戏数据，请确认Steam账号有游戏记录");
                return result;
            }

            // 获取成就信息
            var appIds = ownedGames.Result.games.Select(g => g.appid).ToArray();
            var achievements = GetTopAchievementsForGames.Get(steamId, appIds).Result;

            // 解析游戏数据
            var gridItems = GridItem.Parse(ownedGames.Result.games, achievements ?? Array.Empty<GetTopAchievementsForGames.Game>());

            if (gridItems.Count == 0)
            {
                sendText.MsgToSend.Add("未找到符合条件的游戏数据");
                return result;
            }

            // 创建网格布局并绘制
            var gridLayout = new GridLayout(player, gridItems);
            string imagePath = gridLayout.Draw();
            sendText.MsgToSend.Add(CQApi.CQCode_Image(imagePath).ToSendString());
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
                    sendText.MsgToSend.Add("无效的SteamID或好友码，请检查输入是否正确");
                    return result;
                }
            }
            else
            {
                // 检查是否已绑定
                var binding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ);
                if (binding == null)
                {
                    sendText.MsgToSend.Add("您还没有绑定Steam账号，请先使用 " + AppConfig.SteamBindingCommand + " 进行绑定，或直接在命令后输入SteamID/好友码");
                    return result;
                }
                steamId = binding.SteamId.ToString();
            }

            e.FromQQ.SendPrivateMessage(AppConfig.ReplyDrawGameGrid);

            // 获取玩家信息
            var playerSummary = GetPlayerSummary.Get([steamId], false).Result;
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                sendText.MsgToSend.Add("无法获取Steam用户信息，请检查SteamID/好友码是否正确");
                return result;
            }

            var player = playerSummary.players[0];

            // 获取拥有的游戏
            var ownedGames = GetOwnedGames.Get(steamId).Result;
            if (ownedGames == null || ownedGames.Result == null || ownedGames.Result.games == null || ownedGames.Result.games.Length == 0)
            {
                sendText.MsgToSend.Add("未找到游戏数据，请确认Steam账号有游戏记录");
                return result;
            }

            // 获取成就信息
            var appIds = ownedGames.Result.games.Select(g => g.appid).ToArray();
            var achievements = GetTopAchievementsForGames.Get(steamId, appIds).Result;

            // 解析游戏数据
            var gridItems = GridItem.Parse(ownedGames.Result.games, achievements ?? Array.Empty<GetTopAchievementsForGames.Game>());

            if (gridItems.Count == 0)
            {
                sendText.MsgToSend.Add("未找到符合条件的游戏数据");
                return result;
            }

            // 创建网格布局并绘制
            var gridLayout = new GridLayout(player, gridItems);
            string imagePath = gridLayout.Draw();
            sendText.MsgToSend.Add(CQApi.CQCode_Image(imagePath).ToSendString());
            return result;
        }
    }
}