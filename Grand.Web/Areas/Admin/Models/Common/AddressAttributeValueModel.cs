using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;

using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Common;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Common
{
    [Validator(typeof(AddressAttributeValueValidator))]
    public partial class AddressAttributeValueModel : BaseGrandEntityModel, ILocalizedModel<AddressAttributeValueLocalizedModel>
    {
        public AddressAttributeValueModel()
        {
            Locales = new List<AddressAttributeValueLocalizedModel>();
        }

        public string AddressAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}

        public IList<AddressAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class AddressAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.Name")]
        
        public string Name { get; set; }
    }
}