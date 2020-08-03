using Grand.Services.Seo;
using MediatR;
using Microsoft.AspNetCore.Routing;

namespace Grand.Framework.Mvc.Routing
{
    /// <summary>
    /// Represents event to handle unknow URL record entity names
    /// </summary>
    public class CustomUrlRecordEntityName : INotification
    {
        #region Properties

        /// <summary>
        /// Gets or sets information about the current routing path
        /// </summary>
        public RouteValueDictionary Values { get; private set; }

        /// <summary>
        /// Gets or sets URL record
        /// </summary>
        public UrlRecordService.UrlRecordForCaching UrlRecord { get; private set; }

        #endregion

        #region Ctor

        public CustomUrlRecordEntityName(RouteValueDictionary values, UrlRecordService.UrlRecordForCaching urlRecord)
        {
            Values = values;
            UrlRecord = urlRecord;
        }

        #endregion
    }
}
