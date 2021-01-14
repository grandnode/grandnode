using Grand.Domain.Logging;
using Grand.Admin.Models.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Admin.Interfaces
{
    public interface ILogViewModelService
    {
        LogListModel PrepareLogListModel();
        Task<(IEnumerable<LogModel> logModels, int totalCount)> PrepareLogModel(LogListModel model, int pageIndex, int pageSize);
        Task<LogModel> PrepareLogModel(Log log);
    }
}
