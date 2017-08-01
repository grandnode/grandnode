using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.Orders;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Core.Domain.Catalog;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    [Validator(typeof(CheckoutAttributeValidator))]
    public partial class CheckoutAttributeModel : BaseGrandEntityModel, ILocalizedModel<CheckoutAttributeLocalizedModel>
    {
        public CheckoutAttributeModel()
        {
            Locales = new List<CheckoutAttributeLocalizedModel>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.TextPrompt")]
        
        public string TextPrompt { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.ShippableProductRequired")]
        public bool ShippableProductRequired { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.TaxCategory")]
        public string TaxCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.AttributeControlType")]
        
        public string AttributeControlTypeName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }


        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.MinLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMinLength { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.MaxLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMaxLength { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.FileAllowedExtensions")]
        public string ValidationFileAllowedExtensions { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.FileMaximumSize")]
        [UIHint("Int32Nullable")]
        public int? ValidationFileMaximumSize { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.DefaultValue")]
        public string DefaultValue { get; set; }

        public IList<CheckoutAttributeLocalizedModel> Locales { get; set; }

        //condition
        public bool ConditionAllowed { get; set; }
        public ConditionModel ConditionModel { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        //ACL
        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }
    }

    public partial class ConditionModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Condition.EnableCondition")]
        public bool EnableCondition { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Condition.Attributes")]
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
    public partial class CheckoutAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Fields.TextPrompt")]
        
        public string TextPrompt { get; set; }

    }
}