using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Queries.Models.Catalog;
using Grand.Web.Features.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class PersonalizedProductsViewComponent : BaseViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors

        public PersonalizedProductsViewComponent(
            IWorkContext workContext,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _workContext = workContext;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (!_catalogSettings.PersonalizedProductsEnabled || _catalogSettings.PersonalizedProductsNumber == 0)
                return Content("");

            var products = await _mediator.Send(new GetPersonalizedProductsQuery() { CustomerId = _workContext.CurrentCustomer.Id, ProductsNumber = _catalogSettings.PersonalizedProductsNumber });

            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                PreparePictureModel = true,
                PreparePriceModel = true,
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products,
            });

            return View(model);
        }

        #endregion
    }
}
