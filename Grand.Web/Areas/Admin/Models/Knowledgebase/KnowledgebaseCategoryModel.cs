using FluentValidation.Attributes;
using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Validators;
using Grand.Web.Areas.Admin.Validators.Knowledgebase;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }

        public string[] SelectedCustomerRoleIds { get; set; }

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
    }
}
