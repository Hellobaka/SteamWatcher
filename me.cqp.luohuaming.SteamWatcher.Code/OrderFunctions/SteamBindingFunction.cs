using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI;

namespace me.cqp.luohuaming.SteamWatcher.Code.OrderFunctions
{
    public class SteamBindingFunction : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public int Priority { get; set; } = 10;

        public string GetCommand() => AppConfig.SteamBindingCommand;

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

            string input = e.Message.Text.Replace(GetCommand(), "").Trim();

            if (string.IsNullOrEmpty(input))
            {
                sendText.MsgToSend.Add("请输入正确的SteamID或好友码，格式：17位SteamID或好友码");
                return result;
            }

            // 验证SteamID格式
            if (!IsValidSteamId(input))
            {
                sendText.MsgToSend.Add("SteamID格式不正确，请输入17位SteamID或好友码");
                return result;
            }

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ);
            if (existingBinding != null)
            {
                sendText.MsgToSend.Add("您已经绑定了Steam账号，请先取消绑定再重新绑定");
                return result;
            }

            // 调用Steam API获取用户信息
            var steamId = input;
            if (input.Length < 17)
            {
                // 如果是好友码，转换为SteamID
                steamId = ConvertFriendCodeToSteamId(input);
                if (string.IsNullOrEmpty(steamId))
                {
                    sendText.MsgToSend.Add("好友码转换失败，请检查输入是否正确");
                    return result;
                }
            }

            // 验证SteamID是否存在
            var playerSummary = GetPlayerSummary.Get(new List<string> { steamId }, false).Result;
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                sendText.MsgToSend.Add("无法获取Steam用户信息，请检查SteamID是否正确");
                return result;
            }

            var player = playerSummary.players[0];

            // 添加绑定
            AppConfig.SteamBinding.Add(new QQSteamBinding
            {
                QQ = e.FromQQ,
                SteamId = long.Parse(steamId)
            });

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            sendText.MsgToSend.Add($"绑定成功！您的Steam账号：{player.personaname}");
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

            string input = e.Message.Text.Replace(GetCommand(), "").Trim();

            if (string.IsNullOrEmpty(input))
            {
                sendText.MsgToSend.Add("请输入正确的SteamID或好友码，格式：17位SteamID或好友码");
                return result;
            }

            // 验证SteamID格式
            if (!IsValidSteamId(input))
            {
                sendText.MsgToSend.Add("SteamID格式不正确，请输入17位SteamID或好友码");
                return result;
            }

            // 检查是否已绑定
            var existingBinding = AppConfig.SteamBinding.FirstOrDefault(b => b.QQ == e.FromQQ);
            if (existingBinding != null)
            {
                sendText.MsgToSend.Add("您已经绑定了Steam账号，请先取消绑定再重新绑定");
                return result;
            }

            // 调用Steam API获取用户信息
            var steamId = input;
            if (input.Length < 17)
            {
                // 如果是好友码，转换为SteamID
                steamId = ConvertFriendCodeToSteamId(input);
                if (string.IsNullOrEmpty(steamId))
                {
                    sendText.MsgToSend.Add("好友码转换失败，请检查输入是否正确");
                    return result;
                }
            }

            // 验证SteamID是否存在
            var playerSummary = GetPlayerSummary.Get(new List<string> { steamId }, false).Result;
            if (playerSummary == null || playerSummary.players == null || playerSummary.players.Length == 0)
            {
                sendText.MsgToSend.Add("无法获取Steam用户信息，请检查SteamID是否正确");
                return result;
            }

            var player = playerSummary.players[0];

            // 添加绑定
            AppConfig.SteamBinding.Add(new QQSteamBinding
            {
                QQ = e.FromQQ,
                SteamId = long.Parse(steamId)
            });

            // 保存配置
            AppConfig.Instance.SetConfig("SteamBinding", AppConfig.SteamBinding);

            sendText.MsgToSend.Add($"绑定成功！您的Steam账号：{player.personaname}");
            return result;
        }

        private bool IsValidSteamId(string input)
        {
            // 17位纯数字SteamID
            if (input.Length == 17 && long.TryParse(input, out _))
            {
                return true;
            }

            // 好友码（通常较短的数字）
            if (input.Length < 17 && long.TryParse(input, out _))
            {
                return true;
            }

            return false;
        }

        private string ConvertFriendCodeToSteamId(string friendCode)
        {
            // Steam好友码转换：76561197960265728 + 好友码 = SteamID
            long baseSteamId = 76561197960265728;
            if (long.TryParse(friendCode, out long friendCodeValue))
            {
                return (baseSteamId + friendCodeValue).ToString();
            }
            return null;
        }
    }
}