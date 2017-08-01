using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Catalog;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    [Validator(typeof(SpecificationAttributeOptionValidator))]
    public partial class SpecificationAttributeOptionModel : BaseGrandEntityModel, ILocalizedModel<SpecificationAttributeOptionLocalizedModel>
    {
        public SpecificationAttributeOptionModel()
        {
            Locales = new List<SpecificationAttributeOptionLocalizedModel>();
        }

        public string SpecificationAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.ColorSquaresRgb")]
        
        public string ColorSquaresRgb { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.EnableColorSquaresRgb")]
        public bool EnableColorSquaresRgb { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.NumberOfAssociatedProducts")]
        public int NumberOfAssociatedProducts { get; set; }
        
        public IList<SpecificationAttributeOptionLocalizedModel> Locales { get; set; }

    }

    public partial class SpecificationAttributeOptionLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name")]
        
        public string Name { get; set; }
    }
}