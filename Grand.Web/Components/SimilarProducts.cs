using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class SimilarProductsViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors
        public SimilarProductsViewComponent(
            ICacheManager cacheManager,
            IProductService productService,
            IAclService aclService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _productService = productService;
            _aclService = aclService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }
        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(string productId, int? productThumbPictureSize)
        {

            var productIds = await _cacheManager.GetAsync(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                   async () => (await _productService.GetProductById(productId)).SimilarProducts.OrderBy(x => x.DisplayOrder).Select(x => x.ProductId2).ToArray());

            var products = await _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                PreparePictureModel = true,
                PreparePriceModel = true,
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages,
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products
            });
           
            return View(model);
        }

        #endregion
    }
}
