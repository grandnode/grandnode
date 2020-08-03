using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Rss;
using Grand.Framework.Security.Captcha;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Notifications.Catalog;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Commands.Models.Products;
using Grand.Web.Events;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class ProductController : BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public ProductController(
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IShoppingCartService shoppingCartService,
            ICompareProductsService compareProductsService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            IMediator mediator,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            CaptchaSettings captchaSettings
        )
        {
            _productService = productService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _shoppingCartService = shoppingCartService;
            _compareProductsService = compareProductsService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _customerActivityService = customerActivityService;
            _customerActionEventService = customerActionEventService;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _captchaSettings = captchaSettings;
        }

        #endregion

        #region Product details page

        public virtual async Task<IActionResult> ProductDetails(string productId, string updatecartitemid = "")
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                return InvokeHttp404();

            var customer = _workContext.CurrentCustomer;

            //published?
            if (!_catalogSettings.AllowViewUnpublishedProductPage)
            {
                //Check whether the current user has a "Manage catalog" permission
                //It allows him to preview a product before publishing
                if (!product.Published && !await _permissionService.Authorize(StandardPermissionProvider.ManageProducts, customer))
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
                var parentGroupedProduct = await _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("HomePage");

                return RedirectToRoute("Product", new { SeName = parentGroupedProduct.GetSeName(_workContext.WorkingLanguage.Id) });
            }
            //update existing shopping cart item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && !String.IsNullOrEmpty(updatecartitemid))
            {
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id);

                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) });
                }
            }

            //prepare the model
            var model = await _mediator.Send(new GetProductDetailsPage() {
                Store = _storeContext.CurrentStore,
                Product = product,
                IsAssociatedProduct = false,
                UpdateCartItem = updatecartitem
            });

            //product template
            var productTemplateViewPath = await _mediator.Send(new GetProductTemplateViewPath() { ProductTemplateId = product.ProductTemplateId });

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedList(customer.Id, product.Id);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel, customer) &&
                await _permissionService.Authorize(StandardPermissionProvider.ManageProducts, customer))
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor == null || _workContext.CurrentVendor.Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new { id = product.Id, area = "Admin" }));
                }
            }
            
            //activity log
            await _customerActivityService.InsertActivity("PublicStore.ViewProduct", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);
            await _customerActionEventService.Viewed(customer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");
            await _productService.UpdateMostView(productId, 1);

            return View(productTemplateViewPath, model);
        }

        //handle product attribute selection event. this way we return new price, overridden gtin/sku/mpn
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual async Task<IActionResult> ProductDetails_AttributeChange(string productId, bool validateAttributeConditions, bool loadPicture, IFormCollection form)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var model = await _mediator.Send(new GetProductDetailsAttributeChange() {
                Currency = _workContext.WorkingCurrency,
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore,
                Form = form,
                LoadPicture = loadPicture,
                Product = product,
                ValidateAttributeConditions = validateAttributeConditions
            });

            return Json(new
            {
                gtin = model.Gtin,
                mpn = model.Mpn,
                sku = model.Sku,
                price = model.Price,
                stockAvailability = model.StockAvailability,
                backInStockSubscription = model.DisplayBackInStockSubscription,
                buttonTextBackInStockSubscription = model.ButtonTextBackInStockSubscription,
                enabledattributemappingids = model.EnabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = model.DisabledAttributeMappingids.ToArray(),
                pictureFullSizeUrl = model.PictureFullSizeUrl,
                pictureDefaultSizeUrl = model.PictureDefaultSizeUrl,
            });
        }

        //handle product warehouse selection event. this way we return stock
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual async Task<IActionResult> ProductDetails_WarehouseChange(string productId, string warehouseId, [FromServices] IProductAttributeParser productAttributeParser)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var stock = product.FormatStockMessage(warehouseId, "", _localizationService, productAttributeParser);
            return Json(new
            {
                stockAvailability = stock
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> UploadFileProductAttribute(string attributeId, string productId,
            [FromServices] IDownloadService downloadService)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }
            var attribute = product.ProductAttributeMappings.Where(x => x.Id == attributeId).FirstOrDefault();
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

        #region Quick view product

        public virtual async Task<IActionResult> QuickView(string productId)
        {
            var product = await _productService.GetProductById(productId);
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
                if (!product.Published && !await _permissionService.Authorize(StandardPermissionProvider.ManageProducts, customer))
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
                var parentGroupedProduct = await _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                {
                    return Json(new
                    {
                        redirect = Url.RouteUrl("HomePage"),
                    });
                }
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }),
                });
            }

            //prepare the model
            var model = await _mediator.Send(new GetProductDetailsPage() {
                Store = _storeContext.CurrentStore,
                Product = product,
                IsAssociatedProduct = false,
                UpdateCartItem = null
            });

            //product template
            var productTemplateViewPath = await _mediator.Send(new GetProductTemplateViewPath() { ProductTemplateId = product.ProductTemplateId });

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedList(customer.Id, product.Id);

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.ViewProduct", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewProduct"), product.Name);
            await _customerActionEventService.Viewed(customer, HttpContext.Request.Path.ToString(), Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");
            await _productService.UpdateMostView(productId, 1);
            var qhtml = await RenderPartialViewToString(productTemplateViewPath + ".QuickView", model);
            return Json(new
            {
                success = true,
                product = true,
                html = qhtml,
            });
        }
        #endregion

        #endregion

        #region Recently viewed products

        public virtual async Task<IActionResult> RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var products = await _recentlyViewedProductsService.GetRecentlyViewedProducts(_workContext.CurrentCustomer.Id, _catalogSettings.RecentlyViewedProductsNumber);

            //prepare model
            var model = await _mediator.Send(new GetProductOverview() {
                Products = products,
            });

            return View(model);
        }

        #endregion

        #region Recently added products

        public virtual async Task<IActionResult> NewProducts()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return Content("");

            var products = (await _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber)).products;


            //prepare model
            var model = await _mediator.Send(new GetProductOverview() {
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages,
                Products = products,
            });

            return View(model);
        }

        public virtual async Task<IActionResult> NewProductsRss([FromServices] IWebHelper webHelper)
        {
            var feed = new RssFeed(
                                    string.Format("{0}: New products", _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)),
                                    "Information about products",
                                    new Uri(webHelper.GetStoreLocation(false)),
                                    DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var products = (await _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber)).products;
            foreach (var product in products)
            {
                string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName(_workContext.WorkingLanguage.Id) }, webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                string productName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
                string productDescription = product.GetLocalized(x => x.ShortDescription, _workContext.WorkingLanguage.Id);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), String.Format("urn:store:{0}:newProducts:product:{1}", _storeContext.CurrentStore.Id, product.Id), product.CreatedOnUtc);
                items.Add(item);
            }
            feed.Items = items;
            return new RssActionResult(feed, webHelper.GetThisPageUrl(false));
        }

        #endregion

        #region Product reviews
        public virtual async Task<IActionResult> ProductReviews(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetProductReviews() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Product = product,
                Store = _storeContext.CurrentStore,
                Size = _catalogSettings.NumberOfReview
            });

            //only registered users can leave reviews
            if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError("", _localizationService.GetResource("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
            return View(model);
        }

        [HttpPost, ActionName("ProductReviews")]
        [FormValueRequired("add-review")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ProductReviewsAdd(string productId, ProductReviewsModel model, bool captchaValid,
            [FromServices] IOrderService orderService)
        {
            var product = await _productService.GetProductById(productId);
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
                    !(await orderService.SearchOrders(customerId: _workContext.CurrentCustomer.Id, productId: productId, os: OrderStatus.Complete)).Any())
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));

            if (ModelState.IsValid)
            {
                var productReview = await _mediator.Send(new InsertProductReviewCommand() {
                    Customer = _workContext.CurrentCustomer,
                    Store = _storeContext.CurrentStore,
                    Model = model,
                    Product = product
                });

                //notification
                await _mediator.Publish(new ProductReviewEvent(product, model.AddProductReview));

                await _customerActivityService.InsertActivity("PublicStore.AddProductReview", product.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product.Name);

                //raise event
                if (productReview.IsApproved)
                    await _mediator.Publish(new ProductReviewApprovedEvent(productReview));

                model = await _mediator.Send(new GetProductReviews() {
                    Customer = _workContext.CurrentCustomer,
                    Language = _workContext.WorkingLanguage,
                    Product = product,
                    Store = _storeContext.CurrentStore,
                    Size = _catalogSettings.NumberOfReview
                });

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
            var newmodel = await _mediator.Send(new GetProductReviews() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Product = product,
                Store = _storeContext.CurrentStore,
                Size = _catalogSettings.NumberOfReview
            });

            newmodel.AddProductReview.Rating = model.AddProductReview.Rating;
            newmodel.AddProductReview.ReviewText = model.AddProductReview.ReviewText;
            newmodel.AddProductReview.Title = model.AddProductReview.Title;

            return View(newmodel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SetProductReviewHelpfulness(string productReviewId, string productId, bool washelpful, 
            [FromServices] ICustomerService customerService, [FromServices] IProductReviewService productReviewService)
        {
            var product = await _productService.GetProductById(productId);
            var productReview = await productReviewService.GetProductReviewById(productReviewId);
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
                prh = new ProductReviewHelpfulness {
                    ProductReviewId = productReview.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    WasHelpful = washelpful,
                };
                productReview.ProductReviewHelpfulnessEntries.Add(prh);
                await productReviewService.UpdateProductReview(productReview);
                if (!_workContext.CurrentCustomer.HasContributions)
                {
                    await customerService.UpdateContributions(_workContext.CurrentCustomer);
                }

            }

            //new totals
            productReview.HelpfulYesTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => x.WasHelpful);
            productReview.HelpfulNoTotal = productReview.ProductReviewHelpfulnessEntries.Count(x => !x.WasHelpful);
            await productReviewService.UpdateProductReview(productReview);

            return Json(new
            {
                Result = _localizationService.GetResource("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        #endregion

        #region Email a friend

        public virtual async Task<IActionResult> ProductEmailAFriend(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("HomePage");

            var model = new ProductEmailAFriendModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
            model.YourEmailAddress = _workContext.CurrentCustomer.Email;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
            return View(model);
        }

        [HttpPost, ActionName("ProductEmailAFriend")]
        [FormValueRequired("send-email")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ProductEmailAFriendSend(ProductEmailAFriendModel model, bool captchaValid)
        {
            var product = await _productService.GetProductById(model.ProductId);
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
                await _mediator.Send(new SendProductEmailAFriendMessageCommand() {
                    Customer = _workContext.CurrentCustomer,
                    Product = product,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore,
                    Model = model,
                });

                model.ProductId = product.Id;
                model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
                model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Products.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
            return View(model);
        }

        #endregion

        #region Ask question

        public virtual async Task<IActionResult> AskQuestion(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null || !product.Published || !_catalogSettings.AskQuestionEnabled)
                return NotFound();

            var model = await _mediator.Send(new GetProductAskQuestion() {
                Product = product,
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });

            return View(model);
        }

        [HttpPost, ActionName("AskQuestion")]
        [FormValueRequired("send-email")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> AskQuestion(ProductAskQuestionModel model, bool captchaValid)
        {
            var product = await _productService.GetProductById(model.ProductId);
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
                await _mediator.Send(new SendProductAskQuestionMessageCommand() {
                    Customer = _workContext.CurrentCustomer,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore,
                    Model = model,
                    Product = product
                });

                //activity log
                await _customerActivityService.InsertActivity("PublicStore.AskQuestion", _workContext.CurrentCustomer.Id, _localizationService.GetResource("ActivityLog.PublicStore.AskQuestion"));

                model.SuccessfullySent = true;
                model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
                model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
                model.Result = _localizationService.GetResource("Products.AskQuestion.SuccessfullySent");
                return View(model);
            }

            // If we got this far, something failed, redisplay form
            var customer = _workContext.CurrentCustomer;
            model.Id = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> AskQuestionOnProduct(ProductAskQuestionSimpleModel model, bool captchaValid)
        {
            var product = await _productService.GetProductById(model.Id);
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
                await _mediator.Send(new SendProductAskQuestionMessageCommand() {
                    Customer = _workContext.CurrentCustomer,
                    Language = _workContext.WorkingLanguage,
                    Store = _storeContext.CurrentStore,
                    Model = productaskqestionmodel,
                    Product = product
                });

                //activity log
                await _customerActivityService.InsertActivity("PublicStore.AskQuestion", _workContext.CurrentCustomer.Id, _localizationService.GetResource("ActivityLog.PublicStore.AskQuestion"));
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
        public virtual async Task<IActionResult> AddProductToCompareList(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null || !product.Published)
                return Json(new
                {
                    success = false,
                    comparemessage = "No product found with the specified ID"
                });

            if (product.ProductType == ProductType.Auction || product.ProductType == ProductType.Reservation)
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
            await _customerActivityService.InsertActivity("PublicStore.AddToCompareList", productId, _localizationService.GetResource("ActivityLog.PublicStore.AddToCompareList"), product.Name);

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

        public virtual async Task<IActionResult> CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            var model = new CompareProductsModel {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            var products = await _compareProductsService.GetComparedProducts();

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            (await _mediator.Send(new GetProductOverview() {
                PrepareSpecificationAttributes = true,
                Products = products,
            })).ToList().ForEach(model.Products.Add);

            return View(model);

        }

        public virtual IActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            _compareProductsService.ClearCompareProducts();

            return RedirectToRoute("CompareProducts");
        }

        public virtual async Task<IActionResult> GetDatesForMonth(string productId, int month, string parameter, int year, [FromServices] IProductReservationService productReservationService)
        {
            var allReservations = await productReservationService.GetProductReservationsByProductId(productId, true, null);
            var query = allReservations.Where(x => x.Date.Month == month && x.Date.Year == year && x.Date >= DateTime.UtcNow);
            if (!string.IsNullOrEmpty(parameter))
            {
                query = query.Where(x => x.Parameter == parameter);
            }

            var reservations = query.ToList();
            var inCart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id)
                .Where(x => !string.IsNullOrEmpty(x.ReservationId)).ToList();
            foreach (var cartItem in inCart)
            {
                var match = reservations.FirstOrDefault(x => x.Id == cartItem.ReservationId);
                if (match != null)
                {
                    reservations.Remove(match);
                }
            }

            var toReturn = reservations.GroupBy(x => x.Date).Select(x => x.First()).ToList();

            return Json(toReturn);
        }
        #endregion
    }
}
