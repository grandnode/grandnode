using System;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogCommentModel : BaseGrandEntityModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAvatarUrl { get; set; }

        public string CommentText { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool AllowViewingProfiles { get; set; }
    }
}