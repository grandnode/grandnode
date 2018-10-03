using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Catalog;
using Grand.Framework.Localization;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    [Validator(typeof(ProductTagValidator))]
    public partial class ProductTagModel : BaseGrandEntityModel, ILocalizedModel<ProductTagLocalizedModel>
    {
        public ProductTagModel()
        {
            Locales = new List<ProductTagLocalizedModel>();
        }
        [GrandResourceDisplayName("Admin.Catalog.ProductTags.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.ProductTags.Fields.ProductCount")]
        public int ProductCount { get; set; }

        public IList<ProductTagLocalizedModel> Locales { get; set; }
    }

    public partial class ProductTagLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.ProductTags.Fields.Name")]
        
        public string Name { get; set; }
    }
}