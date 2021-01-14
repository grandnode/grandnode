using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;

namespace Grand.Admin.Models.News
{
    public partial class NewsCommentModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.NewsItem")]
        public string NewsItemId { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.NewsItem")]
        
        public string NewsItemTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.Customer")]
        public string CustomerInfo { get; set; }

        
        [GrandResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CommentTitle")]
        public string CommentTitle { get; set; }

        
        [GrandResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CommentText")]
        public string CommentText { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

    }
}