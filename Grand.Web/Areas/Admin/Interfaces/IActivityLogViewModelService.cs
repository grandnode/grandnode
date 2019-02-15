using Grand.Web.Areas.Admin.Models.Logging;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IActivityLogViewModelService
    {
        IList<ActivityLogTypeModel> PrepareActivityLogTypeModels();
        void SaveTypes(List<string> types);
        ActivityLogSearchModel PrepareActivityLogSearchModel();
        (IEnumerable<ActivityLogModel> activityLogs, int totalCount) PrepareActivityLogModel(ActivityLogSearchModel model, int pageIndex, int pageSize);
        (IEnumerable<ActivityStatsModel> activityStats, int totalCount) PrepareActivityStatModel(ActivityLogSearchModel model, int pageIndex, int pageSize);
    }
}
