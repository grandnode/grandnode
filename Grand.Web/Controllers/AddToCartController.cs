using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Tax;
using Grand.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Grand.Web.Controllers
{
    public partial class AddToCartController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductReservationService _productReservationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShoppingCartViewModelService _shoppingCartViewModelService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ICurrencyService _currencyService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICacheManager _cacheManager;
        private readonly ITaxService _taxService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region CTOR

        public AddToCartController(IProductService productService,
            IProductReservationService productReservationService,
            IShoppingCartService shoppingCartService,
            IShoppingCartViewModelService shoppingCartViewModelService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            ICurrencyService currencyService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            ICacheManager cacheManager,
            ITaxService taxService,
            IProductAttributeParser productAttributeParser,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IPictureService pictureService,
            ShoppingCartSettings shoppingCartSettings,
            MediaSettings mediaSettings)
        {
            this._productService = productService;
            this._productReservationService = productReservationService;
            this._shoppingCartService = shoppingCartService;
            this._shoppingCartViewModelService = shoppingCartViewModelService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._currencyService = currencyService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._cacheManager = cacheManager;
            this._taxService = taxService;
            this._productAttributeParser = productAttributeParser;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._pictureService = pictureService;
            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methods


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
                .GetShoppingCartItemWarnings(customer, new ShoppingCartItem()
                {
                    ShoppingCartType = cartType,
                    StoreId = _storeContext.CurrentStore.Id,
                    CustomerEnteredPrice = decimal.Zero,
                    Quantity = quantityToValidate
                },
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

            var addtoCartModel = _shoppingCartViewModelService.PrepareAddToCartModel(product, customer, quantity, 0, "", cartType, null, null, "", "", "");

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
                        .Sum(x => x.Quantity));

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            html = this.RenderPartialViewToString("_AddToCart", addtoCartModel),
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
                            html = this.RenderPartialViewToString("_AddToCart", addtoCartModel),
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
            string attributes = _shoppingCartViewModelService.ParseProductAttributes(product, form);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.ProductType == ProductType.Reservation)
            {
                _shoppingCartViewModelService.ParseReservationDates(product, form, out rentalStartDate, out rentalEndDate);
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
                    _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer, otherCartItemWithSameParameters);
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

            var addtoCartModel = _shoppingCartViewModelService.PrepareAddToCartModel(product, _workContext.CurrentCustomer, quantity, customerEnteredPriceConverted, attributes, cartType, rentalStartDate, rentalEndDate, reservationId, parameter, duration);

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
                            html = this.RenderPartialViewToString("_AddToCart", addtoCartModel),
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
                            html = this.RenderPartialViewToString("_AddToCart", addtoCartModel),
                            updatetopcartsectionhtml = updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml = updateflyoutcartsectionhtml,
                            refreshreservation = product.ProductType == ProductType.Reservation && product.IntervalUnitType != IntervalUnit.Day
                        });
                    }
            }


            #endregion
        }

        [HttpPost]
        public virtual IActionResult AddBid(string productId, int shoppingCartTypeId, IFormCollection form,
            [FromServices] IAuctionService auctionService)
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
                    if (bid == 0)
                        decimal.TryParse(form[formKey], NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US").NumberFormat, out bid);

                    bid = Math.Round(bid, 2);
                    break;
                }
            }
            if (bid <= 0)
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

            if (product.HighestBidder == customer.Id)
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
            auctionService.NewBid(customer, product, _storeContext.CurrentStore, _workContext.WorkingLanguage, bid);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddNewBid", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToBid"), product.Name);

            var addtoCartModel = _shoppingCartViewModelService.PrepareAddToCartModel(product, customer, 1, 0, "", ShoppingCartType.Auctions, null, null, "", "", "");

            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.Yourbidhasbeenplaced"),
                html = this.RenderPartialViewToString("_AddToCart", addtoCartModel)
            });
        }

        
        #endregion

    }
}
