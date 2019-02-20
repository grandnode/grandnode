using Grand.Core;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class AdminHeaderLinksViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;
        private readonly IWorkContext _workContext;
        public AdminHeaderLinksViewComponent(ICommonViewModelService commonViewModelService,
            IWorkContext workContext)
        {
            this._commonViewModelService = commonViewModelService;
            this._workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await Task.Run(() => _commonViewModelService.PrepareAdminHeaderLinks(_workContext.CurrentCustomer));
            if (!model.DisplayAdminLink && !model.IsCustomerImpersonated)
                return Content("");
            return View(model);
        }
    }
}