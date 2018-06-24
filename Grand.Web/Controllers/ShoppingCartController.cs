using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Web.Models.ShoppingCart;
using Grand.Web.Services;
using Grand.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using Grand.Framework.Mvc.Filters;
using System.Globalization;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Media;
using Grand.Core.Caching;

namespace Grand.Web.Controllers
{
    public partial class ShoppingCartController : BasePublicController
    {
		#region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAddressWebService _addressWebService;
        private readonly IShoppingCartWebService _shoppingCartWebService;
        private readonly IProductReservationService _productReservationService;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly IAuctionService _auctionService;
        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly AddressSettings _addressSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;

        #endregion

        #region Constructors

        public ShoppingCartController(IProductService productService, 
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService, 
            ILocalizationService localizationService, 
            IProductAttributeParser productAttributeParser,
            ITaxService taxService, ICurrencyService currencyService, 
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeFormatter checkoutAttributeFormatter, 
            IDiscountService discountService,
            ICustomerService customerService, 
            IGiftCardService giftCardService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICheckoutAttributeService checkoutAttributeService, 
            IPermissionService permissionService, 
            IDownloadService downloadService,
            IWebHelper webHelper, 
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IAddressWebService addressWebService,
            IShoppingCartWebService shoppingCartWebService,
            IProductReservationService productReservationService,
            ICacheManager cacheManager,
            IPictureService pictureService,
            IAuctionService auctionService,
            MediaSettings mediaSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings, 
            OrderSettings orderSettings,
            ShippingSettings shippingSettings, 
            TaxSettings taxSettings,
            CaptchaSettings captchaSettings, 
            AddressSettings addressSettings,
            RewardPointsSettings rewardPointsSettings
            )
        {
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._localizationService = localizationService;
            this._productAttributeParser = productAttributeParser;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._discountService = discountService;
            this._customerService = customerService;
            this._giftCardService = giftCardService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._genericAttributeService = genericAttributeService;
            this._addressWebService = addressWebService;
            this._shoppingCartWebService = shoppingCartWebService;
            this._productReservationService = productReservationService;
            this._cacheManager = cacheManager;
            this._pictureService = pictureService;
            this._auctionService = auctionService;
            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._captchaSettings = captchaSettings;
            this._addressSettings = addressSettings;
            this._rewardPointsSettings = rewardPointsSettings;
        }

        #endregion

        #region Shopping cart
        
        //add product to cart using AJAX
        //currently we use this method on catalog pages (category/manufacturer/etc)
        [HttpPost]
        public virtual IActionResult AddProductToCart_Catalog(string productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }
            

