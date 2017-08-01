using Microsoft.AspNetCore.Mvc;
using Grand.Framework;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Blogs
{
    public partial class AddBlogCommentModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Blog.Comments.CommentText")]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}