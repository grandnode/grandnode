using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Common;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Areas.Admin.Models.Common
{
    [Validator(typeof(AddressAttributeValidator))]
    public partial class AddressAttributeModel : BaseGrandEntityModel, ILocalizedModel<AddressAttributeLocalizedModel>
    {
        public AddressAttributeModel()
        {
            Locales = new List<AddressAttributeLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Fields.AttributeControlType")]
        public int AttributeControlTypeId { get; set; }
        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Fields.AttributeControlType")]
        
        public string AttributeControlTypeName { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }


        public IList<AddressAttributeLocalizedModel> Locales { get; set; }

    }

    public partial class AddressAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Fields.Name")]
        
        public string Name { get; set; }

    }
}