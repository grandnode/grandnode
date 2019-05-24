using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public interface IConsumer<T>
    {
        void HandleEvent(T eventMessage);
        Task HandleEventAsync(T eventMessage);
    }
}
