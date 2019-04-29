using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    public interface IScheduleTask
    {
        Task Execute();
    }
}
