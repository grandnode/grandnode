using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class CheckoutAttributeValueModel : BaseGrandEntityModel, ILocalizedModel<CheckoutAttributeValueLocalizedModel>
    {
        public CheckoutAttributeValueModel()
        {
            Locales = new List<CheckoutAttributeValueLocalizedModel>();
        }

        public string CheckoutAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.ColorSquaresRgb")]

        public string ColorSquaresRgb { get; set; }
        public bool DisplayColorSquaresRgb { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.PriceAdjustment")]
        public decimal PriceAdjustment { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.WeightAdjustment")]
        public decimal WeightAdjustment { get; set; }
        public string BaseWeightIn { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<CheckoutAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class CheckoutAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name")]

        public string Name { get; set; }
    }
}