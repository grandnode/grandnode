using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Configuration
{
    public partial interface IGoogleAnalyticsService
    {
        GoogleAnalyticsResult GetDataByGeneral(DateTime startDate, DateTime endDate);
        GoogleAnalyticsResult GetDataByLocalization(DateTime startDate, DateTime endDate);
        GoogleAnalyticsResult GetDataBySource(DateTime startDate, DateTime endDate);
        GoogleAnalyticsResult GetDataByExit(DateTime startDate, DateTime endDate);
        GoogleAnalyticsResult GetDataByDevice(DateTime startDate, DateTime endDate);
    }
}
