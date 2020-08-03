using System.Threading.Tasks;

namespace Grand.Core.Caching.Message
{
    public interface IMessageSubscriber
    {
        Task SubscribeAsync();
    }
}
