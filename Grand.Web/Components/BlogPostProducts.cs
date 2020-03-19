using Grand.Core.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Features.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class BlogPostProductsViewComponent : BaseViewComponent
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IMediator _mediator;
        private readonly IBlogService _blogService;

        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors

        public BlogPostProductsViewComponent(
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IMediator mediator,
            IBlogService blogService,
            CatalogSettings catalogSettings)
        {
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _mediator = mediator;
            _blogService = blogService;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(string blogPostId, int? productThumbPictureSize)
        {
            var blogproducts = await _blogService.GetProductsByBlogPostId(blogPostId);
            if (!blogproducts.Any())
                return Content("");

            var products = await _productService.GetProductsByIds(blogproducts.Select(x => x.ProductId).ToArray());
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
