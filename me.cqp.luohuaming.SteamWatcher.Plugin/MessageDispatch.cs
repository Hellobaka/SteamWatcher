using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using Another_Mirai_Native.Abstractions.Handlers;
using System.Threading;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.Plugin
{
    public class MessageDispatch : IGroupMessageHandler, IPrivateMessageHandler
    {
        public Task<EventHandleResult> OnReceiveGroupMessageAsync(GroupMessageContext e, CancellationToken ct)
        {
            return Event_GroupMessage.GroupMessage(e);
        }

        public Task<EventHandleResult> OnReceivePrivateMessageAsync(PrivateMessageContext e, CancellationToken ct)
        {
            return Event_PrivateMessage.PrivateMessage(e);
        }
    }
}
