using Grand.Services.Localization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Components
{
    public class CustomerReportCustomerTimeChartViewComponent : ViewComponent
    {
        private readonly IPermissionService _permissionService;

        public CustomerReportCustomerTimeChartViewComponent(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        public IViewComponentResult Invoke()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            return View();
        }
    }
}