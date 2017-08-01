using Grand.Services.Localization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Components
{
    public class OrderOrderIncompleteReportViewComponent : ViewComponent
    {
        private readonly IPermissionService _permissionService;

        public OrderOrderIncompleteReportViewComponent(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        public IViewComponentResult Invoke()//original Action name: OrderIncompleteReport
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            return View();
        }
    }
}