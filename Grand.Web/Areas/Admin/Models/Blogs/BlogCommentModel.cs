using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;

namespace Grand.Web.Areas.Admin.Models.Blogs
{
    public partial class BlogCommentModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.Comments.Fields.BlogPost")]
        public string BlogPostId { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.Comments.Fields.BlogPost")]
        
        public string BlogPostTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.Comments.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.Comments.Fields.Customer")]
        public string CustomerInfo { get; set; }

        
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.Comments.Fields.Comment")]
        public string Comment { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.Comments.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

    }
}