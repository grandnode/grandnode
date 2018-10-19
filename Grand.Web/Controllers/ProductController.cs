using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Infrastructure;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Rss;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Models.Catalog;
using Grand.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Grand.Web.Controllers
{
    public partial class ProductController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductViewModelService _productViewModelService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public ProductController(
            IProductService productService,
            IProductViewModelService productViewModelService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICompareProductsService compareProductsService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            CaptchaSettings captchaSettings
        )
        {
            this._productService = productService;
            this._productViewModelService = productViewModelService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._compareProductsService = compareProductsService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._customerActionEventService = customerActionEventService;
            this._catalogSettings = catalogSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._captchaSettings = captchaSettings;
        }

        #endregion

        #region Product details page

        public virtual IActionResult ProductDetails(string productId, string updatecartitemid = "")
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //published?
            if (!_catalogSettings.AllowViewUnpublishedProductPage)
            {
                //Check whether the current user has a "Manage catalog" permission
                //It allows him to preview a product before publishing
                if (!product.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts, customer))
                    return InvokeHttp404();
            }

            //ACL (access control list)
            if (!_aclService.Authorize(product, customer))
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
            var model = _productViewModelService.PrepareProductDetailsPage(product, updatecartitem, false);

            //product template
            var productTemplateViewPath = _productViewModelService.PrepareProductTemplateViewPath(product.ProductTemplateId);

            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(customer.Id, product.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) &&
                _permissionService.Authorize(StandardPermissionProvider.ManageProducts, customer))
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

        //handle product attribute selection event. this way we return new price, overridden gtin/sku/mpn
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual IActionResult ProductDetails_AttributeChange(string productId, bool validateAttributeConditions, bool loadPicture, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var model = _productViewModelService.PrepareProductDetailsAttributeChangeModel(product, validateAttributeConditions, loadPicture, form);

            return Json(new
            {
                gtin = model.Gtin,
                mpn = model.Mpn,
                sku = model.Sku,
                price = model.Price,
                stockAvailability = model.StockAvailability,
                enabledattributemappingids = model.EnabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = model.DisabledAttributeMappingids.ToArray(),
                pictureFullSizeUrl = model.PictureFullSizeUrl,
                pictureDefaultSizeUrl = model.PictureDefaultSizeUrl,
            });
        }


        [HttpPost]
        public virtual IActionResult UploadFileProductAttribute(string attributeId, string productId,
            [FromServices] IDownloadService downloadService)
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
            downloadService.InsertDownload(download);

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



        #region Quick view product

        public virtual IActionResult QuickView(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            var customer = _workContext.CurrentCustomer;

            //published?
            if (!_catalogSettings.AllowViewUnpublishedProductPage)
            {
                //Check whether the current user has a "Manage catalog" permission
                //It allows him to preview a product before publishing
                if (!product.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts, customer))
                    return Json(new
                    {
                        success = false,
                        message = "No product found with the specified ID"
                    });
            }

            //ACL (access control list)
            if (!_aclService.Authorize(product, customer))
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //Store mapping
            if (!_storeMappingService.Authorize(product))
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //availability dates
            if (!product.IsAvailable() && !(product.ProductType == ProductType.Auction))
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                {
                    return Json(new
                    {
                        redirect = Url.RouteUrl("HomePage"),
                    });
                }
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //prepare the model
            var model = _productViewModelService.PrepareProductDetailsPage(product, null, false);

            //product template
            var productTemplateViewPath = _productViewModelService.PrepareProductTemplateViewPath(product.ProductTemplateId);

            //save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(customer.Id, product.Id);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewProduct", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);
            _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");
            _productService.UpdateMostView(productId, 1);
            var qhtml = this.RenderPartialViewToString(productTemplateViewPath + ".QuickView", model);
            return Json(new
            {
                success = true,
                product = true,
                html = qhtml,
            });
        }
        #endregion

        #endregion

        #region Add product to cart

        
        #endregion

        #region Recently viewed products

        public virtual IActionResult RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_workContext.CurrentCustomer.Id, _catalogSettings.RecentlyViewedProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productViewModelService.PrepareProductOverviewModels(products));

            return View(model);
        }

        #endregion

        #region Recently added products
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
            model.AddRange(_productViewModelService.PrepareProductOverviewModels(products));

            return View(model);
        }

        public virtual IActionResult NewProductsRss([FromServices] IWebHelper webHelper)
        {
            var feed = new RssFeed(
                                    string.Format("{0}: New products", _storeContext.CurrentStore.GetLocalized(x => x.Name)),
                                    "Information about products",
                                    new Uri(webHelper.GetStoreLocation(false)),
                                    DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);
            foreach (var product in products)
            {
                string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                string productName = product.GetLocalized(x => x.Name);
                string productDescription = product.GetLocalized(x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), String.Format("urn:store:{0}:newProducts:product:{1}", _storeContext.CurrentStore.Id, product.Id), product.CreatedOnUtc);
                items.Add(item);
            }
            feed.Items = items;
            return new RssActionResult(feed, webHelper.GetThisPageUrl(false));
        }

        #endregion

        #region Product reviews
        public virtual IActionResult ProductReviews(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = new ProductReviewsModel();
            _productViewModelService.PrepareProductReviewsModel(model, product);
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
        public virtual IActionResult ProductReviewsAdd(string productId, ProductReviewsModel model, bool captchaValid,
            [FromServices] IOrderService orderService, [FromServices] IEventPublisher eventPublisher)
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
                    !orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, productId: productId, os: OrderStatus.Complete).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                var productReview = _productViewModelService.InsertProductReview(product, model);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddProductReview", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product.Name);

                //raise event
                if (productReview.IsApproved)
                    eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));

                _productViewModelService.PrepareProductReviewsModel(model, product);
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
            _productViewModelService.PrepareProductReviewsModel(model, product);
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
                _productService.UpdateProductReview(productReview);
                if (!_workContext.CurrentCustomer.HasContributions)
                {
                    EngineContext.Current.Resolve<ICustomerService>().UpdateContributions(_workContext.CurrentCustomer);
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
                _productViewModelService.SendProductEmailAFriendMessage(product, model);

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

        public virtual IActionResult AskQuestion(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null ||  !product.Published || !_catalogSettings.AskQuestionEnabled)
                return NotFound();

            var model = _productViewModelService.PrepareProductAskQuestionModel(product);

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
                _productViewModelService.SendProductAskQuestionMessage(product, model);

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

        [HttpPost]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult AskQuestionOnProduct(ProductAskQuestionSimpleModel model, bool captchaValid)
        {
            var product = _productService.GetProductById(model.Id);
            if (product == null || !product.Published || !_catalogSettings.AskQuestionOnProduct)
                return Json(new
                {
                    success = false,
                    message = "Product not found"
                }); 

            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage && !captchaValid)
            {
                return Json(new
                {
                    success = false,
                    message = _captchaSettings.GetWrongCaptchaMessage(_localizationService)
                });
            }

            if (ModelState.IsValid)
            {
                var productaskqestionmodel = new ProductAskQuestionModel() {
                    Email = model.AskQuestionEmail,
                    FullName = model.AskQuestionFullName,
                    Phone = model.AskQuestionPhone,
                    Message = model.AskQuestionMessage
                };
                // email
                _productViewModelService.SendProductAskQuestionMessage(product, productaskqestionmodel);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.AskQuestion", _workContext.CurrentCustomer.Id, _localizationService.GetResource("ActivityLog.PublicStore.AskQuestion"));
                //return Json
                return Json(new
                {
                    success = true,
                    message = _localizationService.GetResource("Products.AskQuestion.SuccessfullySent")
                });

            }

            // If we got this far, something failed, redisplay form
            return Json(new
            {
                success = false,
                message = string.Join(",", ModelState.Values.SelectMany(v => v.Errors).Select(x => x.ErrorMessage))
            });

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

            if(product.ProductType == ProductType.Auction || product.ProductType == ProductType.Reservation)
                return Json(new
                {
                    success = false,
                    comparemessage = _localizationService.GetResource("Products.ProductCantAddToCompareList")
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
            _productViewModelService.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
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

        public IActionResult GetDatesForMonth(string productId, int month, string parameter, int year, [FromServices] IProductReservationService productReservationService)
        {
            var allReservations = productReservationService.GetProductReservationsByProductId(productId, true, null);
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
