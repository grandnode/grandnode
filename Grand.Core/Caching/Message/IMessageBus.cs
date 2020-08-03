using System.Threading.Tasks;

namespace Grand.Core.Caching.Message
{
    public interface IMessageBus : IMessagePublisher, IMessageSubscriber
    {
        Task OnSubscriptionChanged(IMessageEvent message);
    }
}
