﻿using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Orders;
using Grand.Web.Framework;
using Grand.Web.Framework.Localization;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Orders
{
    [Validator(typeof(CheckoutAttributeValueValidator))]
    public partial class CheckoutAttributeValueModel : BaseNopEntityModel, ILocalizedModel<CheckoutAttributeValueLocalizedModel>
    {
        public CheckoutAttributeValueModel()
        {
            Locales = new List<CheckoutAttributeValueLocalizedModel>();
        }

        public string CheckoutAttributeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.ColorSquaresRgb")]
        [AllowHtml]
        public string ColorSquaresRgb { get; set; }
        public bool DisplayColorSquaresRgb { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.PriceAdjustment")]
        public decimal PriceAdjustment { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.WeightAdjustment")]
        public decimal WeightAdjustment { get; set; }
        public string BaseWeightIn { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}

        public IList<CheckoutAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class CheckoutAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}