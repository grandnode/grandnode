using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Blogs
{
    public class BlogCategoryPost : BaseGrandEntityModel
    {
        public string BlogPostId { get; set; }
        public string Name { get; set; }
    }
}
