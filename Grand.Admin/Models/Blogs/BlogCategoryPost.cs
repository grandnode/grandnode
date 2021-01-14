using Grand.Core.Models;

namespace Grand.Admin.Models.Blogs
{
    public class BlogCategoryPost : BaseEntityModel
    {
        public string BlogPostId { get; set; }
        public string Name { get; set; }
    }
}
