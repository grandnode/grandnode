using Grand.Core;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class HeaderLinksViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;
        private readonly IWorkContext _workContext;

        public HeaderLinksViewComponent(ICommonViewModelService commonViewModelService, IWorkContext workContext)
        {
            _commonViewModelService = commonViewModelService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _commonViewModelService.PrepareHeaderLinks(_workContext.CurrentCustomer);
            return View(model);
        }
    }
}