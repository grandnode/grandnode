using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Models.Stores;
using Grand.Admin.Validators.News;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using Grand.Admin.Models.Customers;
using Grand.Web.Framework.Localization;

namespace Grand.Admin.Models.News
{
    [Validator(typeof(NewsItemValidator))]
    public partial class NewsItemModel : BaseNopEntityModel, ILocalizedModel<NewsLocalizedModel>
    {
        public NewsItemModel()
        {
            this.AvailableStores = new List<StoreModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            Locales = new List<NewsLocalizedModel>();
        }

        //Store mapping
        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Short")]
        [AllowHtml]
        public string Short { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Full")]
        [AllowHtml]
        public string Full { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        public IList<NewsLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Comments")]
        public int Comments { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //ACL
        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

    }

    public partial class NewsLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Short")]
        [AllowHtml]
        public string Short { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Full")]
        [AllowHtml]
        public string Full { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

    }

}