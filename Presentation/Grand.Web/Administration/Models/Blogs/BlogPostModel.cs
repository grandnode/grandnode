﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Models.Stores;
using Grand.Admin.Validators.Blogs;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Blogs
{
    [Validator(typeof(BlogPostValidator))]
    public partial class BlogPostModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Language")]
        public string LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Language")]
        [AllowHtml]
        public string LanguageName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.BodyOverview")]
        [AllowHtml]
        public string BodyOverview { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags")]
        [AllowHtml]
        public string Tags { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Comments")]
        public int Comments { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //Store mapping
        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }


    }
}