using Grand.Core.Models;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogPostTagModel : BaseModel
    {
        public string Name { get; set; }

        public int BlogPostCount { get; set; }
    }
}