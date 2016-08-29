﻿using System;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.News
{
    public partial class NewsCommentModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.NewsItem")]
        public string NewsItemId { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.NewsItem")]
        [AllowHtml]
        public string NewsItemTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.Customer")]
        public string CustomerId { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.Customer")]
        public string CustomerInfo { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CommentTitle")]
        public string CommentTitle { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CommentText")]
        public string CommentText { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Comments.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

    }
}