using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Common;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.ShoppingCart
{
    public class GetShoppingCartHandler : IRequestHandler<GetShoppingCart, ShoppingCartModel>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IDiscountService _discountService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly ICountryService _countryService;
        private readonly IShippingService _shippingService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IVendorService _vendorService;
        private readonly IMediator _mediator;

        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly CommonSettings _commonSettings;

        public GetShoppingCartHandler(
            ICacheManager cacheManager,
            IPaymentService paymentService,
            IProductService productService,
            IPictureService pictureService,
            IProductAttributeParser productAttributeParser,
            ILocalizationService localizationService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IOrderProcessingService orderProcessingService,
            ICurrencyService currencyService,
            IDiscountService discountService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeService checkoutAttributeService,
            IPermissionService permissionService,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDownloadService downloadService,
            ICountryService countryService,
            IShippingService shippingService,
            IProductAttributeFormatter productAttributeFormatter,
            IPriceCalculationService priceCalculationService,
            IDateTimeHelper dateTimeHelper,
            IVendorService vendorService,
            IMediator mediator,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            ShippingSettings shippingSettings,
            CommonSettings commonSettings)
        {
            _cacheManager = cacheManager;
            _paymentService = paymentService;
            _productService = productService;
            _pictureService = pictureService;
            _productAttributeParser = productAttributeParser;
            _localizationService = localizationService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _orderProcessingService = orderProcessingService;
            _currencyService = currencyService;
            _discountService = discountService;
            _shoppingCartService = shoppingCartService;
            _checkoutAttributeService = checkoutAttributeService;
            _permissionService = permissionService;
            _taxService = taxService;
            _priceFormatter = priceFormatter;
            _checkoutAttributeParser = checkoutAttributeParser;
            _downloadService = downloadService;
            _countryService = countryService;
            _shippingService = shippingService;
            _productAttributeFormatter = productAttributeFormatter;
            _priceCalculationService = priceCalculationService;
            _dateTimeHelper = dateTimeHelper;
            _vendorService = vendorService;
            _mediator = mediator;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
            _shippingSettings = shippingSettings;
            _commonSettings = commonSettings;
        }

        public async Task<ShoppingCartModel> Handle(GetShoppingCart request, CancellationToken cancellationToken)
        {
            var model = new ShoppingCartModel();

            model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

            if (!request.Cart.Any())
                return model;

            await PrepareSimpleProperties(model, request);

            await PrepareCheckoutAttributes(model, request);

            await PrepareCartItems(model, request);

            await PrepareButtonPayment(model, request);

            await PrepareOrderReview(model, request);

            return model;
        }

        private async Task PrepareSimpleProperties(ShoppingCartModel model, GetShoppingCart request)
        {
            #region Simple properties

            model.IsEditable = request.IsEditable;
            model.IsAllowOnHold = _shoppingCartSettings.AllowOnHoldCart;
            model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            model.IsGuest = request.Customer.IsGuest();
            model.ShowCheckoutAsGuestButton = model.IsGuest && _orderSettings.AnonymousCheckoutAllowed;
            var checkoutAttributesXml = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, request.Store.Id);
            model.CheckoutAttributeInfo = await _checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, request.Customer);
            if (!request.Cart.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart || x.ShoppingCartType == ShoppingCartType.Auctions).ToList().Any())
            {
                model.MinOrderSubtotalWarning = _localizationService.GetResource("Checkout.MinOrderOneProduct");
            }
            else
            {
                bool minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmount(request.Cart.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart || x.ShoppingCartType == ShoppingCartType.Auctions).ToList());
                if (!minOrderSubtotalAmountOk)
                {
                    decimal minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, request.Currency);
                    model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
                }
            }
            model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

            //gift card and gift card boxes
            model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCodes = request.Customer.ParseAppliedCouponCodes(SystemCustomerAttributeNames.DiscountCoupons);
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = await _discountService.GetDiscountByCouponCode(couponCode);
                if (discount != null &&
                    discount.RequiresCouponCode &&
                    (await _discountService.ValidateDiscount(discount, request.Customer)).IsValid)
                {
                    model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel() {
                        Id = discount.Id,
                        CouponCode = couponCode
                    });
                }
            }

            model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = await _shoppingCartService.GetShoppingCartWarnings(request.Cart, checkoutAttributesXml, request.ValidateCheckoutAttributes);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion
        }

        private async Task PrepareCheckoutAttributes(ShoppingCartModel model, GetShoppingCart request)
        {
            #region Checkout attributes

            var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributes(request.Store.Id, !request.Cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                var attributeModel = new ShoppingCartModel.CheckoutAttributeModel {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, request.Language.Id),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt, request.Language.Id),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = attribute.DefaultValue
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.CheckoutAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ShoppingCartModel.CheckoutAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, request.Language.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
                        };
                        attributeModel.Values.Add(attributeValueModel);

                        //display price if allowed
                        if (await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal priceAdjustmentBase = (await _taxService.GetCheckoutAttributePrice(attribute, attributeValue)).checkoutPrice;
                            decimal priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, request.Currency);
                            if (priceAdjustmentBase > decimal.Zero)
                                attributeValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                            else if (priceAdjustmentBase < decimal.Zero)
                                attributeValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                        }
                    }
                }
                //set already selected attributes
                var selectedCheckoutAttributes = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, request.Store.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = await _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
                                foreach (var attributeValue in selectedValues)
                                    if (attributeModel.Id == attributeValue.CheckoutAttributeId)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                            if (selectedDateStr.Any())
                            {
                                DateTime selectedDate;
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate))
                                {
                                    //successfully parsed
                                    attributeModel.SelectedDay = selectedDate.Day;
                                    attributeModel.SelectedMonth = selectedDate.Month;
                                    attributeModel.SelectedYear = selectedDate.Year;
                                }
                            }

                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
                            {
                                var downloadGuidStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id).FirstOrDefault();
                                Guid downloadGuid;
                                Guid.TryParse(downloadGuidStr, out downloadGuid);
                                var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                    attributeModel.DefaultValue = download.DownloadGuid.ToString();
                            }
                        }
                        break;
                    default:
                        break;
                }

                model.CheckoutAttributes.Add(attributeModel);
            }
            #endregion 
        }

        private async Task PrepareCartItems(ShoppingCartModel model, GetShoppingCart request)
        {
            #region Cart items

            foreach (var sci in request.Cart)
            {
                var product = await _productService.GetProductById(sci.ProductId);
                if (product == null)
                    continue;

                var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel {
                    Id = sci.Id,
                    Sku = product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    IsCart = sci.ShoppingCartType == ShoppingCartType.ShoppingCart,
                    ProductId = product.Id,
                    WarehouseId = sci.WarehouseId,
                    ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                    ProductSeName = product.GetSeName(request.Language.Id),
                    Quantity = sci.Quantity,
                    AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml),
                };

                //allow editing?
                //1. setting enabled?
                //2. simple product?
                //3. has attribute or gift card?
                //4. visible individually?
                cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                    product.ProductType == ProductType.SimpleProduct &&
                    (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) || product.IsGiftCard) &&
                    product.VisibleIndividually;

                //disable removal?
                //1. do other items require this one?
                if (product.RequireOtherProducts)
                    cartItemModel.DisableRemoval = product.RequireOtherProducts && product.ParseRequiredProductIds().Intersect(request.Cart.Select(x => x.ProductId)).Any();

                //warehouse
                if (!string.IsNullOrEmpty(cartItemModel.WarehouseId))
                    cartItemModel.WarehouseName = (await _shippingService.GetWarehouseById(cartItemModel.WarehouseId))?.Name;

                //vendor
                if (!string.IsNullOrEmpty(product.VendorId))
                {
                    var vendor = await _vendorService.GetVendorById(product.VendorId);
                    if (vendor != null)
                    {
                        cartItemModel.VendorId = product.VendorId;
                        cartItemModel.VendorName = vendor.Name;
                        cartItemModel.VendorSeName = vendor.GetSeName(request.Language.Id);
                    }
                }
                //allowed quantities
                var allowedQuantities = product.ParseAllowedQuantities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new SelectListItem {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }

                //recurring info
                if (product.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, request.Language.Id));

                //reservation info
                if (product.ProductType == ProductType.Reservation)
                {
                    if (sci.RentalEndDateUtc == default(DateTime) || sci.RentalEndDateUtc == null)
                    {
                        cartItemModel.ReservationInfo = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.StartDate"), sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));
                    }
                    else
                    {
                        cartItemModel.ReservationInfo = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Date"), sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat), sci.RentalEndDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));
                    }

                    if (!string.IsNullOrEmpty(sci.Parameter))
                    {
                        cartItemModel.ReservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Option"), sci.Parameter);
                        cartItemModel.Parameter = sci.Parameter;
                    }
                    if (!string.IsNullOrEmpty(sci.Duration))
                    {
                        cartItemModel.ReservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Duration"), sci.Duration);
                    }
                }
                if (sci.ShoppingCartType == ShoppingCartType.Auctions)
                {
                    cartItemModel.DisableRemoval = true;
                    cartItemModel.AuctionInfo = _localizationService.GetResource("ShoppingCart.auctionwonon") + " " + _dateTimeHelper.ConvertToUserTime(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc).ToString();
                }

                //unit prices
                if (product.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                    cartItemModel.UnitPriceWithoutDiscount = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    var unitprices = await _priceCalculationService.GetUnitPrice(sci, true);
                    decimal discountAmount = unitprices.discountAmount;
                    List<AppliedDiscount> appliedDiscounts = unitprices.appliedDiscounts;
                    var productprices = await _taxService.GetProductPrice(product, unitprices.unitprice);
                    decimal shoppingCartUnitPriceWithDiscountBase = productprices.productprice;
                    decimal taxRate = productprices.taxRate;
                    decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, request.Currency);

                    cartItemModel.UnitPriceWithoutDiscountValue =
                         await _currencyService.ConvertFromPrimaryStoreCurrency(
                        (await _taxService.GetProductPrice(product,
                        (await _priceCalculationService.GetUnitPrice(sci, false)).unitprice)).productprice,
                        request.Currency);

                    cartItemModel.UnitPriceWithoutDiscount = _priceFormatter.FormatPrice(cartItemModel.UnitPriceWithoutDiscountValue);
                    cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                    if (appliedDiscounts != null && appliedDiscounts.Any())
                    {
                        var discount = await _discountService.GetDiscountById(appliedDiscounts.FirstOrDefault().DiscountId);
                        if (discount != null && discount.MaximumDiscountedQuantity.HasValue)
                            cartItemModel.DiscountedQty = discount.MaximumDiscountedQuantity.Value;

                        foreach (var disc in appliedDiscounts)
                        {
                            cartItemModel.Discounts.Add(disc.DiscountId);
                        }
                    }
                    //sub total
                    var subtotal = await _priceCalculationService.GetSubTotal(sci, true);
                    decimal shoppingCartItemDiscountBase = subtotal.discountAmount;
                    List<AppliedDiscount> scDiscounts = subtotal.appliedDiscounts;
                    decimal shoppingCartItemSubTotalWithDiscountBase = (await _taxService.GetProductPrice(product, subtotal.subTotal)).productprice;
                    decimal shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, request.Currency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
                    cartItemModel.SubTotalValue = shoppingCartItemSubTotalWithDiscount;

                    //display an applied discount amount
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        shoppingCartItemDiscountBase = (await _taxService.GetProductPrice(product, shoppingCartItemDiscountBase)).productprice;
                        if (shoppingCartItemDiscountBase > decimal.Zero)
                        {
                            decimal shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, request.Currency);
                            cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                        }
                    }
                }
                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    cartItemModel.Picture = await PrepareCartItemPicture(product, sci.AttributesXml, request.Language.Id, request.Store.Id);
                }

                //item warnings
                var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarnings(request.Customer, sci, product, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);

                model.Items.Add(cartItemModel);
            }

            #endregion

        }

        private async Task PrepareButtonPayment(ShoppingCartModel model, GetShoppingCart request)
        {
            #region Button payment methods

            var paymentMethods = (await _paymentService
                .LoadActivePaymentMethods(request.Customer, request.Store.Id))
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            var availablepaymentMethods = new List<IPaymentMethod>();
            foreach (var pm in paymentMethods)
            {
                if (!await pm.HidePaymentMethod(request.Cart))
                    availablepaymentMethods.Add(pm);
            }
            foreach (var pm in availablepaymentMethods)
            {
                if (request.Cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                pm.GetPublicViewComponent(out string viewComponentName);
                model.ButtonPaymentMethodViewComponentNames.Add(viewComponentName);

            }

            #endregion

        }

        private async Task PrepareOrderReview(ShoppingCartModel model, GetShoppingCart request)
        {
            #region Order review data

            if (request.PrepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData.Display = true;

                //billing info
                var billingAddress = request.Customer.BillingAddress;
                if (billingAddress != null)
                    model.OrderReviewData.BillingAddress = await _mediator.Send(new GetAddressModel() {
                        Language = request.Language,
                        Address = billingAddress,
                        ExcludeProperties = false,
                    });
                //shipping info
                if (request.Cart.RequiresShipping())
                {
                    model.OrderReviewData.IsShippable = true;

                    var pickupPoint = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.SelectedPickupPoint, request.Store.Id);

                    model.OrderReviewData.SelectedPickUpInStore = _shippingSettings.AllowPickUpInStore && !String.IsNullOrEmpty(pickupPoint);

                    if (!model.OrderReviewData.SelectedPickUpInStore)
                    {
                        var shippingAddress = request.Customer.ShippingAddress;
                        if (shippingAddress != null)
                            model.OrderReviewData.ShippingAddress = await _mediator.Send(new GetAddressModel() {
                                Language = request.Language,
                                Address = shippingAddress,
                                ExcludeProperties = false,
                            });
                    }
                    else
                    {
                        var pickup = await _shippingService.GetPickupPointById(pickupPoint);
                        if (pickup != null)
                        {
                            var country = await _countryService.GetCountryById(pickup.Address.CountryId);
                            model.OrderReviewData.PickupAddress = new AddressModel {
                                Address1 = pickup.Address.Address1,
                                City = pickup.Address.City,
                                CountryName = country != null ? country.Name : string.Empty,
                                ZipPostalCode = pickup.Address.ZipPostalCode
                            };
                        }
                    }
                    //selected shipping method
                    var shippingOption = request.Customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, request.Store.Id);
                    if (shippingOption != null)
                    {
                        model.OrderReviewData.ShippingMethod = shippingOption.Name;
                        model.OrderReviewData.ShippingAdditionDescription = request.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ShippingOptionAttributeDescription, request.Store.Id);
                    }
                }
                //payment info
                var selectedPaymentMethodSystemName = request.Customer.GetAttributeFromEntity<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, request.Store.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                model.OrderReviewData.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, request.Language.Id) : "";

            }
            #endregion


        }

        private async Task<PictureModel> PrepareCartItemPicture(Product product, string attributesXml, string langId, string storeId)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConst.CART_PICTURE_MODEL_KEY, product.Id, _mediaSettings.CartThumbPictureSize, true, langId, storeId);
            var model = await _cacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var sciPicture = await product.GetProductPicture(attributesXml, _productService, _pictureService, _productAttributeParser);
                return new PictureModel {
                    Id = sciPicture?.Id,
                    ImageUrl = await _pictureService.GetPictureUrl(sciPicture, _mediaSettings.CartThumbPictureSize, true),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), product.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), product.Name),
                };
            });

            return model;
        }

    }
}
