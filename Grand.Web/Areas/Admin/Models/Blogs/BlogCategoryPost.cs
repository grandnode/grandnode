using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Blogs
{
    public class BlogCategoryPost : BaseEntityModel
    {
        public string BlogPostId { get; set; }
        public string Name { get; set; }
    }
}
