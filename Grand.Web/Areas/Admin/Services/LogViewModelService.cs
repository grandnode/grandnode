using Grand.Core;
using Grand.Domain.Logging;
using Grand.Framework.Extensions;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class LogViewModelService: ILogViewModelService
    {
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IServiceProvider _serviceProvider;

        public LogViewModelService(ILogger logger, IWorkContext workContext,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _workContext = workContext;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _serviceProvider = serviceProvider;
        }

        public virtual LogListModel PrepareLogListModel()
        {
            var model = new LogListModel
            {
                AvailableLogLevels = LogLevel.Debug.ToSelectList(_localizationService, _workContext, false).ToList()
            };
            model.AvailableLogLevels.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            return model;
        }
        public virtual async Task<(IEnumerable<LogModel> logModels, int totalCount)> PrepareLogModel(LogListModel model, int pageIndex, int pageSize)
        {
            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            LogLevel? logLevel = model.LogLevelId > 0 ? (LogLevel?)(model.LogLevelId) : null;


            var logItems = await _logger.GetAllLogs(createdOnFromValue, createdToFromValue, model.Message,
                logLevel, pageIndex - 1, pageSize);
            return (logItems.Select(x => new LogModel
            {
                Id = x.Id,
                LogLevel = x.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                ShortMessage = x.ShortMessage,
                //little hack here:
                //ensure that FullMessage is not returned
                //otherwise, we can get the following error if log records have too long FullMessage:
                //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
                //also it improves performance
                //FullMessage = x.FullMessage,
                FullMessage = "",
                IpAddress = x.IpAddress,
                CustomerId = x.CustomerId,
                PageUrl = x.PageUrl,
                ReferrerUrl = x.ReferrerUrl,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
            }), logItems.TotalCount);

        }
        public virtual async Task<LogModel> PrepareLogModel(Log log)
        {
            var model = new LogModel
            {
                Id = log.Id,
                LogLevel = log.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                ShortMessage = log.ShortMessage,
                FullMessage = log.FullMessage,
                IpAddress = log.IpAddress,
                CustomerId = log.CustomerId,
                CustomerEmail = !String.IsNullOrEmpty(log.CustomerId) ? (await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(log.CustomerId))?.Email : "",
                PageUrl = log.PageUrl,
                ReferrerUrl = log.ReferrerUrl,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(log.CreatedOnUtc, DateTimeKind.Utc)
            };
            return model;
        }
    }
}
