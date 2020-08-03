using Grand.Domain.Customers;
using Grand.Framework.Kendoui;
using Grand.Framework.Security.Authorization;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Customers)]
    public partial class OnlineCustomerController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IGeoLookupService _geoLookupService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CustomerSettings _customerSettings;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Constructors

        public OnlineCustomerController(ICustomerService customerService,
            IGeoLookupService geoLookupService, IDateTimeHelper dateTimeHelper,
            CustomerSettings customerSettings,
            ILocalizationService localizationService)
        {
            _customerService = customerService;
            _geoLookupService = geoLookupService;
            _dateTimeHelper = dateTimeHelper;
            _customerSettings = customerSettings;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public IActionResult List() => View();

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var customers = await _customerService.GetOnlineCustomers(DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes),
                null, command.Page - 1, command.PageSize);
            var items = new List<OnlineCustomerModel>();
            foreach (var x in customers)
            {
                var item = new OnlineCustomerModel() {
                    Id = x.Id,
                    CustomerInfo = !string.IsNullOrEmpty(x.Email) ? x.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                    LastIpAddress = x.LastIpAddress,
                    Location = _geoLookupService.LookupCountryName(x.LastIpAddress),
                    LastActivityDate = _dateTimeHelper.ConvertToUserTime(x.LastActivityDateUtc, DateTimeKind.Utc),
                    LastVisitedPage = _customerSettings.StoreLastVisitedPage ?
                        x.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastVisitedPage) :
                        _localizationService.GetResource("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage.Disabled")
                };
                items.Add(item);
            }

            var gridModel = new DataSourceResult {
                Data = items,
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        #endregion
    }
}
