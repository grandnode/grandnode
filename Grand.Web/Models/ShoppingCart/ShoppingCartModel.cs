﻿using Grand.Domain.Catalog;
using Grand.Core.Models;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Models.ShoppingCart
{
    public partial class ShoppingCartModel : BaseModel
    {
        public ShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            DiscountBox = new DiscountBoxModel();
            GiftCardBox = new GiftCardBoxModel();
            CheckoutAttributes = new List<CheckoutAttributeModel>();
            OrderReviewData = new OrderReviewDataModel();
            ButtonPaymentMethodViewComponentNames = new List<string>();
        }

        public bool OnePageCheckoutEnabled { get; set; }

        public bool ShowSku { get; set; }
        public bool ShowProductImages { get; set; }
        public bool IsEditable { get; set; }
        public bool IsAllowOnHold { get; set; }
        public bool TermsOfServicePopup { get; set; }
        public IList<ShoppingCartItemModel> Items { get; set; }

        public string CheckoutAttributeInfo { get; set; }
        public IList<CheckoutAttributeModel> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }
        public string MinOrderSubtotalWarning { get; set; }
        public bool ShowCheckoutAsGuestButton { get; set; }
        public bool IsGuest { get; set; }
        public bool DisplayTaxShippingInfo { get; set; }
        public bool TermsOfServiceOnShoppingCartPage { get; set; }
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public DiscountBoxModel DiscountBox { get; set; }
        public GiftCardBoxModel GiftCardBox { get; set; }
        public OrderReviewDataModel OrderReviewData { get; set; }

        public IList<string> ButtonPaymentMethodViewComponentNames { get; set; }

        #region Nested Classes

        public partial class ShoppingCartItemModel : BaseEntityModel
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                AllowedQuantities = new List<SelectListItem>();
                Discounts = new List<string>();
                Warnings = new List<string>();
            }
            public string Sku { get; set; }
            public bool IsCart { get; set; }
            public PictureModel Picture {get;set;}
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string VendorId { get; set; }
            public string VendorName { get; set; }
            public string VendorSeName { get; set; }
            public string UnitPriceWithoutDiscount { get; set; }
            public decimal UnitPriceWithoutDiscountValue { get; set; }
            public string UnitPrice { get; set; }
            public decimal UnitPriceValue { get; set; }
            public string SubTotal { get; set; }
            public decimal SubTotalValue { get; set; }
            public string Discount { get; set; }
            public int DiscountedQty { get; set; }
            public List<string> Discounts { get; set; }
            public int Quantity { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }
            public string AttributeInfo { get; set; }
            public string RecurringInfo { get; set; }
            public bool AllowItemEditing { get; set; }
            public bool DisableRemoval { get; set; }
            public string ReservationInfo { get; set; }
            public string AuctionInfo { get; set; }
            public string Parameter { get; set; }
            public IList<string> Warnings { get; set; }
        }

        public partial class CheckoutAttributeModel : BaseEntityModel
        {
            public CheckoutAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<CheckoutAttributeValueModel>();
            }

            public string Name { get; set; }

            public string DefaultValue { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            /// Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<CheckoutAttributeValueModel> Values { get; set; }
        }

        public partial class CheckoutAttributeValueModel : BaseEntityModel
        {
            public string Name { get; set; }

            public string ColorSquaresRgb { get; set; }

            public string PriceAdjustment { get; set; }

            public bool IsPreSelected { get; set; }
        }

        public partial class DiscountBoxModel: BaseModel
        {
            public DiscountBoxModel()
            {
                AppliedDiscountsWithCodes = new List<DiscountInfoModel>();
            }
            public List<DiscountInfoModel> AppliedDiscountsWithCodes { get; set; }
            public bool Display { get; set; }
            public string Message { get; set; }
            public bool IsApplied { get; set; }
            public class DiscountInfoModel : BaseEntityModel
            {
                public string CouponCode { get; set; }
            }
        }

        public partial class GiftCardBoxModel : BaseModel
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public bool IsApplied { get; set; }
        }

        public partial class OrderReviewDataModel : BaseModel
        {
            public OrderReviewDataModel()
            {
                BillingAddress = new AddressModel();
                ShippingAddress = new AddressModel();
                PickupAddress = new AddressModel();
            }
            public bool Display { get; set; }

            public AddressModel BillingAddress { get; set; }

            public bool IsShippable { get; set; }
            public AddressModel ShippingAddress { get; set; }
            public bool SelectedPickUpInStore { get; set; }
            public AddressModel PickupAddress { get; set; }
            public string ShippingMethod { get; set; }
            public string ShippingAdditionDescription { get; set; }

            public string PaymentMethod { get; set; }

        }
		#endregion
    }
}