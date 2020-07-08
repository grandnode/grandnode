using Grand.Core;
using Grand.Domain.Tax;
using Grand.Framework.Components;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class TaxTypeSelectorViewComponent : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;

        public TaxTypeSelectorViewComponent(IWorkContext workContext, TaxSettings taxSettings)
        {
            _workContext = workContext;
            _taxSettings = taxSettings;
        }

        public IViewComponentResult Invoke()
        {
            var model = PrepareTaxTypeSelector();
            if (model == null)
                return Content("");

            return View(model);
        }
        private TaxTypeSelectorModel PrepareTaxTypeSelector()
        {
            if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                return null;

            var model = new TaxTypeSelectorModel {
                CurrentTaxType = _workContext.TaxDisplayType
            };
            return model;
        }
    }
}