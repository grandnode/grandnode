using System;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogCommentModel : BaseNopEntityModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAvatarUrl { get; set; }

        public string CommentText { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool AllowViewingProfiles { get; set; }
    }
}