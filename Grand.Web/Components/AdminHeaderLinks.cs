using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Core;

namespace Grand.Web.ViewComponents
{
    public class AdminHeaderLinksViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;
        private readonly IWorkContext _workContext;
        public AdminHeaderLinksViewComponent(ICommonWebService commonWebService,
            IWorkContext workContext)
        {
            this._commonWebService = commonWebService;
            this._workContext = workContext;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonWebService.PrepareAdminHeaderLinks(_workContext.CurrentCustomer);
            if (!model.DisplayAdminLink)
                return Content("");
            return View(model);
        }
    }
}