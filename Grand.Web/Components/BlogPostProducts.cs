using Grand.Framework.Components;
using Grand.Services.Blogs;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class BlogPostProductsViewComponent : BaseViewComponent
    {
        #region Fields

        private readonly IProductViewModelService _productViewModelService;
        private readonly IBlogService _blogService; 

        #endregion

        #region Constructors

        public BlogPostProductsViewComponent(
            IProductViewModelService productViewModelService,
            IBlogService blogService)
        {
            _productViewModelService = productViewModelService;
            _blogService = blogService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(string blogPostId, int? productThumbPictureSize)
        {
            var products = await _blogService.GetProductsByBlogPostId(blogPostId);
            if(!products.Any())
                return Content("");

            var model = await _productViewModelService.PrepareIdsProducts(products.Select(x=>x.ProductId).ToArray(), productThumbPictureSize);
            if (!model.Any())
                return Content("");

            return View(model);
        }

        #endregion
    }
}
