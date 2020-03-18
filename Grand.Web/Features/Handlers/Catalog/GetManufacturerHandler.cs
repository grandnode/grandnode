using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetManufacturerHandler : IRequestHandler<GetManufacturer, ManufacturerModel>
    {
        private readonly IMediator _mediator;
        private readonly IWebHelper _webHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly ICacheManager _cacheManager;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;

        public GetManufacturerHandler(
            IMediator mediator,
            IWebHelper webHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            ICacheManager cacheManager,
            IProductService productService,
            CatalogSettings catalogSettings)
        {
            _mediator = mediator;
            _webHelper = webHelper;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _cacheManager = cacheManager;
            _productService = productService;
            _catalogSettings = catalogSettings;
        }

        public async Task<ManufacturerModel> Handle(GetManufacturer request, CancellationToken cancellationToken)
        {
            var model = request.Manufacturer.ToModel(request.Language);

            if (request.Command != null && request.Command.OrderBy == null && request.Manufacturer.DefaultSort >= 0)
                request.Command.OrderBy = request.Manufacturer.DefaultSort;

            //view/sorting/page size
            var options = await _mediator.Send(new GetViewSortSizeOptions() {
                Command = request.Command,
                PagingFilteringModel = request.Command,
                Language = request.Language,
                AllowCustomersToSelectPageSize = request.Manufacturer.AllowCustomersToSelectPageSize,
                PageSizeOptions = request.Manufacturer.PageSizeOptions,
                PageSize = request.Manufacturer.PageSize
            });
            model.PagingFilteringContext = options.command;

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(request.Manufacturer.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, request.Manufacturer.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, request.Currency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, request.Currency);
            }

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                IPagedList<Product> featuredProducts = null;

                //We cache a value indicating whether we have featured products
                string cacheKey = string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY,
                    request.Manufacturer.Id,
                    string.Join(",", request.Customer.GetCustomerRoleIds()),
                    request.Store.Id);
                var hasFeaturedProductsCache = await _cacheManager.GetAsync<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = (await _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       manufacturerId: request.Manufacturer.Id,
                       storeId: request.Store.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true)).products;
                    hasFeaturedProductsCache = featuredProducts.Any();
                    await _cacheManager.SetAsync(cacheKey, hasFeaturedProductsCache, CommonHelper.CacheTimeMinutes);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the manufacturer has featured products
                    //let's load them
                    featuredProducts = (await _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       manufacturerId: request.Manufacturer.Id,
                       storeId: request.Store.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true)).products;
                }
                if (featuredProducts != null)
                {
                    model.FeaturedProducts = (await _mediator.Send(new GetProductOverview() {
                        Products = featuredProducts,
                    })).ToList();
                }
            }
            var products = (await _productService.SearchProducts(
                manufacturerId: request.Manufacturer.Id,
                storeId: request.Store.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
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
