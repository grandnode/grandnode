using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public interface IConsumer<T>
    {
        Task HandleEvent(T eventMessage);
    }
}
