using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Captcha;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Queries.Models.Orders;
using Grand.Services.Security;
using Grand.Web.Commands.Models.ShoppingCart;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class ShoppingCartController : BasePublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IMediator _mediator;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Constructors

        public ShoppingCartController(
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            ILocalizationService localizationService,
            IDiscountService discountService,
            ICustomerService customerService,
            ICheckoutAttributeService checkoutAttributeService,
            IPermissionService permissionService,
            IGenericAttributeService genericAttributeService,
            IMediator mediator,
            ShoppingCartSettings shoppingCartSettings,
            OrderSettings orderSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _localizationService = localizationService;
            _discountService = discountService;
            _customerService = customerService;
            _checkoutAttributeService = checkoutAttributeService;
            _permissionService = permissionService;
            _genericAttributeService = genericAttributeService;
            _mediator = mediator;
            _shoppingCartSettings = shoppingCartSettings;
            _orderSettings = orderSettings;
        }

        #endregion


        #region Utilities

        protected ShoppingCartType[] PrepareCartTypes()
        {
            var shoppingCartTypes = new List<ShoppingCartType>();
            shoppingCartTypes.Add(ShoppingCartType.ShoppingCart);
            shoppingCartTypes.Add(ShoppingCartType.Auctions);
            if (_shoppingCartSettings.AllowOnHoldCart)
                shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

            return shoppingCartTypes.ToArray();
        }

        #endregion

        #region Shopping cart

        [HttpPost]
        public virtual async Task<IActionResult> CheckoutAttributeChange(IFormCollection form,
            [FromServices] ICheckoutAttributeParser checkoutAttributeParser,
            [FromServices] ICheckoutAttributeFormatter checkoutAttributeFormatter)
        {
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            var attributeXml = await _mediator.Send(new SaveCheckoutAttributesCommand() {
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore,
                Cart = cart,
                Form = form
            });

            var enabledAttributeIds = new List<string>();
            var disabledAttributeIds = new List<string>();
            var attributes = await _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in attributes)
            {
                var conditionMet = await checkoutAttributeParser.IsConditionMet(attribute, attributeXml);
                if (conditionMet.HasValue)
                {
                    if (conditionMet.Value)
                        enabledAttributeIds.Add(attribute.Id);
                    else
                        disabledAttributeIds.Add(attribute.Id);
                }
            }
            var model = await _mediator.Send(new GetOrderTotals() {
                Cart = cart,
                IsEditable = true,
                Store = _storeContext.CurrentStore,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType
            });

            return Json(new
            {
                enabledattributeids = enabledAttributeIds.ToArray(),
                disabledattributeids = disabledAttributeIds.ToArray(),
                htmlordertotal = await RenderPartialViewToString("Components/OrderTotals/Default", model),
                checkoutattributeinfo = await checkoutAttributeFormatter.FormatAttributes(attributeXml, _workContext.CurrentCustomer),
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> UploadFileCheckoutAttribute(string attributeId,
            [FromServices] IDownloadService downloadService)
        {
            var attribute = await _checkoutAttributeService.GetCheckoutAttributeById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }

            var form = await HttpContext.Request.ReadFormAsync();
            var httpPostedFile = form.Files.FirstOrDefault();
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
            if (string.IsNullOrEmpty(fileName) && form.ContainsKey(qqFileNameParameter))
                fileName = form[qqFileNameParameter].ToString();
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

            var download = new Download {
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
            await downloadService.InsertDownload(download);

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

        public virtual async Task<IActionResult> Cart(bool checkoutAttributes)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());
            var model = await _mediator.Send(new GetShoppingCart() {
                Cart = cart,
                ValidateCheckoutAttributes = checkoutAttributes,
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> UpdateCart(IFormCollection form)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var shoppingCartTypes = new List<ShoppingCartType>();
            shoppingCartTypes.Add(ShoppingCartType.ShoppingCart);
            if (_shoppingCartSettings.AllowOnHoldCart)
                shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, shoppingCartTypes.ToArray());

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<string, IList<string>>();
            foreach (var sci in cart)
            {
                foreach (string formKey in form.Keys)
                    if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(form[formKey], out int newQuantity))
                        {
                            var currSciWarnings = await _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                sci.Id, sci.WarehouseId, sci.AttributesXml, sci.CustomerEnteredPrice,
                                sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                newQuantity, true, sci.ReservationId, sci.Id);
                            innerWarnings.Add(sci.Id, currSciWarnings);
                        }
                        break;
                    }
            }

            cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());

            var model = await _mediator.Send(new GetShoppingCart() {
                Cart = cart,
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                TaxDisplayType = _workContext.TaxDisplayType
            });

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
            return Json(new
            {
                totalproducts = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"), model.Items.Sum(x => x.Quantity)),
                cart = RenderViewComponentToString("OrderSummary", new { overriddenModel = model })
            });
        }

        public virtual async Task<IActionResult> ClearCart()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());

            foreach (var sci in cart)
            {
                await _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer, sci, ensureOnlyActiveCheckoutAttributes: true);
            }

            return RedirectToRoute("HomePage");

        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteCartItem(string id, bool shoppingcartpage = false)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var shoppingCartTypes = new List<ShoppingCartType>();
            shoppingCartTypes.Add(ShoppingCartType.ShoppingCart);
            if (_shoppingCartSettings.AllowOnHoldCart)
                shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

            var item = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, shoppingCartTypes.ToArray())
                .FirstOrDefault(sci => sci.Id == id);

            if (item != null)
            {
                await _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer, item, ensureOnlyActiveCheckoutAttributes: true);
            }

            var model = await _mediator.Send(new GetMiniShoppingCart() {
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType,
                Store = _storeContext.CurrentStore
            });
            if (!shoppingcartpage)
            {
                return Json(new
                {
                    totalproducts = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"), model.TotalProducts),
                    flyoutshoppingcart = this.RenderViewComponentToString("FlyoutShoppingCart", model)
                });
            }
            else
            {
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());
                var shoppingcartmodel = await _mediator.Send(new GetShoppingCart() {
                    Cart = cart,
                    Customer = _workContext.CurrentCustomer,
                    Currency = _workContext.WorkingCurrency,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore,
                    TaxDisplayType = _workContext.TaxDisplayType
                });

                return Json(new
                {
                    totalproducts = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"), model.TotalProducts),
                    flyoutshoppingcart = RenderViewComponentToString("FlyoutShoppingCart", model),
                    cart = RenderViewComponentToString("OrderSummary", new { overriddenModel = shoppingcartmodel })
                });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ChangeTypeCartItem(string id, bool status = false)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            if(!_shoppingCartSettings.AllowOnHoldCart)
                return RedirectToRoute("HomePage");

            var shoppingCartTypes = new List<ShoppingCartType>();
            shoppingCartTypes.Add(ShoppingCartType.ShoppingCart);
            if (_shoppingCartSettings.AllowOnHoldCart)
                shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

            var item = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, shoppingCartTypes.ToArray())
                .FirstOrDefault(sci => sci.Id == id);

            if (item != null)
            {
                item.ShoppingCartType = status ? ShoppingCartType.ShoppingCart : ShoppingCartType.OnHoldCart;
                await _customerService.UpdateShoppingCartItem(_workContext.CurrentCustomer.Id, item);
            }

            var model = await _mediator.Send(new GetMiniShoppingCart() {
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                TaxDisplayType = _workContext.TaxDisplayType,
                Store = _storeContext.CurrentStore
            });

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());
            var shoppingcartmodel = await _mediator.Send(new GetShoppingCart() {
                Cart = cart,
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                TaxDisplayType = _workContext.TaxDisplayType
            });

            return Json(new
            {
                cart = RenderViewComponentToString("OrderSummary", new { overriddenModel = shoppingcartmodel })
            });

        }

        public virtual IActionResult ContinueShopping()
        {
            var returnUrl = _workContext.CurrentCustomer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToRoute("HomePage");
            }
        }

        public virtual async Task<IActionResult> StartCheckout(IFormCollection form = null)
        {
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);
            string checkoutAttributes;
            //parse and save checkout attributes
            if (form != null && form.Count > 0)
            {
                checkoutAttributes = await _mediator.Send(new SaveCheckoutAttributesCommand() {
                    Customer = _workContext.CurrentCustomer,
                    Store = _storeContext.CurrentStore,
                    Cart = cart,
                    Form = form
                });
            }
            else
            {
                checkoutAttributes = _workContext.CurrentCustomer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
            }

            var checkoutAttributeWarnings = await _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Any())
            {
                return RedirectToRoute("ShoppingCart", new { checkoutAttributes = true });
            }

            //everything is OK
            if (_workContext.CurrentCustomer.IsGuest())
            {
                if (!_orderSettings.AnonymousCheckoutAllowed)
                    return Challenge();

                return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
            }

            return RedirectToRoute("Checkout");
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> ApplyDiscountCoupon(string discountcouponcode)
        {
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());

            var message = string.Empty;
            var isApplied = false;

            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                discountcouponcode = discountcouponcode.ToUpper();
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discount = await _discountService.GetDiscountByCouponCode(discountcouponcode, true);
                if (discount != null && discount.RequiresCouponCode)
                {
                    var coupons = _workContext.CurrentCustomer.ParseAppliedCouponCodes(SystemCustomerAttributeNames.DiscountCoupons);
                    var existsAndUsed = false;
                    foreach (var item in coupons)
                    {
                        if (await _discountService.ExistsCodeInDiscount(item, discount.Id, null))
                            existsAndUsed = true;
                    }
                    if (!existsAndUsed)
                    {
                        if (!discount.Reused)
                            existsAndUsed = !await _discountService.ExistsCodeInDiscount(discountcouponcode, discount.Id, false);

                        if (!existsAndUsed)
                        {
                            var validationResult = await _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer, discountcouponcode);
                            if (validationResult.IsValid)
                            {
                                //valid
                                var applyCouponCode = _workContext.CurrentCustomer.ApplyCouponCode(SystemCustomerAttributeNames.DiscountCoupons, discountcouponcode);
                                //apply new value
                                await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCoupons, applyCouponCode);
                                message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");
                                isApplied = true;
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(validationResult.UserError))
                                {
                                    //some user error
                                    message = validationResult.UserError;
                                    isApplied = false;
                                }
                                else
                                {
                                    //general error text
                                    message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                                    isApplied = false;
                                }
                            }
                        }
                        else
                        {
                            message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WasUsed");
                            isApplied = false;
                        }
                    }
                    else
                    {
                        message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.UsesTheSameDiscount");
                        isApplied = false;
                    }
                }
                else
                {
                    message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                    isApplied = false;
                }
            }
            else
            {
                message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Required");
                isApplied = false;
            }

            var model = await _mediator.Send(new GetShoppingCart() {
                Cart = cart,
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                TaxDisplayType = _workContext.TaxDisplayType
            });

            model.DiscountBox.Message = message;
            model.DiscountBox.IsApplied = isApplied;

            return Json(new
            {
                cart = RenderViewComponentToString("OrderSummary", new { overriddenModel = model })
            });
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> ApplyGiftCard(string giftcardcouponcode)
        {
            //trim
            if (giftcardcouponcode != null)
                giftcardcouponcode = giftcardcouponcode.Trim();

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());

            var message = string.Empty;
            var isApplied = false;

            if (!cart.IsRecurring())
            {
                if (!string.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = (await _mediator.Send(new GetGiftCardQuery() { GiftCardCouponCode = giftcardcouponcode, IsGiftCardActivated = true })).FirstOrDefault();
                    bool isGiftCardValid = giftCard != null && giftCard.IsGiftCardValid();
                    if (isGiftCardValid)
                    {
                        var result = _workContext.CurrentCustomer.ApplyCouponCode(SystemCustomerAttributeNames.GiftCardCoupons, giftcardcouponcode.Trim().ToLower());
                        //apply new value
                        await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.GiftCardCoupons, result);

                        message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.Applied");
                        isApplied = true;
                    }
                    else
                    {
                        message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        isApplied = false;
                    }
                }
                else
                {
                    message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.Required");
                    isApplied = false;
                }
            }
            else
            {
                message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                isApplied = false;
            }

            var model = await _mediator.Send(new GetShoppingCart() {
                Cart = cart,
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                TaxDisplayType = _workContext.TaxDisplayType
            });

            model.GiftCardBox.Message = message;
            model.GiftCardBox.IsApplied = isApplied;

            return Json(new
            {
                cart = RenderViewComponentToString("OrderSummary", new { overriddenModel = model })
            });
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> GetEstimateShipping(string countryId, string stateProvinceId, string zipPostalCode, IFormCollection form)
        {
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart, ShoppingCartType.Auctions);

            var model = await _mediator.Send(new GetEstimateShippingResult() {
                Cart = cart,
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore,
                CountryId = countryId,
                StateProvinceId = stateProvinceId,
                ZipPostalCode = zipPostalCode
            });

            return PartialView("_EstimateShippingResult", model);
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> RemoveDiscountCoupon(string discountId)
        {
            var discount = await _discountService.GetDiscountById(discountId);
            if (discount != null)
            {
                var coupons = _workContext.CurrentCustomer.ParseAppliedCouponCodes(SystemCustomerAttributeNames.DiscountCoupons);
                foreach (var item in coupons)
                {
                    var dd = await _discountService.GetDiscountByCouponCode(item);
                    if (dd.Id == discount.Id)
                    {
                        //remove coupon
                        var result = _workContext.CurrentCustomer.RemoveCouponCode(SystemCustomerAttributeNames.DiscountCoupons, item);
                        await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCoupons, result);
                    }
                }
            }
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());

            var model = await _mediator.Send(new GetShoppingCart() {
                Cart = cart,
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                TaxDisplayType = _workContext.TaxDisplayType
            });

            return Json(new
            {
                cart = RenderViewComponentToString("OrderSummary", new { overriddenModel = model })
            });
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> RemoveGiftCardCode(string giftCardId, [FromServices] IGiftCardService giftCardService)
        {
            if (!string.IsNullOrEmpty(giftCardId))
            {
                //remove card
                var giftcard = await giftCardService.GetGiftCardById(giftCardId);
                if (giftcard != null)
                {
                    var result = _workContext.CurrentCustomer.RemoveCouponCode(SystemCustomerAttributeNames.GiftCardCoupons, giftcard.GiftCardCouponCode);
                    await _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.GiftCardCoupons, result);
                }
            }
            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, PrepareCartTypes());

            var model = await _mediator.Send(new GetShoppingCart() {
                Cart = cart,
                Customer = _workContext.CurrentCustomer,
                Currency = _workContext.WorkingCurrency,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore,
                TaxDisplayType = _workContext.TaxDisplayType
            });

            return Json(new
            {
                cart = RenderViewComponentToString("OrderSummary", new { overriddenModel = model })
            });

        }
        #endregion

        #region Wishlist

        public virtual async Task<IActionResult> Wishlist(Guid? customerGuid)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            Customer customer = customerGuid.HasValue ?
                await _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (customer == null)
                return RedirectToRoute("HomePage");

            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist);

            if (!string.IsNullOrEmpty(_storeContext.CurrentStore.Id))
                cart = cart.LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, _storeContext.CurrentStore.Id);

            var model = await _mediator.Send(new GetWishlist() {
                Cart = cart.ToList(),
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Currency = _workContext.WorkingCurrency,
                Store = _storeContext.CurrentStore,
                IsEditable = !customerGuid.HasValue,
                TaxDisplayType = _workContext.TaxDisplayType
            });

            return View(model);
        }

        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public virtual async Task<IActionResult> UpdateWishlist(IFormCollection form)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var customer = _workContext.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.Wishlist);

            var allIdsToRemove = !string.IsNullOrEmpty(form["removefromcart"].ToString())
                ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x)
                .ToList()
                : new List<string>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<string, IList<string>>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    await _shoppingCartService.DeleteShoppingCartItem(customer, sci);
                else
                {
                    foreach (string formKey in form.Keys)
                        if (formKey.Equals(string.Format("itemquantity{0}", sci.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(form[formKey], out int newQuantity))
                            {
                                var currSciWarnings = await _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                    sci.Id, sci.WarehouseId, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }
                            break;
                        }
                }
            }

            cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.Wishlist);
            var model = await _mediator.Send(new GetWishlist() {
                Cart = cart,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Currency = _workContext.WorkingCurrency,
                Store = _storeContext.CurrentStore,
                TaxDisplayType = _workContext.TaxDisplayType
            });

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
        public virtual async Task<IActionResult> AddItemsToCartFromWishlist(Guid? customerGuid, IFormCollection form)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var pageCustomer = customerGuid.HasValue
                ? await _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (pageCustomer == null)
                return RedirectToRoute("HomePage");

            var pageCart = pageCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist);
            if (!string.IsNullOrEmpty(_storeContext.CurrentStore.Id))
                pageCart = pageCart.LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, _storeContext.CurrentStore.Id);

            var allWarnings = new List<string>();
            var numberOfAddedItems = 0;
            var allIdsToAdd = form.ContainsKey("addtocart") ? form["addtocart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x)
                .ToList()
                : new List<string>();
            foreach (var sci in pageCart.ToList())
            {
                if (allIdsToAdd.Contains(sci.Id))
                {
                    var warnings = await _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                        sci.ProductId, ShoppingCartType.ShoppingCart,
                        _storeContext.CurrentStore.Id, sci.WarehouseId,
                        sci.AttributesXml, sci.CustomerEnteredPrice,
                        sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, true);
                    if (!warnings.Any())
                        numberOfAddedItems++;
                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        !warnings.Any()) //no warnings ( already in the cart)
                    {
                        //let's remove the item from wishlist
                        await _shoppingCartService.DeleteShoppingCartItem(_workContext.CurrentCustomer, sci);
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
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.Wishlist);
                var model = await _mediator.Send(new GetWishlist() {
                    Cart = cart,
                    Customer = _workContext.CurrentCustomer,
                    Language = _workContext.WorkingLanguage,
                    Currency = _workContext.WorkingCurrency,
                    Store = _storeContext.CurrentStore,
                    TaxDisplayType = _workContext.TaxDisplayType,
                    IsEditable = !customerGuid.HasValue
                });
                return View(model);
            }
        }

        public virtual async Task<IActionResult> EmailWishlist([FromServices] CaptchaSettings captchaSettings)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.Wishlist);

            if (!cart.Any())
                return RedirectToRoute("HomePage");

            var model = new WishlistEmailAFriendModel {
                YourEmailAddress = _workContext.CurrentCustomer.Email,
                DisplayCaptcha = captchaSettings.Enabled && captchaSettings.ShowOnEmailWishlistToFriendPage
            };
            return View(model);
        }

        [HttpPost, ActionName("EmailWishlist")]
        [FormValueRequired("send-email")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> EmailWishlistSend(WishlistEmailAFriendModel model, bool captchaValid,
            [FromServices] IWorkflowMessageService workflowMessageService,
            [FromServices] CaptchaSettings captchaSettings)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

            var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.Wishlist);
            if (!cart.Any())
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (captchaSettings.Enabled && captchaSettings.ShowOnEmailWishlistToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //check whether the current customer is guest and ia allowed to email wishlist
            if (_workContext.CurrentCustomer.IsGuest() && !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Wishlist.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                await workflowMessageService.SendWishlistEmailAFriendMessage(_workContext.CurrentCustomer, _storeContext.CurrentStore,
                        _workContext.WorkingLanguage.Id, model.YourEmailAddress,
                        model.FriendEmail, Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Wishlist.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.DisplayCaptcha = captchaSettings.Enabled && captchaSettings.ShowOnEmailWishlistToFriendPage;
            return View(model);
        }

        #endregion
    }
}
