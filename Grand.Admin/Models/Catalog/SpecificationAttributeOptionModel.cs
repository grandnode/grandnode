using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Catalog
{
    public partial class SpecificationAttributeOptionModel : BaseEntityModel, ILocalizedModel<SpecificationAttributeOptionLocalizedModel>
    {
        public SpecificationAttributeOptionModel()
        {
            Locales = new List<SpecificationAttributeOptionLocalizedModel>();
        }

        public string SpecificationAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.SeName")]
        public string SeName { get; set; }

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