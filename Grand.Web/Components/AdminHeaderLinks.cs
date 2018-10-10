using Grand.Core;
using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

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

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareAdminHeaderLinks(_workContext.CurrentCustomer);
            if (!model.DisplayAdminLink)
                return Content("");
            return View(model);
        }
    }
}