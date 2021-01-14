using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;

namespace Grand.Admin.Models.Blogs
{
    public partial class BlogCommentModel : BaseEntityModel
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