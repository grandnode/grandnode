using Grand.Framework.Components;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Components
{
    public class CustomerReportCustomerTimeChartViewComponent : BaseViewComponent
    {
        private readonly IPermissionService _permissionService;

        public CustomerReportCustomerTimeChartViewComponent(IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageReports))
                return Content("");

            return View();
        }
    }
}