using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class CustomerNavigationViewComponent : BaseViewComponent
    {
        private readonly ICustomerViewModelService _customerViewModelService;

        public CustomerNavigationViewComponent(ICustomerViewModelService customerViewModelService)
        {
            this._customerViewModelService = customerViewModelService;
        }

        public IViewComponentResult Invoke(int selectedTabId = 0)
        {
            var model = _customerViewModelService.PrepareNavigation(selectedTabId);
            return View(model);
        }
    }
}
