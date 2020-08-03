using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.ShoppingCart;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Web.Controllers
{
    public partial class AddToCartController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IMediator _mediator;

        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region CTOR

        public AddToCartController(IProductService productService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            IMediator mediator,
            ShoppingCartSettings shoppingCartSettings)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _mediator = mediator;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methods


        //add product to cart using AJAX
        //currently we use this method on catalog pages (category/manufacturer/etc)
        [HttpPost]
        public virtual async Task<IActionResult> AddProductToCart_Catalog(string productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = await _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //we can add only simple products and 
            if (product.ProductType != ProductType.SimpleProduct || _shoppingCartSettings.AllowToSelectWarehouse)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }


            var allowedQuantities = product.ParseAllowedQuantities();
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }

            if (product.ProductAttributeMappings.Any())
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }

            var customer = _workContext.CurrentCustomer;

            string warehouseId =
               product.UseMultipleWarehouses ? _storeContext.CurrentStore.DefaultWarehouseId :
               (string.IsNullOrEmpty(_storeContext.CurrentStore.DefaultWarehouseId) ? product.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId);

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, cartType);
            var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product.Id, warehouseId);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = await _shoppingCartService
                .GetShoppingCartItemWarnings(customer, new ShoppingCartItem() {
                    ShoppingCartType = cartType,
                    StoreId = _storeContext.CurrentStore.Id,
                    CustomerEnteredPrice = decimal.Zero,
                    WarehouseId = warehouseId,
                    Quantity = quantityToValidate
                },
                product, false);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = await _shoppingCartService.AddToCart(customer: customer,
                productId: productId,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                warehouseId: warehouseId,
                quantity: quantity);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }

            var addtoCartModel = await _mediator.Send(new GetAddToCart() {
                Product = product,
                Customer = customer,
                Quantity = quantity,
                CartType = cartType,
                CustomerEnteredPrice = 0,
                AttributesXml = "",
                Currency = _workContext.WorkingCurrency,
                Store = _storeContext.CurrentStore,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType,
            });

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        await _customerActivityService.InsertActivity("PublicStore.AddToWishlist", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

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
                            _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.Wishlist)
                                .Sum(x => x.Quantity));

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            html = await RenderPartialViewToString("_PopupAddToCart", addtoCartModel),
                            updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        await _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart"),
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCartTypes = new List<ShoppingCartType>();
                        shoppingCartTypes.Add(ShoppingCartType.ShoppingCart);
                        shoppingCartTypes.Add(ShoppingCartType.Auctions);
                        if (_shoppingCartSettings.AllowOnHoldCart)
                            shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, shoppingCartTypes.ToArray())
                                .Sum(x => x.Quantity));

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            html = await RenderPartialViewToString("_PopupAddToCart", addtoCartModel),
                            updatetopcartsectionhtml = updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml = updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        //add product to cart using AJAX
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual async Task<IActionResult> AddProductToCart_Details(string productId, int shoppingCartTypeId, IFormCollection form)
        {
            var product = await _productService.GetProductById(productId);
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
            //check available date
            if (product.AvailableEndDateTimeUtc.HasValue && product.AvailableEndDateTimeUtc.Value < DateTime.UtcNow)
            {
                return Json(new
                {
                    success = false,
                    message = _localizationService.GetResource("ShoppingCart.NotAvailable")
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
            if (_shoppingCartSettings.AllowCartItemEditing && !string.IsNullOrEmpty(updatecartitemid))
            {
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, (ShoppingCartType)shoppingCartTypeId);
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
                            customerEnteredPriceConverted = await _currencyService.ConvertToPrimaryStoreCurrency(customerEnteredPrice, _workContext.WorkingCurrency);
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
            string attributes = await _mediator.Send(new GetParseProductAttributes() { Product = product, Form = form });

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.ProductType == ProductType.Reservation)
            {
                product.ParseReservationDates(form, out rentalStartDate, out rentalEndDate);
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
                    var productReservationService = HttpContext.RequestServices.GetRequiredService<IProductReservationService>();
                    var reservation = await productReservationService.GetProductReservation(reservationId);
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

            
            string warehouseId = _shoppingCartSettings.AllowToSelectWarehouse ?
                form["WarehouseId"].ToString() :
                product.UseMultipleWarehouses ? _storeContext.CurrentStore.DefaultWarehouseId :
                (string.IsNullOrEmpty(_storeContext.CurrentStore.DefaultWarehouseId) ? product.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId);

            if (updatecartitem == null)
            {
                //add to the cart
                addToCartWarnings.AddRange(await _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                    productId, cartType, _storeContext.CurrentStore.Id, warehouseId,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true, reservationId, parameter, duration));
            }
            else
            {
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, (ShoppingCartType)shoppingCartTypeId);
                var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCart(
                    cart, updatecartitem.ShoppingCartType, productId, warehouseId, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    //ensure it's other shopping cart cart item
                    otherCartItemWithSameParameters = null;
                }
                //update existing item
                addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    updatecartitem.Id, warehouseId, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    //delete the same shopping cart item (the other one)
                    await _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer, otherCartItemWithSameParameters);
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

            var addtoCartModel = await _mediator.Send(new GetAddToCart() {
                Product = product,
                Customer = _workContext.CurrentCustomer,
                Quantity = quantity,
                CartType = cartType,
                CustomerEnteredPrice = customerEnteredPriceConverted,
                AttributesXml = attributes,
                Currency = _workContext.WorkingCurrency,
                Store = _storeContext.CurrentStore,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType,
                Duration = duration,
                Parameter = parameter,
                ReservationId = reservationId,
                StartDate = rentalStartDate,
                EndDate = rentalEndDate
            });

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        await _customerActivityService.InsertActivity("PublicStore.AddToWishlist", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

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
                            _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.Wishlist)
                                .Sum(x => x.Quantity));

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml = updatetopwishlistsectionhtml,
                            html = await RenderPartialViewToString("_PopupAddToCart", addtoCartModel),
                        });
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        await _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart"),
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCartTypes = new List<ShoppingCartType>();
                        shoppingCartTypes.Add(ShoppingCartType.ShoppingCart);
                        shoppingCartTypes.Add(ShoppingCartType.Auctions);
                        if (_shoppingCartSettings.AllowOnHoldCart)
                            shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, shoppingCartTypes.ToArray())
                                .Sum(x => x.Quantity));

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            html = await RenderPartialViewToString("_PopupAddToCart", addtoCartModel),
                            updatetopcartsectionhtml = updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml = updateflyoutcartsectionhtml,
                            refreshreservation = product.ProductType == ProductType.Reservation && product.IntervalUnitType != IntervalUnit.Day
                        });
                    }
            }


            #endregion
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddBid(string productId, int shoppingCartTypeId, IFormCollection form,
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

            Product product = await _productService.GetProductById(productId);
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

            var warehouseId = _shoppingCartSettings.AllowToSelectWarehouse ? form["WarehouseId"].ToString() : _storeContext.CurrentStore.DefaultWarehouseId;

            var shoppingCartItem = new ShoppingCartItem {
                AttributesXml = "",
                CreatedOnUtc = DateTime.UtcNow,
                ProductId = product.Id,
                WarehouseId = warehouseId,
                ShoppingCartType = ShoppingCartType.Auctions,
                StoreId = _storeContext.CurrentStore.Id,
                CustomerEnteredPrice = bid,
                Quantity = 1
            };

            var warnings = (await _shoppingCartService.GetStandardWarnings(customer, product, shoppingCartItem)).ToList();
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
            await auctionService.NewBid(customer, product, _storeContext.CurrentStore, _workContext.WorkingLanguage, warehouseId, bid);

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.AddNewBid", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddToBid"), product.Name);

            var addtoCartModel = await _mediator.Send(new GetAddToCart() {
                Product = product,
                Customer = customer,
                Quantity = 1,
                CartType = ShoppingCartType.Auctions,
                CustomerEnteredPrice = 0,
                Currency = _workContext.WorkingCurrency,
                Store = _storeContext.CurrentStore,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType,
            });

            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.Yourbidhasbeenplaced"),
                html = await RenderPartialViewToString("_PopupAddToCart", addtoCartModel)
            });
        }


        #endregion

    }
}
