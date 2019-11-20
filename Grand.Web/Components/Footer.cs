using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class FooterViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public FooterViewComponent(ICommonViewModelService commonViewModelService)
        {
            _commonViewModelService = commonViewModelService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _commonViewModelService.PrepareFooter();
            return View(model);

        }
    }
}