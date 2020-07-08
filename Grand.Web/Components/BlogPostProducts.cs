using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
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
        private readonly IMediator _mediator;
        private readonly IBlogService _blogService;

        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors

        public BlogPostProductsViewComponent(
            IProductService productService,
            IMediator mediator,
            IBlogService blogService,
            CatalogSettings catalogSettings)
        {
            _productService = productService;
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
