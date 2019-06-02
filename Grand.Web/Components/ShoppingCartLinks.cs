using Grand.Core;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ShoppingCartLinksViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;
        private readonly IWorkContext _workContext;

        public ShoppingCartLinksViewComponent(ICommonViewModelService commonViewModelService, IWorkContext workContext)
        {
            _commonViewModelService = commonViewModelService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _commonViewModelService.PrepareShoppingCartLinks(_workContext.CurrentCustomer);
            return View(model);
        }
    }
}