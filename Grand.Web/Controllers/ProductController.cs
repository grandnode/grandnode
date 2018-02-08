using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Framework.Controllers;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Web.Models.Catalog;
using Grand.Core.Infrastructure;
using Grand.Services.Common;
using Grand.Web.Services;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Rss;
using Grand.Framework.Mvc;
using Microsoft.Net.Http.Headers;

namespace Grand.Web.Controllers
{
    public partial class ProductController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductWebService _productWebService;
        private readonly IProductReservationService _productReservationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IOrderReportService _orderReportService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly IEventPublisher _eventPublisher;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IOrderService _orderService;

        #endregion

        #region Constructors

        public ProductController(
            IProductService productService,
            IProductWebService productWebService,
            IProductReservationService productReservationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICompareProductsService compareProductsService,
            IOrderReportService orderReportService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            ICacheManager cacheManager,
            IOrderService orderService
            )
        {
            this._productService = productService;
            this._productWebService = productWebService;
            this._productReservationService = productReservationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._compareProductsService = compareProductsService;
            this._orderReportService = orderReportService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._customerActionEventService = customerActionEventService;
            this._eventPublisher = eventPublisher;
            this._catalogSettings = catalogSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._cacheManager = cacheManager;
            this._orderService = orderService;
        }

        #endregion

        #region Product details page

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductDetails(string productId, string updatecartitemid = "")
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return InvokeHttp404();
            //published?
            if (!_catalogSettings.AllowViewUnpublishedProductPage)
            {
                //Check whether the current user has a "Manage catalog" permission
                //It allows him to preview a product before publishing
                if (!product.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                    return InvokeHttp404();
            }

            //ACL (access control list)
            if (!_aclService.Authorize(product))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(product))
                return InvokeHttp404();

            //availability dates
            if (!product.IsAvailable() && !(product.ProductType == ProductType.Auction))
                return InvokeHttp404();
            
            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("HomePage");

