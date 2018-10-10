using Grand.Framework.Components;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Components
{
    public class CustomerReportRegisteredCustomersViewComponent : BaseViewComponent
    {
        private readonly IPermissionService _permissionService;

        public CustomerReportRegisteredCustomersViewComponent(IPermissionService permissionService)
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
