using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Shipping;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Shipping
{
    [Validator(typeof(ShippingMethodValidator))]
    public partial class ShippingMethodModel : BaseGrandEntityModel, ILocalizedModel<ShippingMethodLocalizedModel>
    {
        public ShippingMethodModel()
        {
            Locales = new List<ShippingMethodLocalizedModel>();
        }
        [GrandResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Description")]
        
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<ShippingMethodLocalizedModel> Locales { get; set; }
    }

    public partial class ShippingMethodLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Methods.Fields.Description")]
        
        public string Description { get; set; }

    }
}