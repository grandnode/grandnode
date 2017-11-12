using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class TaxTypeSelectorViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;

        public TaxTypeSelectorViewComponent(ICommonWebService commonWebService)
        {
            this._commonWebService = commonWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonWebService.PrepareTaxTypeSelector();
            if (model == null)
                return Content("");

            return View(model);

        }
    }
}