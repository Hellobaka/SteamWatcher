using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using me.cqp.luohuaming.SteamWatcher.PublicInfos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin
{
    public static class Event_PrivateMessage
    {
        public static async Task<EventHandleResult> PrivateMessage(PrivateMessageContext e)
        {
            try
            {
                foreach (var item in MainSave.Instances.OrderByDescending(x => x.Priority)
                    .Where(item => item.CanExecute(e.Message.Text)))
                {
                    var result = await item.ExecuteAsync(e);
                    if (result == EventHandleResult.Block)
                    {
                        return result;
                    }
                }
                return EventHandleResult.Pass;
            }
            catch (Exception exc)
            {
                MainSave.Logger?.Info("异常抛出", exc.Message + exc.StackTrace);
                return EventHandleResult.Pass;
            }
        }
    }
}
