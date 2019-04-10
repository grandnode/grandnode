using Grand.Web.Areas.Admin.Models.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IActivityLogViewModelService
    {
        Task<IList<ActivityLogTypeModel>> PrepareActivityLogTypeModels();
        Task SaveTypes(List<string> types);
        Task<ActivityLogSearchModel> PrepareActivityLogSearchModel();
        Task<(IEnumerable<ActivityLogModel> activityLogs, int totalCount)> PrepareActivityLogModel(ActivityLogSearchModel model, int pageIndex, int pageSize);
        Task<(IEnumerable<ActivityStatsModel> activityStats, int totalCount)> PrepareActivityStatModel(ActivityLogSearchModel model, int pageIndex, int pageSize);
    }
}
