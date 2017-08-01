using Grand.Services.Localization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Components
{
    public class OrderBestsellersBriefReportByQuantityViewComponent : ViewComponent
    {
        private readonly IPermissionService _permissionService;

        public OrderBestsellersBriefReportByQuantityViewComponent(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        public IViewComponentResult Invoke()//original Action name: BestsellersBriefReportByQuantity
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            return View();
        }
    }
}