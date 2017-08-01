using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class CustomerNavigationViewComponent : ViewComponent
    {
        private readonly ICustomerWebService _customerWebService;

        public CustomerNavigationViewComponent(ICustomerWebService customerWebService)
        {
            this._customerWebService = customerWebService;
        }

        public IViewComponentResult Invoke(int selectedTabId = 0)
        {
            var model = _customerWebService.PrepareNavigation(selectedTabId);
            return View(model);
        }
    }
}
