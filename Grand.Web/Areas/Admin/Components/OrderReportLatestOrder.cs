using Grand.Core;
using Grand.Framework.Components;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Components
{
    public class OrderReportLatestOrderViewComponent : BaseViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        public OrderReportLatestOrderViewComponent(IPermissionService permissionService, IWorkContext workContext)
        {
            _permissionService = permissionService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content("");

            var isLoggedInAsVendor = _workContext.CurrentVendor != null;
            return View(isLoggedInAsVendor);
        }
    }
}