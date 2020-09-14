﻿using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Blogs
{
    public partial class BlogPostModel : BaseEntityModel, ILocalizedModel<BlogLocalizedModel>, IStoreMappingModel
    {
        public BlogPostModel()
        {
            AvailableStores = new List<StoreModel>();
            Locales = new List<BlogLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Title")]
        public string Title { get; set; }

        [UIHint("Picture")]
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Picture")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]

        public string Body { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.BodyOverview")]

        public string BodyOverview { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags")]

        public string Tags { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Comments")]
        public int Comments { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.CreateDate")]
        [UIHint("DateTime")]
        public DateTime CreateDate { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.SeName")]

        public string SeName { get; set; }

        public IList<BlogLocalizedModel> Locales { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }


    }

    public partial class BlogLocalizedModel : ILocalizedModelLocal, ISlugModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Title")]
        public string Title { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.BodyOverview")]

        public string BodyOverview { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]

        public string Body { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.SeName")]

        public string SeName { get; set; }

    }
}