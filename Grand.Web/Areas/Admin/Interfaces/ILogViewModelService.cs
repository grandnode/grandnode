using Grand.Core.Domain.Logging;
using Grand.Web.Areas.Admin.Models.Logging;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ILogViewModelService
    {
        LogListModel PrepareLogListModel();
        (IEnumerable<LogModel> logModels, int totalCount) PrepareLogModel(LogListModel model, int pageIndex, int pageSize);
        LogModel PrepareLogModel(Log log);
    }
}
