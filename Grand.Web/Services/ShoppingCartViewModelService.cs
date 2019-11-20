using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Core.Http;
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
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class ShoppingCartViewModelService : IShoppingCartViewModelService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
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
        private readonly IStoreContext _storeContext;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IShippingService _shippingService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAuctionService _auctionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDateTimeHelper _dateTimeHelper;

        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CommonSettings _commonSettings;

        public ShoppingCartViewModelService(
            ICacheManager cacheManager,
            IWorkContext workContext,
            IWebHelper webHelper,
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
            IStoreContext storeContext,
            ICheckoutAttributeService checkoutAttributeService,
            IPermissionService permissionService,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDownloadService downloadService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressViewModelService addressViewModelService,
            IShippingService shippingService,
            IProductAttributeFormatter productAttributeFormatter,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IGenericAttributeService genericAttributeService,
            IAuctionService auctionService,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeHelper dateTimeHelper,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            CommonSettings commonSettings)
        {
            _cacheManager = cacheManager;
            _workContext = workContext;
            _webHelper = webHelper;
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
            _storeContext = storeContext;
            _checkoutAttributeService = checkoutAttributeService;
            _permissionService = permissionService;
            _taxService = taxService;
            _priceFormatter = priceFormatter;
            _checkoutAttributeParser = checkoutAttributeParser;
            _downloadService = downloadService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressViewModelService = addressViewModelService;
            _shippingService = shippingService;
            _productAttributeFormatter = productAttributeFormatter;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceCalculationService = priceCalculationService;
            _genericAttributeService = genericAttributeService;
            _auctionService = auctionService;
            _httpContextAccessor = httpContextAccessor;
            _dateTimeHelper = dateTimeHelper;

            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _commonSettings = commonSettings;
        }

        public virtual async Task<PictureModel> PrepareCartItemPicture(Product product, string attributesXml,
            int pictureSize, bool showDefaultPicture, string productName)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConsumer.CART_PICTURE_MODEL_KEY, product.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            var model = await _cacheManager.GetAsync(pictureCacheKey, async () =>
                {
                    var sciPicture = await product.GetProductPicture(attributesXml, _productService, _pictureService, _productAttributeParser);
                    return new PictureModel
                    {
                        Id = sciPicture?.Id,
                        ImageUrl = await _pictureService.GetPictureUrl(sciPicture, pictureSize, showDefaultPicture),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), productName),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), productName),
                    };
                });

            return model;
        }

        public virtual async Task PrepareShoppingCart(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

            if (!cart.Any())
                return;

            var customer = _workContext.CurrentCustomer;

            #region Simple properties

            model.IsEditable = isEditable;
            model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            model.IsGuest = customer.IsGuest();
            model.ShowCheckoutAsGuestButton = model.IsGuest && _orderSettings.AnonymousCheckoutAllowed;
            var checkoutAttributesXml = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
            model.CheckoutAttributeInfo = await _checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, customer);
            bool minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                decimal minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
            }
            model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

            //gift card and gift card boxes
            model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCodes = await customer.ParseAppliedDiscountCouponCodes(_genericAttributeService);
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = await _discountService.GetDiscountByCouponCode(couponCode);
                if (discount != null &&
                    discount.RequiresCouponCode &&
                    (await _discountService.ValidateDiscount(discount, customer)).IsValid)
                {
                    model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel()
                    {
                        Id = discount.Id,
                        CouponCode = couponCode
                    });
                }
            }

            model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = await _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Checkout attributes

            var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                var attributeModel = new ShoppingCartModel.CheckoutAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt, _workContext.WorkingLanguage.Id),
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
                        var attributeValueModel = new ShoppingCartModel.CheckoutAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
                        };
                        attributeModel.Values.Add(attributeValueModel);

                        //display price if allowed
                        if (await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal priceAdjustmentBase = (await _taxService.GetCheckoutAttributePrice(attributeValue)).checkoutPrice;
                            decimal priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                attributeValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
                            else if (priceAdjustmentBase < decimal.Zero)
                                attributeValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
                        }
                    }
                }



                //set already selected attributes
                var selectedCheckoutAttributes = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
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


            #region Cart items

            foreach (var sci in cart)
            {
                var product = await _productService.GetProductById(sci.ProductId);
                var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
                {
                    Id = sci.Id,
                    Sku = product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    ProductId = product.Id,
                    WarehouseId = sci.WarehouseId,
                    ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
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
                    cartItemModel.DisableRemoval = product.RequireOtherProducts && product.ParseRequiredProductIds().Intersect(cart.Select(x => x.ProductId)).Any();

                //warehouse
                if (!string.IsNullOrEmpty(cartItemModel.WarehouseId))
                    cartItemModel.WarehouseName = (await _shippingService.GetWarehouseById(cartItemModel.WarehouseId))?.Name;

                //allowed quantities
                var allowedQuantities = product.ParseAllowedQuantities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new SelectListItem
                    {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }

                //recurring info
                if (product.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

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
                    decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);

                    cartItemModel.UnitPriceWithoutDiscountValue = 
                         await _currencyService.ConvertFromPrimaryStoreCurrency(
                        (await _taxService.GetProductPrice(product,
                        (await _priceCalculationService.GetUnitPrice(sci, false)).unitprice)).productprice,
                        _workContext.WorkingCurrency);

                    cartItemModel.UnitPriceWithoutDiscount = _priceFormatter.FormatPrice(cartItemModel.UnitPriceWithoutDiscountValue);
                    cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                    if (appliedDiscounts != null && appliedDiscounts.Any())
                    {
                        var discount = await _discountService.GetDiscountById(appliedDiscounts.FirstOrDefault().DiscountId);
                        if (discount != null && discount.MaximumDiscountedQuantity.HasValue)
                            cartItemModel.DiscountedQty =  discount.MaximumDiscountedQuantity.Value;

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
                    decimal shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        shoppingCartItemDiscountBase = (await _taxService.GetProductPrice(product, shoppingCartItemDiscountBase)).productprice;
                        if (shoppingCartItemDiscountBase > decimal.Zero)
                        {
                            decimal shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                            cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                        }
                    }
                }

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
                {
                    cartItemModel.Picture = await PrepareCartItemPicture(product, sci.AttributesXml,
                        _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
                }

                //item warnings
                var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarnings(customer, sci, product, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);

                model.Items.Add(cartItemModel);
            }

            #endregion

            #region Button payment methods

            var paymentMethods = (await _paymentService
                .LoadActivePaymentMethods(customer, _storeContext.CurrentStore.Id))
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            var availablepaymentMethods = new List<IPaymentMethod>();
            foreach (var pm in paymentMethods)
            {
                if (!await pm.HidePaymentMethod(cart))
                    availablepaymentMethods.Add(pm);
            }
            foreach (var pm in availablepaymentMethods)
            {
                if (cart.IsRecurring() && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                pm.GetPublicViewComponent(out string viewComponentName);
                model.ButtonPaymentMethodViewComponentNames.Add(viewComponentName);

            }

            #endregion

            #region Order review data

            if (prepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData.Display = true;

                //billing info
                var billingAddress = customer.BillingAddress;
                if (billingAddress != null)
                    await _addressViewModelService.PrepareModel(model: model.OrderReviewData.BillingAddress,
                        address: billingAddress,
                        excludeProperties: false);

                //shipping info
                if (cart.RequiresShipping())
                {
                    model.OrderReviewData.IsShippable = true;

                    var pickupPoint = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.SelectedPickupPoint, _storeContext.CurrentStore.Id);

                    model.OrderReviewData.SelectedPickUpInStore = _shippingSettings.AllowPickUpInStore && !String.IsNullOrEmpty(pickupPoint);

                    if (!model.OrderReviewData.SelectedPickUpInStore)
                    {
                        var shippingAddress = customer.ShippingAddress;
                        if (shippingAddress != null)
                            await _addressViewModelService.PrepareModel(model: model.OrderReviewData.ShippingAddress,
                                address: shippingAddress,
                                excludeProperties: false);
                    }
                    else
                    {
                        var pickup = await _shippingService.GetPickupPointById(pickupPoint);
                        if (pickup != null)
                        {
                            var country = await _countryService.GetCountryById(pickup.Address.CountryId);
                            model.OrderReviewData.PickupAddress = new AddressModel
                            {
                                Address1 = pickup.Address.Address1,
                                City = pickup.Address.City,
                                CountryName = country != null ? country.Name : string.Empty,
                                ZipPostalCode = pickup.Address.ZipPostalCode
                            };
                        }
                    }
                    //selected shipping method
                    var shippingOption = customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                    if (shippingOption != null)
                    {
                        model.OrderReviewData.ShippingMethod = shippingOption.Name;
                        model.OrderReviewData.ShippingAdditionDescription = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ShippingOptionAttributeDescription, _storeContext.CurrentStore.Id);
                    }
                }
                //payment info
                var selectedPaymentMethodSystemName = customer.GetAttributeFromEntity<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                model.OrderReviewData.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : "";

                //custom values
                var processPaymentRequest = _httpContextAccessor.HttpContext?.Session?.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest != null)
                {
                    model.OrderReviewData.CustomValues = processPaymentRequest.CustomValues;
                }
            }
            #endregion
        }

        public virtual async Task PrepareWishlist(WishlistModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
            model.IsEditable = isEditable;
            model.DisplayAddToCart = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoWishlist;

            if (!cart.Any())
                return;

            #region Simple properties

            var customer = _workContext.CurrentCustomer;
            model.CustomerGuid = customer.CustomerGuid;
            model.CustomerFullname = customer.GetFullName();
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnWishList;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;

            //cart warnings
            var cartWarnings = await _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Cart items

            foreach (var sci in cart)
            {
                var product = await _productService.GetProductById(sci.ProductId);
                var cartItemModel = new WishlistModel.ShoppingCartItemModel
                {
                    Id = sci.Id,
                    Sku = product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    ProductId = product.Id,
                    ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                    Quantity = sci.Quantity,
                    AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml),
                };

                //allow editing?
                //1. setting enabled?
                //2. simple product?
                //3. has attribute or gift card?
                //4. visible individually?
                cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing && product.ProductType == ProductType.SimpleProduct && (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) || product.IsGiftCard) && product.VisibleIndividually;

                //allowed quantities
                var allowedQuantities = product.ParseAllowedQuantities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new SelectListItem
                    {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }


                //recurring info
                if (product.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                //unit prices
                if (product.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    var productprice = await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice);
                    decimal taxRate = productprice.taxRate;
                    decimal shoppingCartUnitPriceWithDiscountBase = productprice.productprice;
                    decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                }
                //subtotal, discount
                if (product.CallForPrice)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    var subtotal = await _priceCalculationService.GetSubTotal(sci, true);
                    decimal shoppingCartItemDiscountBase = subtotal.discountAmount;
                    List<AppliedDiscount> scDiscounts = subtotal.appliedDiscounts;
                    var productprices = await _taxService.GetProductPrice(product, subtotal.subTotal);
                    decimal taxRate = productprices.taxRate;
                    decimal shoppingCartItemSubTotalWithDiscountBase = productprices.productprice;

                    decimal shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        shoppingCartItemDiscountBase = (await _taxService.GetProductPrice(product, shoppingCartItemDiscountBase)).productprice;
                        if (shoppingCartItemDiscountBase > decimal.Zero)
                        {
                            decimal shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                            cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                        }
                    }
                }

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnWishList)
                {
                    cartItemModel.Picture = await PrepareCartItemPicture(product, sci.AttributesXml,
                        _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
                }

                //item warnings
                var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarnings(customer, sci, product, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);

                model.Items.Add(cartItemModel);
            }

            #endregion
        }

        public virtual async Task<EstimateShippingModel> PrepareEstimateShipping(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true)
        {
            var model = new EstimateShippingModel();
            model.Enabled = cart.Any() && cart.RequiresShipping() && _shippingSettings.EstimateShippingEnabled;
            if (model.Enabled)
            {
                var customer = _workContext.CurrentCustomer;
                var languageId = _workContext.WorkingLanguage.Id;
                //countries
                var defaultEstimateCountryId = (setEstimateShippingDefaultAddress && customer.ShippingAddress != null) ? customer.ShippingAddress.CountryId : model.CountryId;
                if (string.IsNullOrEmpty(defaultEstimateCountryId))
                    defaultEstimateCountryId = _storeContext.CurrentStore.DefaultCountryId;

                model.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "" });
                foreach (var c in await _countryService.GetAllCountriesForShipping(languageId))
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        Value = c.Id.ToString(),
                        Selected = c.Id == defaultEstimateCountryId
                    });
                //states
                string defaultEstimateStateId = (setEstimateShippingDefaultAddress && customer.ShippingAddress != null) ? customer.ShippingAddress.StateProvinceId : model.StateProvinceId;
                var states = !String.IsNullOrEmpty(defaultEstimateCountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId, languageId) : new List<StateProvince>();
                if (states.Any())
                    foreach (var s in states)
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            Value = s.Id.ToString(),
                            Selected = s.Id == defaultEstimateStateId
                        });
                else
                    model.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "" });

                if (setEstimateShippingDefaultAddress && customer.ShippingAddress != null)
                    model.ZipPostalCode = customer.ShippingAddress.ZipPostalCode;
            }
            return model;
        }

        public virtual async Task<MiniShoppingCartModel> PrepareMiniShoppingCart()
        {
            var customer = _workContext.CurrentCustomer;
            var storeId = _storeContext.CurrentStore.Id;
            var currency = _workContext.WorkingCurrency;

            var model = new MiniShoppingCartModel
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = customer.IsGuest(),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
            };

            if (customer.ShoppingCartItems.Any())
            {
                var cart = _shoppingCartService.GetShoppingCart(storeId, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
                model.TotalProducts = cart.Sum(x => x.Quantity);
                if (cart.Any())
                {
                    //subtotal
                    var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax);
                    decimal orderSubTotalDiscountAmountBase = shoppingCartSubTotal.discountAmount;
                    List<AppliedDiscount> orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
                    decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
                    decimal subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;
                    decimal subtotalBase = subTotalWithoutDiscountBase;
                    decimal subtotal = await _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, currency);
                    model.SubTotal = _priceFormatter.FormatPrice(subtotal, false, currency, _workContext.WorkingLanguage, subTotalIncludingTax);

                    var requiresShipping = cart.RequiresShipping();
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExistCacheKey = string.Format(ModelCacheEventConsumer.CHECKOUTATTRIBUTES_EXIST_KEY,
                        storeId, requiresShipping);
                    var checkoutAttributesExist = await _cacheManager.GetAsync(checkoutAttributesExistCacheKey,
                        async () =>
                        {
                            var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributes(storeId, !requiresShipping);
                            return checkoutAttributes.Any();
                        });

                    bool minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist;

                    //products. sort descending (recently added products)
                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id)
                        .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                        .ToList())
                    {
                        var product = await _productService.GetProductById(sci.ProductId);
                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                        {
                            Id = sci.Id,
                            ProductId = product.Id,
                            ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                            Quantity = sci.Quantity,
                            AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml)
                        };
                        if (product.ProductType == ProductType.Reservation)
                        {
                            var reservation = "";
                            if (sci.RentalEndDateUtc == default(DateTime) || sci.RentalEndDateUtc == null)
                            {
                                reservation = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.StartDate"), sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));
                            }
                            else
                            {
                                reservation = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Date"), sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat), sci.RentalEndDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));
                            }

                            if (!string.IsNullOrEmpty(sci.Parameter))
                            {
                                reservation += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Option"), sci.Parameter);
                            }
                            if (!string.IsNullOrEmpty(sci.Duration))
                            {
                                reservation += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Duration"), sci.Duration);
                            }
                            if (string.IsNullOrEmpty(cartItemModel.AttributeInfo))
                                cartItemModel.AttributeInfo = reservation;
                            else
                                cartItemModel.AttributeInfo += "<br>" + reservation;
                        }
                        //unit prices
                        if (product.CallForPrice)
                        {
                            cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                        }
                        else
                        {
                            var productprices = await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice);
                            decimal taxRate = productprices.taxRate;
                            decimal shoppingCartUnitPriceWithDiscountBase = productprices.productprice;
                            decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, currency);
                            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                        }

                        //picture
                        if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                        {
                            cartItemModel.Picture = await PrepareCartItemPicture(product, sci.AttributesXml,
                                _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                        }

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        public virtual async Task<OrderTotalsModel> PrepareOrderTotals(IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new OrderTotalsModel();
            model.IsEditable = isEditable;

            if (cart.Any())
            {
                var customer = _workContext.CurrentCustomer;
                //subtotal
                var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax);
                decimal orderSubTotalDiscountAmountBase = shoppingCartSubTotal.discountAmount;
                List<AppliedDiscount> orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
                decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
                decimal subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;
                decimal subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = await _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);

                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
                }


                //shipping info
                model.RequiresShipping = cart.RequiresShipping();
                if (model.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = (await _orderTotalCalculationService.GetShoppingCartShippingTotal(cart)).shoppingCartShippingTotal;
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                        //selected shipping method
                        var shippingOption = customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }

                //payment method fee
                var paymentMethodSystemName = customer.GetAttributeFromEntity<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
                decimal paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = (await _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, customer)).paymentPrice;
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = await _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, _workContext.WorkingCurrency);
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
                }

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && _workContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    var taxtotal = await _orderTotalCalculationService.GetTaxTotal(cart);
                    SortedDictionary<decimal, decimal> taxRates = taxtotal.taxRates;
                    decimal shoppingCartTaxBase = taxtotal.taxtotal;
                    decimal shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                        displayTax = !displayTaxRates;

                        model.Tax = _priceFormatter.FormatPrice(shoppingCartTax, true, false);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                Value = _priceFormatter.FormatPrice(await _currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, _workContext.WorkingCurrency), true, false),
                            });
                        }
                    }
                }
                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                //total
                var carttotal = await _orderTotalCalculationService.GetShoppingCartTotal(cart);
                decimal? shoppingCartTotalBase = carttotal.shoppingCartTotal;
                decimal orderTotalDiscountAmountBase = carttotal.discountAmount;
                List<AppliedDiscount> orderTotalAppliedDiscounts = carttotal.appliedDiscounts;
                List<AppliedGiftCard> appliedGiftCards = carttotal.appliedGiftCards;
                int redeemedRewardPoints = carttotal.redeemedRewardPoints;
                decimal redeemedRewardPointsAmount = carttotal.redeemedRewardPointsAmount;
                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, true, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, false);
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Any())
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };
                        decimal amountCanBeUsed = await _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, true, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, _workContext.WorkingCurrency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false);

                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points to be spent (redeemed)
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = await _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, _workContext.WorkingCurrency);
                    model.RedeemedRewardPoints = redeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }

                //reward points to be earned
                if (_rewardPointsSettings.Enabled &&
                    _rewardPointsSettings.DisplayHowMuchWillBeEarned &&
                    shoppingCartTotalBase.HasValue)
                {
                    decimal? shippingBaseInclTax = model.RequiresShipping
                        ? (await _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, true)).shoppingCartShippingTotal
                        : 0;
                    var earnRewardPoints = shoppingCartTotalBase.Value - shippingBaseInclTax.Value;
                    if (earnRewardPoints > 0)
                        model.WillEarnRewardPoints = _orderTotalCalculationService
                            .CalculateRewardPoints(customer, earnRewardPoints);
                }

            }

            return model;
        }

        public virtual async Task<AddToCartModel> PrepareAddToCartModel(Product product, Customer customer, int quantity, decimal customerEnteredPrice, string attributesXml, ShoppingCartType cartType, DateTime? startDate, DateTime? endDate, string reservationId, string parameter, string duration)
        {
            var model = new AddToCartModel();
            model.AttributeDescription = await _productAttributeFormatter.FormatAttributes(product, attributesXml);
            model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
            model.CartType = cartType;
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.Quantity = quantity;

            //reservation info
            if (product.ProductType == ProductType.Reservation)
            {
                if (endDate == default(DateTime) || endDate == null)
                {
                    model.ReservationInfo = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.StartDate"), startDate?.ToString(_shoppingCartSettings.ReservationDateFormat));
                }
                else
                {
                    model.ReservationInfo = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Date"), startDate?.ToString(_shoppingCartSettings.ReservationDateFormat), endDate?.ToString(_shoppingCartSettings.ReservationDateFormat));
                }

                if (!string.IsNullOrEmpty(parameter))
                {
                    model.ReservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Option"), parameter);
                }
                if (!string.IsNullOrEmpty(duration))
                {
                    model.ReservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Duration"), duration);
                }
            }

            if (cartType != ShoppingCartType.Auctions)
            {
                var sci = customer.ShoppingCartItems.FirstOrDefault(x => x.ProductId == product.Id && (string.IsNullOrEmpty(x.AttributesXml) ? "" : x.AttributesXml) == attributesXml);
                model.ItemQuantity = sci.Quantity;

                //unit prices
                if (product.CallForPrice)
                {
                    model.Price = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    var productprices = await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice);
                    decimal taxRate = productprices.taxRate;
                    decimal shoppingCartUnitPriceWithDiscountBase = productprices.productprice;
                    decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
                    model.Price = customerEnteredPrice == 0 ? _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount) : _priceFormatter.FormatPrice(customerEnteredPrice);
                    model.DecimalPrice = customerEnteredPrice == 0 ? shoppingCartUnitPriceWithDiscount : customerEnteredPrice;
                    model.TotalPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount * sci.Quantity);
                }

                //picture
                model.Picture = await PrepareCartItemPicture(product, sci.AttributesXml, _mediaSettings.AddToCartThumbPictureSize, true, model.ProductName);
            }
            else
            {
                model.Picture = await PrepareCartItemPicture(product, null, _mediaSettings.AddToCartThumbPictureSize, true, model.ProductName);
            }

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, cartType);

            if (cartType != ShoppingCartType.Auctions)
            {
                model.TotalItems = cart.Sum(x => x.Quantity);
            }
            else
            {
                model.TotalItems = 0;
                var grouped = (await _auctionService.GetBidsByCustomerId(_workContext.CurrentCustomer.Id)).GroupBy(x => x.ProductId);
                foreach (var item in grouped)
                {
                    var p = await _productService.GetProductById(item.Key);
                    if (p != null && p.AvailableEndDateTimeUtc > DateTime.UtcNow)
                    {
                        model.TotalItems++;
                    }
                }
            }

            if (cartType == ShoppingCartType.ShoppingCart)
            {
                var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax);
                decimal orderSubTotalDiscountAmountBase = shoppingCartSubTotal.discountAmount;
                List<AppliedDiscount> orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
                decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
                decimal subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;
                decimal subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = await _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
                model.DecimalSubTotal = subtotal;
                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, subTotalIncludingTax);
                }
            }
            else if (cartType == ShoppingCartType.Auctions)
            {
                model.IsAuction = true;
                model.HighestBidValue = product.HighestBid;
                model.HighestBid = _priceFormatter.FormatPrice(product.HighestBid);
                model.EndTime = product.AvailableEndDateTimeUtc;
            }

            return model;

        }

        public virtual async Task ParseAndSaveCheckoutAttributes(IList<ShoppingCartItem> cart, IFormCollection form)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (form == null)
                throw new ArgumentNullException("form");

            string attributesXml = "";
            var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("checkout_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                        attribute, ctrlAttributes);

                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.CheckoutAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(form[controlId], out downloadGuid);
                            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //save checkout attributes
            //validate conditional attributes (if specified)
            foreach (var attribute in checkoutAttributes)
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _checkoutAttributeParser.RemoveCheckoutAttribute(attributesXml, attribute);
            }
            await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.CheckoutAttributes, attributesXml, _storeContext.CurrentStore.Id);
        }

        public virtual async Task<string> ParseProductAttributes(Product product, IFormCollection form)
        {
            string attributesXml = "";

            #region Product attributes
            var productAttributes = product.ProductAttributeMappings;
            foreach (var attribute in productAttributes)
            {
                string controlId = string.Format("product_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, ctrlAttributes);
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!String.IsNullOrEmpty(item))
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.ProductAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId].ToString();
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(form[controlId], out downloadGuid);
                            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(product, attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            #endregion

            #region Gift cards

            if (product.IsGiftCard)
            {
                string recipientName = "";
                string recipientEmail = "";
                string senderName = "";
                string senderEmail = "";
                string giftCardMessage = "";
                foreach (string formKey in form.Keys)
                {
                    if (formKey.Equals(string.Format("giftcard_{0}.RecipientName", product.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        recipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.RecipientEmail", product.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        recipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.SenderName", product.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.SenderEmail", product.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals(string.Format("giftcard_{0}.Message", product.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        giftCardMessage = form[formKey];
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                    recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            return attributesXml;
        }

        public virtual void ParseReservationDates(Product product, IFormCollection form,
            out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            string startControlId = string.Format("reservationDatepickerFrom_{0}", product.Id);
            string endControlId = string.Format("reservationDatepickerTo_{0}", product.Id);
            var ctrlStartDate = form[startControlId];
            var ctrlEndDate = form[endControlId];
            try
            {
                //currenly we support only this format (as in the \Views\Product\_RentalInfo.cshtml file)
                const string datePickerFormat = "MM/dd/yyyy";
                startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }

        public virtual async Task<EstimateShippingResultModel> PrepareEstimateShippingResult(IList<ShoppingCartItem> cart, string countryId, string stateProvinceId, string zipPostalCode)
        {
            var model = new EstimateShippingResultModel();

            if (cart.RequiresShipping())
            {
                var address = new Address
                {
                    CountryId = countryId,
                    StateProvinceId = stateProvinceId,
                    ZipPostalCode = zipPostalCode,
                };
                GetShippingOptionResponse getShippingOptionResponse = await _shippingService
                    .GetShippingOptions(_workContext.CurrentCustomer, cart, address, "", _storeContext.CurrentStore);
                if (!getShippingOptionResponse.Success)
                {
                    foreach (var error in getShippingOptionResponse.Errors)
                        model.Warnings.Add(error);
                }
                else
                {
                    if (getShippingOptionResponse.ShippingOptions.Any())
                    {
                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            var soModel = new EstimateShippingResultModel.ShippingOptionModel
                            {
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,

                            };

                            //calculate discounted and taxed rate
                            var total = await _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate, cart);
                            List<AppliedDiscount> appliedDiscounts = total.appliedDiscounts;
                            decimal shippingTotal = total.shippingRate;

                            decimal rateBase = (await _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer)).shippingPrice;
                            decimal rate = await _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                            soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);
                            model.ShippingOptions.Add(soModel);
                        }

                        //pickup in store?
                        if (_shippingSettings.AllowPickUpInStore)
                        {
                            var pickupPoints = await _shippingService.GetAllPickupPoints();
                            if (pickupPoints.Count > 0)
                            {
                                var soModel = new EstimateShippingResultModel.ShippingOptionModel
                                {
                                    Name = _localizationService.GetResource("Checkout.PickUpInStore"),
                                    Description = _localizationService.GetResource("Checkout.PickUpInStore.Description"),
                                };

                                decimal shippingTotal = pickupPoints.Max(x => x.PickupFee);
                                decimal rateBase = (await _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer)).shippingPrice;
                                decimal rate = await _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                                soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);
                                model.ShippingOptions.Add(soModel);
                            }
                        }
                    }
                    else
                    {
                        model.Warnings.Add(_localizationService.GetResource("Checkout.ShippingIsNotAllowed"));
                    }
                }
            }
            return model;
        }
    }
}
