using Grand.Services.Localization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Components
{
    public class CustomerReportCustomerTimeCharViewComponent : ViewComponent
    {
        private readonly IPermissionService _permissionService;

        public CustomerReportCustomerTimeCharViewComponent(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        public IViewComponentResult Invoke()//original Action name: ReportCustomerTimeChar
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return Content("");

            return View();
        }
    }
}