            var allowedQuantities = product.ParseAllowedQuantities();
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            if (product.ProductAttributeMappings.Any())
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            var customer = _workContext.CurrentCustomer;

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == cartType)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product.Id);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(customer, new ShoppingCartItem() { ShoppingCartType = cartType, StoreId = _storeContext.CurrentStore.Id, CustomerEnteredPrice = decimal.Zero,
                Quantity = quantityToValidate },
                product, false);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(customer: customer,
                productId: productId,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                quantity: quantity);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            var addtoCartModel = _shoppingCartWebService.PrepareAddToCartModel(product, customer, quantity, 0, "", cartType, null, null, "", "", "");

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist"),
                            });
                        }
                        
                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                        customer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .Sum(x=>x.Quantity));

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            html = this.RenderPartialViewToString("AddToCart", addtoCartModel),
                            updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart"),
                            });
                        }
                        
                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                        customer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .Sum(x => x.Quantity));
                        
                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            html = this.RenderPartialViewToString("AddToCart", addtoCartModel),
                            updatetopcartsectionhtml = updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml = updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        //add product to cart using AJAX
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual IActionResult AddProductToCart_Details(string productId, int shoppingCartTypeId, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage"),
                });
            }

            //you can't add group products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Grouped products couldn't be added to the cart"
                });
            }

            //you can't add reservation product to wishlist
            if (product.ProductType == ProductType.Reservation && (ShoppingCartType)shoppingCartTypeId == ShoppingCartType.Wishlist)
            {
                return Json(new
                {
                    success = false,
                    message = "Reservation products couldn't be added to the wishlist"
                });
            }

            //you can't add auction product to wishlist
            if (product.ProductType == ProductType.Auction && (ShoppingCartType)shoppingCartTypeId == ShoppingCartType.Wishlist)
            {
                return Json(new
                {
                    success = false,
                    message = "Auction products couldn't be added to the wishlist"
                });
            }

            #region Update existing shopping cart item?
            string updatecartitemid = "";
            foreach (string formKey in form.Keys)
                if (formKey.Equals(string.Format("addtocart_{0}.UpdatedShoppingCartItemId", productId), StringComparison.OrdinalIgnoreCase))
                {
                    updatecartitemid = form[formKey];
                    break;
                }
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && !String.IsNullOrEmpty(updatecartitemid))
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartTypeId == shoppingCartTypeId)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);

                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }
            #endregion

            #region Customer entered price
            decimal customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
            {
                foreach (string formKey in form.Keys)
                {
                    if (formKey.Equals(string.Format("addtocart_{0}.CustomerEnteredPrice", productId), StringComparison.OrdinalIgnoreCase))
                    {
                        if (decimal.TryParse(form[formKey], out decimal customerEnteredPrice))
                            customerEnteredPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(customerEnteredPrice, _workContext.WorkingCurrency);
                        break;
                    }
                }
            }
            #endregion

            #region Quantity

            int quantity = 1;
            foreach (string formKey in form.Keys)
                if (formKey.Equals(string.Format("addtocart_{0}.EnteredQuantity", productId), StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }

            #endregion

            //product and gift card attributes
            string attributes = _shoppingCartWebService.ParseProductAttributes(product, form);
            
            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.ProductType == ProductType.Reservation)
            {
                _shoppingCartWebService.ParseReservationDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            //product reservation
            string reservationId = "";
            string parameter = "";
            string duration = "";
            if (product.ProductType == ProductType.Reservation)
            {
                foreach (string formKey in form.Keys)
                {
                    if (formKey.Contains("Reservation"))
                    {
                        reservationId = form["Reservation"].ToString();
                        break;
                    }
                }

                if (product.IntervalUnitType == IntervalUnit.Hour || product.IntervalUnitType == IntervalUnit.Minute)
                {
                    if (string.IsNullOrEmpty(reservationId))
                    {
                        return Json(new
                        {
                            success = false,
                            message = _localizationService.GetResource("Product.Addtocart.Reservation.Required")
                        });
                    }
                    var reservation = _productReservationService.GetProductReservation(reservationId);
                    if (reservation == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "No reservation found"
                        });
                    }
                    duration = reservation.Duration;
                    rentalStartDate = reservation.Date;
                    parameter = reservation.Parameter;
                }
                else if (product.IntervalUnitType == IntervalUnit.Day)
                {
                    string datefrom = "";
                    string dateto = "";
                    foreach (var item in form)
                    {
                        if (item.Key == "reservationDatepickerFrom")
                        {
                            datefrom = item.Value;
                        }

                        if (item.Key == "reservationDatepickerTo")
                        {
                            dateto = item.Value;
                        }
                    }

                    string datePickerFormat = "MM/dd/yyyy";
                    if (!string.IsNullOrEmpty(datefrom))
                    {
                        rentalStartDate = DateTime.ParseExact(datefrom, datePickerFormat, CultureInfo.InvariantCulture);
                    }

                    if (!string.IsNullOrEmpty(dateto))
                    {
                        rentalEndDate = DateTime.ParseExact(dateto, datePickerFormat, CultureInfo.InvariantCulture);
                    }
                }
            }

            var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
                        //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                        updatecartitem.ShoppingCartType;

            //save item
            var addToCartWarnings = new List<string>();

            if (product.AvailableEndDateTimeUtc.HasValue && product.AvailableEndDateTimeUtc.Value < DateTime.UtcNow)
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("ShoppingCart.NotAvailable")
                });
            }

            if (updatecartitem == null)
            {
                //add to the cart
                addToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                    productId, cartType, _storeContext.CurrentStore.Id,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true, reservationId, parameter, duration));
            }
            else
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartType == updatecartitem.ShoppingCartType)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                var otherCartItemWithSameParameters = _shoppingCartService.FindShoppingCartItemInTheCart(
                    cart, updatecartitem.ShoppingCartType, productId, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    //ensure it's other shopping cart cart item
                    otherCartItemWithSameParameters = null;
                }
                //update existing item
                addToCartWarnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    updatecartitem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    //delete the same shopping cart item (the other one)
                    _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer.Id, otherCartItemWithSameParameters);
                }
            }

            #region Return result

            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart/wishlist
                //let's display warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            var addtoCartModel = _shoppingCartWebService.PrepareAddToCartModel(product, _workContext.CurrentCustomer, quantity, customerEnteredPriceConverted, attributes, cartType, rentalStartDate, rentalEndDate, reservationId, parameter, duration);

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist"),
                            });
                        }
                        
                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .Sum(x => x.Quantity));
                        
                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                            html = this.RenderPartialViewToString("AddToCart", addtoCartModel),
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart"),
                            });
                        }
                        
                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                        _workContext.CurrentCustomer.ShoppingCartItems
                        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                        .LimitPerStore(_storeContext.CurrentStore.Id)
                        .Sum(x => x.Quantity));

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            html = this.RenderPartialViewToString("AddToCart", addtoCartModel),
                            updatetopcartsectionhtml = updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml = updateflyoutcartsectionhtml,
                            refreshreservation = product.ProductType == ProductType.Reservation && product.IntervalUnitType != IntervalUnit.Day
                        });
                    }
            }


            #endregion
        }

        [HttpPost]
        public virtual IActionResult AddBid(string productId, int shoppingCartTypeId, IFormCollection form)
        {
            var customer = _workContext.CurrentCustomer;
            if (!customer.IsRegistered())
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("ShoppingCart.Mustberegisteredtobid")
                });
            }
            decimal bid = 0;
            foreach (string formKey in form.Keys)
            {
                if (formKey.Equals(string.Format("auction_{0}.HighestBidValue", productId), StringComparison.OrdinalIgnoreCase))
                {
                    decimal.TryParse(form[formKey], out bid);
                    if(bid == 0)
                        decimal.TryParse(form[formKey],NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US").NumberFormat, out bid);

                    bid = Math.Round(bid, 2);
                    break;
                }
            }
            if(bid <= 0 )
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("ShoppingCart.BidMustBeHigher")
                });
            }

            Product product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentNullException("product");

            if(product.HighestBidder == customer.Id)
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("ShoppingCart.AlreadyHighestBidder")
                });
            }

            var warnings = _shoppingCartService.GetStandardWarnings(customer, ShoppingCartType.Auctions, product, "", bid, 1).ToList();
            warnings.AddRange(_shoppingCartService.GetAuctionProductWarning(bid, product, customer));

            if (warnings.Any())
            {
                string toReturn = "";
                foreach (var warning in warnings)
                {
                    toReturn += warning + "</br>";
                }

                return Json(new
                {
                    success = false,
                    message = toReturn
                });
            }

            //insert new bid
            _auctionService.NewBid(customer, product, _storeContext.CurrentStore, _workContext.WorkingLanguage, bid);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddNewBid", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToBid"), product.Name);

            var addtoCartModel = _shoppingCartWebService.PrepareAddToCartModel(product, customer, 1, 0, "", ShoppingCartType.Auctions, null, null, "", "", "");

            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.Yourbidhasbeenplaced"),
                html = this.RenderPartialViewToString("AddToCart", addtoCartModel)
            });
        }

        //handle product attribute selection event. this way we return new price, overridden gtin/sku/mpn
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual IActionResult ProductDetails_AttributeChange(string productId, bool validateAttributeConditions, bool loadPicture, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            string attributeXml = _shoppingCartWebService.ParseProductAttributes(product, form);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.ProductType == ProductType.Reservation)
            {
                _shoppingCartWebService.ParseReservationDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            string sku = product.FormatSku(attributeXml, _productAttributeParser);
            string mpn = product.FormatMpn(attributeXml, _productAttributeParser);
            string gtin = product.FormatGtin(attributeXml, _productAttributeParser);


            string price = "";
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice && product.ProductType != ProductType.Auction)
            {
                //we do not calculate price of "customer enters price" option is enabled
                decimal finalPrice = _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true, out decimal discountAmount, out List<AppliedDiscount> scDiscounts);
                decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPrice, out decimal taxRate);
                decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
            }
            //stock
            var stockAvailability = product.FormatStockMessage(attributeXml, _localizationService, _productAttributeParser, _storeContext);

            //conditional attributes
            var enabledAttributeMappingIds = new List<string>();
            var disabledAttributeMappingIds = new List<string>();
            if (validateAttributeConditions)
            {
                var attributes = product.ProductAttributeMappings;
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(product, attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }
            //picture. used when we want to override a default product picture when some attribute is selected
            var pictureFullSizeUrl = string.Empty;
            var pictureDefaultSizeUrl = string.Empty;
            if (loadPicture)
            {
                
                //first, try to get product attribute combination picture
                var pictureId =  product.ProductAttributeCombinations.Where(x=>x.AttributesXml == attributeXml).FirstOrDefault()?.PictureId ?? "";
                //then, let's see whether we have attribute values with pictures
                if (string.IsNullOrEmpty(pictureId))
                {
                    pictureId = _productAttributeParser.ParseProductAttributeValues(product, attributeXml)
                        .FirstOrDefault(attributeValue => !string.IsNullOrEmpty(attributeValue.PictureId))?.PictureId ?? "";
                }

                if (!string.IsNullOrEmpty(pictureId))
                {
                    var productAttributePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_PICTURE_MODEL_KEY,
                        pictureId, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    var pictureModel = _cacheManager.Get(productAttributePictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(pictureId);
                        return picture == null ? new PictureModel() : new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize)
                        };
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                }

            }
            return Json(new
            {
                gtin = gtin,
                mpn = mpn,
                sku = sku,
                price = price,
                stockAvailability = stockAvailability,
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                pictureFullSizeUrl,
                pictureDefaultSizeUrl,
            });
        }

        [HttpPost]
        public virtual IActionResult CheckoutAttributeChange(IFormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            _shoppingCartWebService.ParseAndSaveCheckoutAttributes(cart, form);
            var attributeXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes,
                _storeContext.CurrentStore.Id);

            var enabledAttributeIds = new List<string>();
            var disabledAttributeIds = new List<string>();
            var attributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in attributes)
            {
                var conditionMet = _checkoutAttributeParser.IsConditionMet(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }

            return Json(new
            {
                enabledattributeids = enabledAttributeIds.ToArray(),
                disabledattributeids = disabledAttributeIds.ToArray()
            });
        }

        [HttpPost]
        public virtual IActionResult UploadFileProductAttribute(string attributeId, string productId)
        {
            var product = _productService.GetProductById(productId);
            var attribute = product.ProductAttributeMappings.Where(x => x.Id == attributeId).FirstOrDefault(); 
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }
            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty,
                });
            }
            var fileBinary = httpPostedFile.GetDownloadBits();

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (String.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = Path.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();


            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty,
                    });
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
            });
        }

        [HttpPost]
        public virtual IActionResult UploadFileCheckoutAttribute(string attributeId)
        {
            var attribute = _checkoutAttributeService.GetCheckoutAttributeById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty,
                });
            }

            var fileBinary = httpPostedFile.GetDownloadBits();

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (String.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = Path.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty,
                    });
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = Path.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
            });
        }


        public virtual IActionResult Cart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            _shoppingCartWebService.PrepareShoppingCart(model, cart);
            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public virtual IActionResult UpdateCart(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = !string.IsNullOrEmpty(form["removefromcart"].ToString()) ? form["removefromcart"].ToString().Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x).ToList() : new List<string>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<string, IList<string>>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer.Id, sci, ensureOnlyActiveCheckoutAttributes: true);
                else
                {
                    foreach (string formKey in form.Keys)
                        if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(form[formKey], out int newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true, sci.ReservationId, sci.Id);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }
            //parse and save checkout attributes
            _shoppingCartWebService.ParseAndSaveCheckoutAttributes(cart, form);

            //updated cart
            _workContext.CurrentCustomer = _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            _shoppingCartWebService.PrepareShoppingCart(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }
            return View(model);
        }
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("clearcart")]
        public virtual IActionResult ClearCart(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var innerWarnings = new Dictionary<string, IList<string>>();
            foreach (var sci in cart)
            {
                _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer.Id, sci, ensureOnlyActiveCheckoutAttributes: true);
            }
            //parse and save checkout attributes
            _shoppingCartWebService.ParseAndSaveCheckoutAttributes(cart, form);

            return RedirectToRoute("HomePage");

        }
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("continueshopping")]
        public virtual IActionResult ContinueShopping()
        {
            var returnUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToRoute("HomePage");
            }
        }
        
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("checkout")]
        public virtual IActionResult StartCheckout(IFormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            _shoppingCartWebService.ParseAndSaveCheckoutAttributes(cart, form);

            //validate attributes
            var checkoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
            var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Any())
            {
                //something wrong, redisplay the page with warnings
                var model = new ShoppingCartModel();
                _shoppingCartWebService.PrepareShoppingCart(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }

            //everything is OK
            if (_workContext.CurrentCustomer.IsGuest())
            {
                if (!_orderSettings.AnonymousCheckoutAllowed)
                    return Challenge();

                return RedirectToRoute("LoginCheckoutAsGuest", new {returnUrl = Url.RouteUrl("ShoppingCart")});
            }
            
            return RedirectToRoute("Checkout");
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applydiscountcouponcode")]
        public virtual IActionResult ApplyDiscountCoupon(string discountcouponcode, IFormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            _shoppingCartWebService.ParseAndSaveCheckoutAttributes(cart, form);
            
            var model = new ShoppingCartModel();
            if (!String.IsNullOrWhiteSpace(discountcouponcode))
            {
                discountcouponcode = discountcouponcode.ToUpper();
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discount = _discountService.GetDiscountByCouponCode(discountcouponcode, true);
                if (discount != null && discount.RequiresCouponCode)
                {
                    var coupons = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
                    var existsAndUsed = false;
                    foreach (var item in coupons)
                    {
                        if (_discountService.ExistsCodeInDiscount(item, discount.Id, null))
                            existsAndUsed = true;
                    }
                    if (!existsAndUsed)
                    {
                        if (!discount.Reused)
                            existsAndUsed = !_discountService.ExistsCodeInDiscount(discountcouponcode, discount.Id, false);

                        if (!existsAndUsed)
                        {
                            var validationResult = _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer, discountcouponcode);
                            if (validationResult.IsValid)
                            {
                                //valid
                                _workContext.CurrentCustomer.ApplyDiscountCouponCode(discountcouponcode);
                                model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");
                                model.DiscountBox.IsApplied = true;
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(validationResult.UserError))
                                {
                                    //some user error
                                    model.DiscountBox.Message = validationResult.UserError;
                                    model.DiscountBox.IsApplied = false;
                                }
                                else
                                {
                                    //general error text
                                    model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                                    model.DiscountBox.IsApplied = false;
                                }
                            }
                        }
                        else
                        {
                            model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WasUsed");
                            model.DiscountBox.IsApplied = false;
                        }
                    }
                    else
                    {
                        model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.UsesTheSameDiscount");
                        model.DiscountBox.IsApplied = false;
                    }
                }
                else
                {
                    model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                    model.DiscountBox.IsApplied = false;
                }
            }
            else
            {
                model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                model.DiscountBox.IsApplied = false;
            }

            _shoppingCartWebService.PrepareShoppingCart(model, cart);
            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applygiftcardcouponcode")]
        public virtual IActionResult ApplyGiftCard(string giftcardcouponcode, IFormCollection form)
        {
            //trim
            if (giftcardcouponcode != null)
                giftcardcouponcode = giftcardcouponcode.Trim();

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            _shoppingCartWebService.ParseAndSaveCheckoutAttributes(cart, form);
            
            var model = new ShoppingCartModel();
            if (!cart.IsRecurring())
            {
                if (!String.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = _giftCardService.GetAllGiftCards(giftCardCouponCode: giftcardcouponcode).FirstOrDefault();
                    bool isGiftCardValid = giftCard != null && giftCard.IsGiftCardValid();
                    if (isGiftCardValid)
                    {
                        _workContext.CurrentCustomer.ApplyGiftCardCouponCode(giftcardcouponcode);
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.Applied");
                        model.GiftCardBox.IsApplied = true;
                    }
                    else
                    {
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        model.GiftCardBox.IsApplied = false;
                    }
                }
                else
                {
                    model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                model.GiftCardBox.IsApplied = false;
            }

            _shoppingCartWebService.PrepareShoppingCart(model, cart);
            return View(model);
        }

        [PublicAntiForgery]
        [HttpPost]
        public virtual IActionResult GetEstimateShipping(string countryId, string stateProvinceId, string zipPostalCode, IFormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            _shoppingCartWebService.ParseAndSaveCheckoutAttributes(cart, form);
            var model = _shoppingCartWebService.PrepareEstimateShippingResult(cart, countryId, stateProvinceId, zipPostalCode);

            return PartialView("_EstimateShippingResult", model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removediscount-")]
        public virtual IActionResult RemoveDiscountCoupon(IFormCollection form)
        {          
            var model = new ShoppingCartModel();
            string discountId = string.Empty;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("removediscount-", StringComparison.OrdinalIgnoreCase))
                    discountId = formValue.Substring("removediscount-".Length);

            var discount = _discountService.GetDiscountById(discountId);
            if (discount != null)
            {
                var coupons = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
                foreach (var item in coupons)
                {
                    var dd = _discountService.GetDiscountByCouponCode(item);
                    if(dd.Id == discount.Id)
                        _workContext.CurrentCustomer.RemoveDiscountCouponCode(item);
                }
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            _shoppingCartWebService.PrepareShoppingCart(model, cart);
            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removegiftcard-")]
        public virtual IActionResult RemoveGiftCardCode(IFormCollection form)
        {
            var model = new ShoppingCartModel();

            //get gift card identifier
            string giftCardId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("removegiftcard-", StringComparison.OrdinalIgnoreCase))
                    giftCardId = formValue.Substring("removegiftcard-".Length);
            var gc = _giftCardService.GetGiftCardById(giftCardId);
            if (gc != null)
            {
                _workContext.CurrentCustomer.RemoveGiftCardCouponCode(gc.GiftCardCouponCode);
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            _shoppingCartWebService.PrepareShoppingCart(model, cart);
            return View(model);
        }
        #endregion

        #region Wishlist

        public virtual IActionResult Wishlist(Guid? customerGuid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            Customer customer = customerGuid.HasValue ? 
                _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (customer == null)
                return RedirectToRoute("HomePage");
            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            _shoppingCartWebService.PrepareWishlist(model, cart, !customerGuid.HasValue);
            return View(model);
        }

        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public virtual IActionResult UpdateWishlist(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var customer = _workContext.CurrentCustomer;

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allIdsToRemove = !string.IsNullOrEmpty(form["removefromcart"].ToString()) 
                ? form["removefromcart"].ToString().Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x=>x)
                .ToList() 
                : new List<string>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<string, IList<string>>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    _shoppingCartService.DeleteShoppingCartItem(customer.Id, sci);
                else
                {
                    foreach (string formKey in form.Keys)
                        if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(form[formKey], out int newQuantity))
                            {
                                var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            //updated wishlist
            _workContext.CurrentCustomer = _customerService.GetCustomerById(customer.Id);

            cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new WishlistModel();
            _shoppingCartWebService.PrepareWishlist(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }
            return View(model);
        }
       
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("addtocartbutton")]
        public virtual IActionResult AddItemsToCartFromWishlist(Guid? customerGuid, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var pageCustomer = customerGuid.HasValue
                ? _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (pageCustomer == null)
                return RedirectToRoute("HomePage");

            var pageCart = pageCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var allWarnings = new List<string>();
            var numberOfAddedItems = 0;
            var allIdsToAdd = form.ContainsKey("addtocart") ? form["addtocart"].ToString().Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x=>x)
                .ToList() 
                : new List<string>();
            foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var product = _productService.GetProductById(sci.ProductId);
                    var warnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                        sci.ProductId, ShoppingCartType.ShoppingCart,
                        _storeContext.CurrentStore.Id,
                        sci.AttributesXml, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                    if (!warnings.Any())
                        numberOfAddedItems++;
                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        !warnings.Any()) //no warnings ( already in the cart)
                    {
                        //let's remove the item from wishlist
                        _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer.Id, sci);
                    }
                    allWarnings.AddRange(warnings);
                }
            }

            if (numberOfAddedItems > 0)
            {
                //redirect to the shopping cart page

                if (allWarnings.Any())
                {
                    ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), true);
                }

                return RedirectToRoute("ShoppingCart");
            }
            else
            {
                //no items added. redisplay the wishlist page

                if (allWarnings.Any())
                {
                    ErrorNotification(_localizationService.GetResource("Wishlist.AddToCart.Error"), false);
                }
                //no items added. redisplay the wishlist page
                var cart = pageCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                var model = new WishlistModel();
                _shoppingCartWebService.PrepareWishlist(model, cart, !customerGuid.HasValue);
                return View(model);
            }
        }

        public virtual IActionResult EmailWishlist()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (!cart.Any())
                return RedirectToRoute("HomePage");

            var model = new WishlistEmailAFriendModel
            {
                YourEmailAddress = _workContext.CurrentCustomer.Email,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage
            };
            return View(model);
        }

        [HttpPost, ActionName("EmailWishlist")]
        [FormValueRequired("send-email")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult EmailWishlistSend(WishlistEmailAFriendModel model, bool captchaValid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (!cart.Any())
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //check whether the current customer is guest and ia allowed to email wishlist
            if (_workContext.CurrentCustomer.IsGuest() && !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Wishlist.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                EngineContext.Current.Resolve<IWorkflowMessageService>().SendWishlistEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, model.YourEmailAddress,
                        model.FriendEmail, Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Wishlist.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage;
            return View(model);
        }

        #endregion
    }
}
