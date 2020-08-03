using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductAttributeModel : BaseGrandEntityModel, ILocalizedModel<ProductAttributeLocalizedModel>
    {
        public ProductAttributeModel()
        {
            Locales = new List<ProductAttributeLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.SeName")]
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Description")]
        public string Description { get; set; }



        public IList<ProductAttributeLocalizedModel> Locales { get; set; }

        #region Nested classes

        public partial class UsedByProductModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.UsedByProducts.Product")]
            public string ProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.UsedByProducts.Published")]
            public bool Published { get; set; }
        }

        #endregion
    }

    public partial class ProductAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Description")]

        public string Description { get; set; }
    }

    public partial class PredefinedProductAttributeValueModel : BaseGrandEntityModel, ILocalizedModel<PredefinedProductAttributeValueLocalizedModel>
    {
        public PredefinedProductAttributeValueModel()
        {
            Locales = new List<PredefinedProductAttributeValueLocalizedModel>();
        }

        public string ProductAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.PriceAdjustment")]
        public decimal PriceAdjustment { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.PriceAdjustment")]
        //used only on the values list page
        public string PriceAdjustmentStr { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.WeightAdjustment")]
        public decimal WeightAdjustment { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.WeightAdjustment")]
        //used only on the values list page
        public string WeightAdjustmentStr { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.Cost")]
        public decimal Cost { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<PredefinedProductAttributeValueLocalizedModel> Locales { get; set; }
    }
    public partial class PredefinedProductAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.Name")]

        public string Name { get; set; }
    }
}