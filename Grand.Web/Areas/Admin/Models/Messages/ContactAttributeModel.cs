using FluentValidation.Attributes;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(ContactAttributeValidator))]
    public partial class ContactAttributeModel : BaseGrandEntityModel, ILocalizedModel<ContactAttributeLocalizedModel>
    {
        public ContactAttributeModel()
        {
            Locales = new List<ContactAttributeLocalizedModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.TextPrompt")]
        
        public string TextPrompt { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.AttributeControlType")]
        
        public string AttributeControlTypeName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }


        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.MinLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMinLength { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.MaxLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMaxLength { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.FileAllowedExtensions")]
        public string ValidationFileAllowedExtensions { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.FileMaximumSize")]
        [UIHint("Int32Nullable")]
        public int? ValidationFileMaximumSize { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.DefaultValue")]
        public string DefaultValue { get; set; }

        public IList<ContactAttributeLocalizedModel> Locales { get; set; }

        //condition
        public bool ConditionAllowed { get; set; }
        public ConditionModel ConditionModel { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        //ACL
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }
    }

    public partial class ConditionModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Condition.EnableCondition")]
        public bool EnableCondition { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Condition.Attributes")]
        public string SelectedAttributeId { get; set; }

        public IList<AttributeConditionModel> ConditionAttributes { get; set; }
    }
    public partial class AttributeConditionModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<SelectListItem> Values { get; set; }

        public string SelectedValueId { get; set; }
    }
    public partial class ContactAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Fields.TextPrompt")]
        
        public string TextPrompt { get; set; }

    }
}