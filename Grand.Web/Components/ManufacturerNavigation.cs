using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Web.Features.Models.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ManufacturerNavigationViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;

        public ManufacturerNavigationViewComponent(
            IMediator mediator,
            IWorkContext workContext,
            IStoreContext storeContex,
            CatalogSettings catalogSettings)
        {
            _mediator = mediator;
            _workContext = workContext;
            _storeContext = storeContex;
            _catalogSettings = catalogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string currentManufacturerId)
        {
            if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0)
                return Content("");

            var model = await _mediator.Send(new GetManufacturerNavigation() {
                CurrentManufacturerId = currentManufacturerId,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });

            if (!model.Manufacturers.Any())
                return Content("");

            return View(model);
        }
    }
}