                return RedirectToRoute("Product", new { SeName = parentGroupedProduct.GetSeName() });
            }
            var customer = _workContext.CurrentCustomer;
            //update existing shopping cart item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && !String.IsNullOrEmpty(updatecartitemid))
            {
                var cart = customer.ShoppingCartItems
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
            }

            //prepare the model
            var model = _productWebService.PrepareProductDetailsPage(product, updatecartitem, false);

            //product template
            var productTemplateViewPath = _productWebService.PrepareProductTemplateViewPath(product.ProductTemplateId);

            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(customer.Id, product.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) &&
                _permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor == null || _workContext.CurrentVendor.Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new { id = product.Id, area = "Admin" }));
                }
            }

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewProduct", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);
            _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");
            _productService.UpdateMostView(productId, 1);

            return View(productTemplateViewPath, model);
        }

        #endregion

        #region Recently viewed products

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_workContext.CurrentCustomer.Id, _catalogSettings.RecentlyViewedProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productWebService.PrepareProductOverviewModels(products));

            return View(model);
        }

        #endregion

        #region Recently added products

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult NewProducts()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return Content("");

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productWebService.PrepareProductOverviewModels(products));

            return View(model);
        }

        public virtual IActionResult NewProductsRss()
        {
            var feed = new RssFeed(
                                    string.Format("{0}: New products", _storeContext.CurrentStore.GetLocalized(x => x.Name)),
                                    "Information about products",
                                    new Uri(_webHelper.GetStoreLocation(false)),
                                    DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);
            foreach (var product in products)
            {
                string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                string productName = product.GetLocalized(x => x.Name);
                string productDescription = product.GetLocalized(x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), String.Format("urn:store:{0}:newProducts:product:{1}", _storeContext.CurrentStore.Id, product.Id), product.CreatedOnUtc);
                items.Add(item);
            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        #endregion

        #region Product reviews

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductReviews(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = new ProductReviewsModel();
            _productWebService.PrepareProductReviewsModel(model, product);
            //only registered users can leave reviews
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
            return View(model);
        }

        [HttpPost, ActionName("ProductReviews")]
        [FormValueRequired("add-review")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult ProductReviewsAdd(string productId, ProductReviewsModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            if (_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing &&
                    !_orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, productId: productId, os: OrderStatus.Complete).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                var productReview = _productWebService.InsertProductReview(product, model);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddProductReview", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product.Name);

                //raise event
                if (productReview.IsApproved)
                    _eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));

                _productWebService.PrepareProductReviewsModel(model, product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                model.AddProductReview.SuccessfullyAdded = true;
                if (!productReview.IsApproved)
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SeeAfterApproving");
                else
                    model.AddProductReview.Result = _localizationService.GetResource("Reviews.SuccessfullyAdded");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            _productWebService.PrepareProductReviewsModel(model, product);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult SetProductReviewHelpfulness(string productReviewId, string productId, bool washelpful)
        {
            var product = _productService.GetProductById(productId);
            var productReview = _productService.GetProductReviewById(productReviewId);
            if (productReview == null)
                throw new ArgumentException("No product review found with the specified id");

            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == _workContext.CurrentCustomer.Id)
            {
                return Json(new
                {
                    Result = _localizationService.GetResource("Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //delete previous helpfulness
            var prh = productReview.ProductReviewHelpfulnessEntries
                .FirstOrDefault(x => x.CustomerId == _workContext.CurrentCustomer.Id);
            if (prh != null)
            {
                //existing one
                prh.WasHelpful = washelpful;
            }
            else
            {
                //insert new helpfulness
                prh = new ProductReviewHelpfulness
                {
                    ProductReviewId = productReview.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    WasHelpful = washelpful,
                };
                productReview.ProductReviewHelpfulnessEntries.Add(prh);
                if (!_workContext.CurrentCustomer.IsHasProductReviewH)
                {
                    _workContext.CurrentCustomer.IsHasProductReviewH = true;
                    EngineContext.Current.Resolve<ICustomerService>().UpdateHasProductReviewH(_workContext.CurrentCustomer.Id);
                }

            }

            //new totals
            productReview.HelpfulYesTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => x.WasHelpful);
            productReview.HelpfulNoTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => !x.WasHelpful);
            _productService.UpdateProductReview(productReview);

            return Json(new
            {
                Result = _localizationService.GetResource("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        #endregion

        #region Email a friend
        
        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductEmailAFriend(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            var model = new ProductEmailAFriendModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.YourEmailAddress = _workContext.CurrentCustomer.Email;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
            return View(model);
        }

        [HttpPost, ActionName("ProductEmailAFriend")]
        [FormValueRequired("send-email")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult ProductEmailAFriendSend(ProductEmailAFriendModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            //check whether the current customer is guest and ia allowed to email a friend
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Products.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                _productWebService.SendProductEmailAFriendMessage(product, model);

                model.ProductId = product.Id;
                model.ProductName = product.GetLocalized(x => x.Name);
                model.ProductSeName = product.GetSeName();

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Products.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
            return View(model);
        }

        #endregion

        #region Ask question

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult AskQuestion(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null ||  !product.Published || !_catalogSettings.AskQuestionEnabled)
                return NotFound();

            var customer = _workContext.CurrentCustomer;

            var model = new ProductAskQuestionModel();
            model.Id = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.Email = customer.Email;
            model.FullName = customer.GetFullName();
            model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
            model.Message = "";
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;

            return View(model);
        }

        [HttpPost, ActionName("AskQuestion")]
        [FormValueRequired("send-email")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult AskQuestion(ProductAskQuestionModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null || !product.Published || !_catalogSettings.AskQuestionEnabled)
                return NotFound();

            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                // email
                _productWebService.SendProductAskQuestionMessage(product, model);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AskQuestion", _workContext.CurrentCustomer.Id, _localizationService.GetResource("ActivityLog.PublicStore.AskQuestion"));

                model.SuccessfullySent = true;
                model.ProductSeName= product.GetSeName();
                model.ProductName = product.GetLocalized(x => x.Name);
                model.Result = _localizationService.GetResource("Products.AskQuestion.SuccessfullySent");
                return View(model);
            }

            // If we got this far, something failed, redisplay form
            var customer = _workContext.CurrentCustomer;
            model.Id = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;
            return View(model);
        }

        #endregion

        #region Comparing products

        [HttpPost]
        public virtual IActionResult AddProductToCompareList(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.Published)
                return Json(new
                {
                    success = false,
                    comparemessage = "No product found with the specified ID"
                });

            if (!_catalogSettings.CompareProductsEnabled)
                return Json(new
                {
                    success = false,
                    comparemessage = "Product comparison is disabled"
                });

            _compareProductsService.AddProductToCompareList(productId);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddToCompareList", productId, _localizationService.GetResource("ActivityLog.PublicStore.AddToCompareList"), product.Name);
            
            return Json(new
            {
                success = true,
                comparemessage = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToCompareList.Link"), Url.RouteUrl("CompareProducts"))
                //use the code below (commented) if you want a customer to be automatically redirected to the compare products page
                //redirect = Url.RouteUrl("CompareProducts"),
            });
        }

        public virtual IActionResult RemoveProductFromCompareList(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return RedirectToRoute("HomePage");

            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.RemoveProductFromCompareList(productId);

            return RedirectToRoute("CompareProducts");
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            var products = _compareProductsService.GetComparedProducts();

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            //prepare model
            _productWebService.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
                .ToList()
                .ForEach(model.Products.Add);
            return View(model);
        }

        public virtual IActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.ClearCompareProducts();

            return RedirectToRoute("CompareProducts");
        }

        public IActionResult GetDatesForMonth(string productId, int month, string parameter, int year)
        {
            var allReservations = _productReservationService.GetProductReservationsByProductId(productId, true, null);
            var query = allReservations.Where(x => x.Date.Month == month && x.Date.Year == year && x.Date >= DateTime.UtcNow);
            if (!string.IsNullOrEmpty(parameter))
            {
                query = query.Where(x => x.Parameter == parameter);
            }

            var reservations = query.ToList();
            var inCart = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => !string.IsNullOrEmpty(x.ReservationId)).ToList();
            foreach (var cartItem in inCart)
            {
                var matching = reservations.Where(x => x.Id == cartItem.ReservationId);
                if (matching.Any())
                {
                    reservations.Remove(matching.First());
                }
            }

            var toReturn = reservations.GroupBy(x => x.Date).Select(x => x.First()).ToList();

            return Json(toReturn);
        }
        #endregion
    }
}
