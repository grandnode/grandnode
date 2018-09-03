using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.Knowledgebase;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Knowledgebase
{
    [Validator(typeof(KnowledgebaseArticleModelValidator))]
    public class KnowledgebaseArticleModel : BaseGrandEntityModel, ILocalizedModel<KnowledgebaseArticleLocalizedModel>
    {
        public KnowledgebaseArticleModel()
        {
            Categories = new List<SelectListItem>();
            Locales = new List<KnowledgebaseArticleLocalizedModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            AvailableStores = new List<StoreModel>();
        }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Content")]
        public string Content { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.ParentCategoryId")]
        public string ParentCategoryId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.SeName")]
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.ShowOnHomepage")]
        public bool ShowOnHomepage { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public IList<KnowledgebaseArticleLocalizedModel> Locales { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }

        public string[] SelectedCustomerRoleIds { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        public partial class ActivityLogModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.ActivityLogType")]
            public string ActivityLogTypeName { get; set; }
            [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.ActivityLog.Comment")]
            public string Comment { get; set; }
            [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.ActivityLog.CreatedOn")]
            public DateTime CreatedOn { get; set; }
            [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.ActivityLog.Customer")]
            public string CustomerId { get; set; }
            public string CustomerEmail { get; set; }
        }

        public partial class AddRelatedArticleModel : BaseGrandEntityModel
        {
            public AddRelatedArticleModel()
            {
                AvailableArticles = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Related.SearchArticleName")]
            public string SearchArticleName { get; set; }

            public string ArticleId { get; set; }

            public string[] SelectedArticlesIds { get; set; }

            public IList<SelectListItem> AvailableArticles { get; set; }
        }
    }

    public class KnowledgebaseArticleLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Content")]
        public string Content { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.SeName")]
        public string SeName { get; set; }
    }
}
