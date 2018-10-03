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
    [Validator(typeof(KnowledgebaseCategoryModelValidator))]
    public class KnowledgebaseCategoryModel : BaseGrandEntityModel, ILocalizedModel<KnowledgebaseCategoryLocalizedModel>
    {
        public KnowledgebaseCategoryModel()
        {
            Categories = new List<SelectListItem>();
            Locales = new List<KnowledgebaseCategoryLocalizedModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            AvailableStores = new List<StoreModel>();
        }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.ParentCategoryId")]
        public string ParentCategoryId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Description")]
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Published")]
        public bool Published { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public IList<KnowledgebaseCategoryLocalizedModel> Locales { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.SeName")]
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }

        public string[] SelectedCustomerRoleIds { get; set; }

        //Store mapping
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
    }

    public class KnowledgebaseCategoryLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Description")]
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.SeName")]
        public string SeName { get; set; }
    }
}
