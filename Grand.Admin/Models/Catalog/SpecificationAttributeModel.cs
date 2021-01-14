using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Mapping;

namespace Grand.Admin.Models.Catalog
{
    public partial class SpecificationAttributeModel : BaseEntityModel, ILocalizedModel<SpecificationAttributeLocalizedModel>, IStoreMappingModel
    {
        public SpecificationAttributeModel()
        {
            Locales = new List<SpecificationAttributeLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.SeName")]
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        public IList<SpecificationAttributeLocalizedModel> Locales { get; set; }

    }

    public partial class SpecificationAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        public string Name { get; set; }
    }
}