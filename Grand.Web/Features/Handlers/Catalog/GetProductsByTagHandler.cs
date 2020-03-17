using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetProductsByTagHandler : IRequestHandler<GetProductsByTag, ProductsByTagModel>
    {

        private readonly IMediator _mediator;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;

        public GetProductsByTagHandler(IMediator mediator,
            IProductService productService,
            CatalogSettings catalogSettings)
        {
            _mediator = mediator;
            _productService = productService;
            _catalogSettings = catalogSettings;
        }

        public async Task<ProductsByTagModel> Handle(GetProductsByTag request, CancellationToken cancellationToken)
        {
            var model = new ProductsByTagModel {
                Id = request.ProductTag.Id,
                TagName = request.ProductTag.GetLocalized(y => y.Name, request.Language.Id),
                TagSeName = request.ProductTag.GetSeName(request.Language.Id)
            };

            //view/sorting/page size
            var options = await _mediator.Send(new GetViewSortSizeOptions() {
                Command = request.Command,
                PagingFilteringModel = request.Command,
                Language = request.Language,
                AllowCustomersToSelectPageSize = _catalogSettings.ProductsByTagAllowCustomersToSelectPageSize,
                PageSize = _catalogSettings.ProductsByTagPageSize,
                PageSizeOptions = _catalogSettings.ProductsByTagPageSizeOptions
            });
            model.PagingFilteringContext = options.command;

            //products
            var products = (await _productService.SearchProducts(
                storeId: request.Store.Id,
                productTag: request.ProductTag.Name,
                visibleIndividuallyOnly: true,
                orderBy: (ProductSortingEnum)request.Command.OrderBy,
                pageIndex: request.Command.PageNumber - 1,
                pageSize: request.Command.PageSize)).products;

            model.Products = (await _mediator.Send(new GetProductOverview() {
                Products = products,
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages
            })).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }
    }
}
