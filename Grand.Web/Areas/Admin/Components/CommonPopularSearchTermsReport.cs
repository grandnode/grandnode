using Grand.Framework.Components;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Components
{
    public class CommonPopularSearchTermsReportViewComponent : BaseViewComponent
    {
        private readonly IPermissionService _permissionService;

        public CommonPopularSearchTermsReportViewComponent(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        public IViewComponentResult Invoke()//original Action name: PopularSearchTermsReport
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("");

            return View();
        }
    }
}