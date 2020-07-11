using Grand.Domain.Blogs;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IBlogViewModelService
    {
        Task<BlogPostModel> PrepareBlogPostModel();
        Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel);
        Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel, BlogPost blogPost);
        Task<BlogPostModel> PrepareBlogPostModel(BlogPost blogPost);
        Task<(IEnumerable<BlogPostModel> blogPosts, int totalCount)> PrepareBlogPostsModel(int pageIndex, int pageSize);
        Task<BlogPost> InsertBlogPostModel(BlogPostModel model);
        Task<BlogPost> UpdateBlogPostModel(BlogPostModel model, BlogPost blogPost);
        Task<(IEnumerable<BlogCommentModel> blogComments, int totalCount)> PrepareBlogPostCommentsModel(string filterByBlogPostId, int pageIndex, int pageSize);
        Task<(IEnumerable<BlogProductModel> blogProducts, int totalCount)> PrepareBlogProductsModel(string filterByBlogPostId, int pageIndex, int pageSize);
        Task<BlogProductModel.AddProductModel> PrepareBlogModelAddProductModel(string blogPostId);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(BlogProductModel.AddProductModel model, int pageIndex, int pageSize);
        Task InsertProductModel(string blogPostId, BlogProductModel.AddProductModel model);
        Task UpdateProductModel(BlogProductModel model);
        Task DeleteProductModel(string id);
    }
}
