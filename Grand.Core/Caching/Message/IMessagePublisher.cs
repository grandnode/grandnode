using System.Threading.Tasks;

namespace Grand.Core.Caching.Message
{
    public interface IMessagePublisher
    {
        Task PublishAsync<TMessage>(TMessage msg) where TMessage : IMessageEvent;
    }
}
