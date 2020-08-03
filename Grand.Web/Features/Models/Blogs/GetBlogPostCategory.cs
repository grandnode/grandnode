using Grand.Web.Models.Blogs;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Blogs
{
    public class GetBlogPostCategory : IRequest<IList<BlogPostCategoryModel>>
    {
    }
}
