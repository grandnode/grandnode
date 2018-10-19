using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.News;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.News
{
    [Validator(typeof(NewsItemValidator))]
    public partial class NewsItemModel : BaseGrandEntityModel, ILocalizedModel<NewsLocalizedModel>
    {
        public NewsItemModel()
        {
            this.AvailableStores = new List<StoreModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            Locales = new List<NewsLocalizedModel>();
        }

        //Store mapping
        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Title")]
        public string Title { get; set; }

        [UIHint("Picture")]
        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Picture")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Short")]
        public string Short { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Full")]
        public string Full { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.SeName")]
        public string SeName { get; set; }

        public IList<NewsLocalizedModel> Locales { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Comments")]
        public int Comments { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //ACL
        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

    }

    public partial class NewsLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Title")]
        
        public string Title { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Short")]
        
        public string Short { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Full")]
        
        public string Full { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.SeName")]
        
        public string SeName { get; set; }

    }

}