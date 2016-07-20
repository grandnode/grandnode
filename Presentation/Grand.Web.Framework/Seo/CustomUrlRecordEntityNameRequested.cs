
using System.Web.Routing;
using Grand.Services.Seo;

namespace Grand.Web.Framework.Seo
{
    /// <summary>
    /// Event to handle unknow URL record entity names
    /// </summary>
    public class CustomUrlRecordEntityNameRequested
    {
        public CustomUrlRecordEntityNameRequested(RouteData routeData, UrlRecordService.UrlRecordForCaching urlRecord)
        {
            this.RouteData = routeData;
            this.UrlRecord = urlRecord;
        }

        public RouteData RouteData { get; private set; }
        public UrlRecordService.UrlRecordForCaching UrlRecord { get; private set; }
    }
}
