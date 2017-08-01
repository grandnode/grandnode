using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogPostTagModel : BaseGrandModel
    {
        public string Name { get; set; }

        public int BlogPostCount { get; set; }
    }
}