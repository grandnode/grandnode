using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Topics
{
    public partial class TopicModel : BaseGrandEntityModel, ILocalizedModel<TopicLocalizedModel>, IAclMappingModel, IStoreMappingModel
    {
        public TopicModel()
        {
            AvailableTopicTemplates = new List<SelectListItem>();
            Locales = new List<TopicLocalizedModel>();
            AvailableStores = new List<StoreModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
        }

        //Store mapping
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }


        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.SystemName")]

        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInSitemap")]
        public bool IncludeInSitemap { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInFooterRow1")]
        public bool IncludeInFooterRow1 { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInFooterRow2")]
        public bool IncludeInFooterRow2 { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInFooterRow3")]
        public bool IncludeInFooterRow3 { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.AccessibleWhenStoreClosed")]
        public bool AccessibleWhenStoreClosed { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.IsPasswordProtected")]
        public bool IsPasswordProtected { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.Password")]
        public string Password { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.URL")]

        public string Url { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]

        public string Title { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]

        public string Body { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.TopicTemplate")]
        public string TopicTemplateId { get; set; }
        public IList<SelectListItem> AvailableTopicTemplates { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.SeName")]

        public string SeName { get; set; }

        public IList<TopicLocalizedModel> Locales { get; set; }
        //ACL
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

    }

    public partial class TopicLocalizedModel : ILocalizedModelLocal, ISlugModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]

        public string Title { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]

        public string Body { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.Fields.SeName")]

        public string SeName { get; set; }

    }
}