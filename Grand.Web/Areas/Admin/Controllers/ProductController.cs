using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.ExportImport;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Products)]
    public partial class ProductController : BaseAdminController
    {
        #region Fields

        private readonly IProductViewModelService _productViewModelService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly IStoreService _storeService;
        private readonly IProductReservationService _productReservationService;
        private readonly IAuctionService _auctionService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructors

        public ProductController(
            IProductViewModelService productViewModelService,
            IProductService productService,
            ICustomerService customerService,
            IWorkContext workContext,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IExportManager exportManager,
            IImportManager importManager,
            IStoreService storeService,
            IProductReservationService productReservationService,
            IAuctionService auctionService,
            IDateTimeHelper dateTimeHelper)
        {
            _productViewModelService = productViewModelService;
            _productService = productService;
            _customerService = customerService;
            _workContext = workContext;
            _languageService = languageService;
            _localizationService = localizationService;
            _exportManager = exportManager;
            _importManager = importManager;
            _storeService = storeService;
            _productReservationService = productReservationService;
            _auctionService = auctionService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        protected (bool allow, string message) CheckAccessToProduct(Product product)
        {
            if (product == null)
            {
                return (false, "Product not exists");
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return (false, "This is not your product");
                }
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!(!product.LimitedToStores || (product.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && product.LimitedToStores)))
                    return (false, "This is not your product");
            }
            return (true, null);
        }

        #region Product list / create / edit / delete

        //list products
        public IActionResult Index() => RedirectToAction("List");

        public async Task<IActionResult> List()
        {
            var model = await _productViewModelService.PrepareProductListModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> ProductList(DataSourceRequest command, ProductListModel model)
        {
            var (productModels, totalCount) = await _productViewModelService.PrepareProductsModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = productModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public async Task<IActionResult> GoToSku(ProductListModel model)
        {
            string sku = model.GoDirectlyToSku;

            //try to load a product entity
            var product = await _productService.GetProductBySku(sku);
            if (product != null)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (!product.LimitedToStores || (product.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && product.LimitedToStores))
                        return RedirectToAction("Edit", new { id = product.Id });
                    else
                        return RedirectToAction("List", "Product");
                }

                return RedirectToAction("Edit", "Product", new { id = product.Id });
            }
            //not found
            WarningNotification(_localizationService.GetResource("Admin.Catalog.Products.List.SkuNotFound"));
            return RedirectToAction("List", "Product");
        }

        //create product
        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new ProductModel();
            await _productViewModelService.PrepareProductModel(model, null, true, true);
            await AddLocales(_languageService, model.Locales);
            await model.PrepareACLModel(null, false, _customerService);
            await model.PrepareStoresMappingModel(null, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(ProductModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var product = await _productViewModelService.InsertProductModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            await _productViewModelService.PrepareProductModel(model, null, false, true);
            await model.PrepareACLModel(null, true, _customerService);
            await model.PrepareStoresMappingModel(null, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        //edit product
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _productService.GetProductById(id, true);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!product.LimitedToStores || (product.LimitedToStores && product.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && product.Stores.Count > 1))
                    WarningNotification(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));
                else
                {
                    if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                        return RedirectToAction("List");
                }
            }

            var model = product.ToModel(_dateTimeHelper);
            model.Ticks = product.UpdatedOnUtc.Ticks;

            await _productViewModelService.PrepareProductModel(model, product, false, false);
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = product.GetSeName(languageId, false, false);
            });

            await model.PrepareACLModel(product, false, _customerService);
            await model.PrepareStoresMappingModel(product, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(ProductModel model, bool continueEditing)
        {
            var product = await _productService.GetProductById(model.Id, true);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = product.Id });
            }

            if (model.Ticks != product.UpdatedOnUtc.Ticks)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.Fields.ChangedWarning"));
                return RedirectToAction("Edit", new { id = product.Id });
            }
            if (ModelState.IsValid)
            {
                product = await _productViewModelService.UpdateProductModel(product, model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();
                    return RedirectToAction("Edit", new { id = product.Id });
                }
                return RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            await _productViewModelService.PrepareProductModel(model, product, false, true);
            await model.PrepareACLModel(product, true, _customerService);
            await model.PrepareStoresMappingModel(product, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }
        //delete product
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetProductById(id, true);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = product.Id });
            }

            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteProduct(product);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                await _productViewModelService.DeleteSelected(selectedIds.ToList());
            }

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        [HttpPost]
        public async Task<IActionResult> CopyProduct(ProductModel model, [FromServices] ICopyProductService copyProductService)
        {
            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = await _productService.GetProductById(copyModel.Id, true);

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && originalProduct.VendorId != _workContext.CurrentVendor.Id)
                    return RedirectToAction("List");

                if (_workContext.CurrentCustomer.IsStaff())
                {
                    originalProduct.LimitedToStores = true;
                    originalProduct.Stores.Clear();
                    originalProduct.Stores.Add(_workContext.CurrentCustomer.StaffStoreId);
                }

                var newProduct = await copyProductService.CopyProduct(originalProduct,
                    copyModel.Name, copyModel.Published, copyModel.CopyImages);
                SuccessNotification("The product has been copied successfully");
                return RedirectToAction("Edit", new { id = newProduct.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion

        #region Required products

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]

        public async Task<IActionResult> LoadProductFriendlyNames(string productIds)
        {
            var result = "";

            if (!String.IsNullOrWhiteSpace(productIds))
            {
                var ids = new List<string>();
                var rangeArray = productIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                foreach (string str1 in rangeArray)
                {
                    ids.Add(str1);
                }

                var products = await _productService.GetProductsByIds(ids.ToArray(), true);
                for (int i = 0; i <= products.Count - 1; i++)
                {
                    result += products[i].Name;
                    if (i != products.Count - 1)
                        result += ", ";
                }
            }

            return Json(new { Text = result });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> RequiredProductAddPopup(string productIdsInput)
        {
            var model = await _productViewModelService.PrepareAddRequiredProductModel();
            ViewBag.productIdsInput = productIdsInput;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> RequiredProductAddPopupList(DataSourceRequest command, ProductModel.AddRequiredProductModel model)
        {
            var (products, totalCount) = await _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Product categories

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductCategoryList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var productCategoriesModel = await _productViewModelService.PrepareProductCategoryModel(product);
            var gridModel = new DataSourceResult {
                Data = productCategoriesModel,
                Total = productCategoriesModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductCategoryInsert(ProductModel.ProductCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productViewModelService.InsertProductCategoryModel(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductCategoryUpdate(ProductModel.ProductCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productViewModelService.UpdateProductCategoryModel(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductCategoryDelete(ProductModel.ProductCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteProductCategory(model.Id, model.ProductId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Product manufacturers

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductManufacturerList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var productManufacturersModel = await _productViewModelService.PrepareProductManufacturerModel(product);
            var gridModel = new DataSourceResult {
                Data = productManufacturersModel,
                Total = productManufacturersModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductManufacturerInsert(ProductModel.ProductManufacturerModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productViewModelService.InsertProductManufacturer(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductManufacturerUpdate(ProductModel.ProductManufacturerModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productViewModelService.UpdateProductManufacturer(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductManufacturerDelete(ProductModel.ProductManufacturerModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteProductManufacturer(model.Id, model.ProductId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Related products

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> RelatedProductList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var relatedProducts = product.RelatedProducts.OrderBy(x => x.DisplayOrder);
            var relatedProductsModel = new List<ProductModel.RelatedProductModel>();
            foreach (var x in relatedProducts)
            {
                relatedProductsModel.Add(new ProductModel.RelatedProductModel {
                    Id = x.Id,
                    ProductId1 = productId,
                    ProductId2 = x.ProductId2,
                    Product2Name = (await _productService.GetProductById(x.ProductId2))?.Name,
                    DisplayOrder = x.DisplayOrder
                });
            }

            var gridModel = new DataSourceResult {
                Data = relatedProductsModel,
                Total = relatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> RelatedProductUpdate(ProductModel.RelatedProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.UpdateRelatedProductModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> RelatedProductDelete(ProductModel.RelatedProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteRelatedProductModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> RelatedProductAddPopup(string productId)
        {
            var model = await _productViewModelService.PrepareRelatedProductModel();
            model.ProductId = productId;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> RelatedProductAddPopupList(DataSourceRequest command, ProductModel.AddRelatedProductModel model)
        {
            var (products, totalCount) = await _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> RelatedProductAddPopup(ProductModel.AddRelatedProductModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedProductIds != null)
                {
                    await _productViewModelService.InsertRelatedProductModel(model);
                }

                //a vendor should have access only to his products
                model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
                ViewBag.RefreshPage = true;
            }
            else
            {
                ErrorNotification(ModelState);
                model = await _productViewModelService.PrepareRelatedProductModel();
                model.ProductId = model.ProductId;
            }
            return View(model);
        }

        #endregion

        #region Similar products

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> SimilarProductList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var similarProducts = product.SimilarProducts.OrderBy(x => x.DisplayOrder);
            var similarProductsModel = new List<ProductModel.SimilarProductModel>();
            foreach (var x in similarProducts)
            {
                similarProductsModel.Add(new ProductModel.SimilarProductModel {
                    Id = x.Id,
                    ProductId1 = productId,
                    ProductId2 = x.ProductId2,
                    Product2Name = (await _productService.GetProductById(x.ProductId2))?.Name,
                    DisplayOrder = x.DisplayOrder
                });
            }

            var gridModel = new DataSourceResult {
                Data = similarProductsModel,
                Total = similarProductsModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> SimilarProductUpdate(ProductModel.SimilarProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.UpdateSimilarProductModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> SimilarProductDelete(ProductModel.SimilarProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteSimilarProductModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> SimilarProductAddPopup(string productId)
        {
            var model = await _productViewModelService.PrepareSimilarProductModel();
            model.ProductId = productId;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> SimilarProductAddPopupList(DataSourceRequest command, ProductModel.AddSimilarProductModel model)
        {
            var (products, totalCount) = await _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> SimilarProductAddPopup(ProductModel.AddSimilarProductModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedProductIds != null)
                {
                    await _productViewModelService.InsertSimilarProductModel(model);
                }
                //a vendor should have access only to his products
                model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
                ViewBag.RefreshPage = true;
            }
            else
            {
                ErrorNotification(ModelState);
                model = await _productViewModelService.PrepareSimilarProductModel();
                model.ProductId = model.ProductId;
            }
            return View(model);
        }

        #endregion

        #region Bundle products

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> BundleProductList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var bundleProducts = product.BundleProducts.OrderBy(x => x.DisplayOrder);
            var bundleProductsModel = new List<ProductModel.BundleProductModel>();
            foreach (var x in bundleProducts)
            {
                bundleProductsModel.Add(new ProductModel.BundleProductModel {
                    Id = x.Id,
                    ProductBundleId = productId,
                    ProductId = x.ProductId,
                    ProductName = (await _productService.GetProductById(x.ProductId))?.Name,
                    DisplayOrder = x.DisplayOrder,
                    Quantity = x.Quantity
                });
            }
            var gridModel = new DataSourceResult {
                Data = bundleProductsModel,
                Total = bundleProductsModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> BundleProductUpdate(ProductModel.BundleProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.UpdateBundleProductModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> BundleProductDelete(ProductModel.BundleProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteBundleProductModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> BundleProductAddPopup(string productId)
        {
            var model = await _productViewModelService.PrepareBundleProductModel();
            model.ProductId = productId;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> BundleProductAddPopupList(DataSourceRequest command, ProductModel.AddBundleProductModel model)
        {
            var (products, totalCount) = await _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> BundleProductAddPopup(ProductModel.AddBundleProductModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedProductIds != null)
                {
                    await _productViewModelService.InsertBundleProductModel(model);
                }

                //a vendor should have access only to his products
                model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
                ViewBag.RefreshPage = true;
            }
            else
            {
                ErrorNotification(ModelState);
                model = await _productViewModelService.PrepareBundleProductModel();
                model.ProductId = model.ProductId;
            }
            return View(model);
        }

        #endregion

        #region Cross-sell products

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> CrossSellProductList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var crossSellProducts = product.CrossSellProduct;
            var crossSellProductsModel = new List<ProductModel.CrossSellProductModel>();
            foreach (var x in crossSellProducts)
            {
                crossSellProductsModel.Add(new ProductModel.CrossSellProductModel {
                    Id = x,
                    ProductId = product.Id,
                    Product2Name = (await _productService.GetProductById(x))?.Name,
                });
            }
            var gridModel = new DataSourceResult {
                Data = crossSellProductsModel,
                Total = crossSellProductsModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> CrossSellProductDelete(ProductModel.CrossSellProductModel model)
        {
            var product = await _productService.GetProductById(model.ProductId);
            if (product == null)
            {
                throw new ArgumentException("Product not exists");
            }
            var crossSellProduct = product.CrossSellProduct.Where(x => x == model.Id).FirstOrDefault();
            if (string.IsNullOrEmpty(crossSellProduct))
                throw new ArgumentException("No cross-sell product found with the specified id");

            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteCrossSellProduct(product.Id, crossSellProduct);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> CrossSellProductAddPopup(string productId)
        {
            var model = await _productViewModelService.PrepareCrossSellProductModel();
            model.ProductId = productId;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> CrossSellProductAddPopupList(DataSourceRequest command, ProductModel.AddCrossSellProductModel model)
        {
            var (products, totalCount) = await _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> CrossSellProductAddPopup(ProductModel.AddCrossSellProductModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedProductIds != null)
                {
                    await _productViewModelService.InsertCrossSellProductModel(model);
                }
                //a vendor should have access only to his products
                model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
                ViewBag.RefreshPage = true;
            }
            else
            {
                ErrorNotification(ModelState);
                model = await _productViewModelService.PrepareCrossSellProductModel();
                model.ProductId = model.ProductId;
            }
            return View(model);
        }

        #endregion

        #region Associated products

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> AssociatedProductList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var vendorId = "";
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var associatedProducts = await _productService.GetAssociatedProducts(parentGroupedProductId: productId,
                vendorId: vendorId,
                showHidden: true);
            var associatedProductsModel = associatedProducts
                .Select(x => new ProductModel.AssociatedProductModel {
                    Id = x.Id,
                    ProductId = productId,
                    ProductName = x.Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult {
                Data = associatedProductsModel,
                Total = associatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AssociatedProductUpdate(ProductModel.AssociatedProductModel model)
        {
            if (ModelState.IsValid)
            {
                var associatedProduct = await _productService.GetProductById(model.Id);
                if (associatedProduct == null)
                    throw new ArgumentException("No associated product found with the specified id");

                associatedProduct.DisplayOrder = model.DisplayOrder;
                await _productService.UpdateAssociatedProduct(associatedProduct);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AssociatedProductDelete(ProductModel.AssociatedProductModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(model.Id);
                if (product == null)
                    throw new ArgumentException("No associated product found with the specified id");

                await _productViewModelService.DeleteAssociatedProduct(product);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AssociatedProductAddPopup(string productId)
        {
            var model = await _productViewModelService.PrepareAssociatedProductModel();
            model.ProductId = productId;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AssociatedProductAddPopupList(DataSourceRequest command, ProductModel.AddAssociatedProductModel model)
        {
            var (products, totalCount) = await _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> AssociatedProductAddPopup(ProductModel.AddAssociatedProductModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedProductIds != null)
                {
                    await _productViewModelService.InsertAssociatedProductModel(model);
                }
                //a vendor should have access only to his products
                model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
                ViewBag.RefreshPage = true;
            }
            else
            {
                ErrorNotification(ModelState);
                model = await _productViewModelService.PrepareAssociatedProductModel();
                model.ProductId = model.ProductId;
            }
            return View(model);
        }

        #endregion

        #region Product pictures
        public async Task<IActionResult> ProductPictureAdd(string pictureId, int displayOrder,
            string overrideAltAttribute, string overrideTitleAttribute,
            string productId)
        {
            if (string.IsNullOrEmpty(pictureId))
                throw new ArgumentException();

            var product = await _productService.GetProductById(productId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { Result = false });

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Json(new { Result = false });

            await _productViewModelService.InsertProductPicture(product, pictureId, displayOrder, overrideAltAttribute, overrideTitleAttribute);

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductPictureList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var productPicturesModel = await _productViewModelService.PrepareProductPictureModel(product);
            var gridModel = new DataSourceResult {
                Data = productPicturesModel,
                Total = productPicturesModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductPictureUpdate(ProductModel.ProductPictureModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.UpdateProductPicture(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductPictureDelete(ProductModel.ProductPictureModel model)
        {
            if (ModelState.IsValid)
            {
                await _productViewModelService.DeleteProductPicture(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion

        #region Product specification attributes
        //ajax
        [AcceptVerbs("GET")]
        public async Task<IActionResult> GetOptionsByAttributeId(string attributeId, [FromServices] ISpecificationAttributeService specificationAttributeService)
        {
            if (String.IsNullOrEmpty(attributeId))
                throw new ArgumentNullException("attributeId");

            var options = (await specificationAttributeService.GetSpecificationAttributeById(attributeId)).SpecificationAttributeOptions.OrderBy(x => x.DisplayOrder);
            var result = (from o in options
                          select new { id = o.Id, name = o.Name }).ToList();
            return Json(result);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductSpecificationAttributeAdd(ProductModel.AddProductSpecificationAttributeModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(model.ProductId);
                if (product == null)
                    return Content("Product not exists");

                await _productViewModelService.InsertProductSpecificationAttributeModel(model, product);

                return Json(new { Result = true });
            }
            return Json(new { Result = false });
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductSpecAttrList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var productrSpecsModel = await _productViewModelService.PrepareProductSpecificationAttributeModel(product);
            var gridModel = new DataSourceResult {
                Data = productrSpecsModel,
                Total = productrSpecsModel.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductSpecAttrUpdate(ProductSpecificationAttributeModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(model.ProductId);
                if (product == null)
                    return Content("Product not exists");

                var psa = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeId == model.ProductSpecificationId).Where(x => x.Id == model.Id).FirstOrDefault();
                if (psa == null)
                    return Content("No product specification attribute found with the specified id");

                await _productViewModelService.UpdateProductSpecificationAttributeModel(product, psa, model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductSpecAttrDelete(ProductSpecificationAttributeModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(model.ProductId);
                if (product == null)
                    return Content("Product not exists");

                var psa = product.ProductSpecificationAttributes.Where(x => x.Id == model.Id && x.SpecificationAttributeId == model.ProductSpecificationId).FirstOrDefault();
                if (psa == null)
                    throw new ArgumentException("No specification attribute found with the specified id");

                await _productViewModelService.DeleteProductSpecificationAttribute(product, psa);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Purchased with order

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> PurchasedWithOrders(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var (orderModels, totalCount) = await _productViewModelService.PrepareOrderModel(productId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = orderModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Reviews

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> Reviews(DataSourceRequest command, string productId, [FromServices] IProductReviewService productReviewService)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            var productReviews = await productReviewService.GetAllProductReviews("", null,
                null, null, "", storeId, productId);

            var items = new List<ProductReviewModel>();
            foreach (var item in productReviews.PagedForCommand(command))
            {
                var m = new ProductReviewModel();
                await _productViewModelService.PrepareProductReviewModel(m, item, false, true);
                items.Add(m);
            }
            var gridModel = new DataSourceResult {
                Data = items,
                Total = productReviews.Count,
            };

            return Json(gridModel);
        }

        #endregion

        #region Export / Import

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("download-catalog-pdf")]
        public async Task<IActionResult> DownloadCatalogAsPdf(ProductListModel model, [FromServices] IPdfService pdfService)
        {
            var products = await _productViewModelService.PrepareProducts(model);
            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await pdfService.PrintProductsToPdf(stream, products);
                    bytes = stream.ToArray();
                }
                return File(bytes, "application/pdf", "pdfcatalog.pdf");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public async Task<IActionResult> ExportXmlAll(ProductListModel model)
        {
            var products = await _productViewModelService.PrepareProducts(model);
            try
            {
                var xml = await _exportManager.ExportProductsToXml(products);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "products.xml");

            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportXmlSelected(string selectedIds)
        {
            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                products.AddRange(await _productService.GetProductsByIds(ids, true));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var xml = await _exportManager.ExportProductsToXml(products);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "products.xml");

        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public async Task<IActionResult> ExportExcelAll(ProductListModel model)
        {
            var products = await _productViewModelService.PrepareProducts(model);
            try
            {
                byte[] bytes = _exportManager.ExportProductsToXlsx(products);
                return File(bytes, "text/xls", "products.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportExcelSelected(string selectedIds)
        {
            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                products.AddRange(await _productService.GetProductsByIds(ids, true));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            byte[] bytes = _exportManager.ExportProductsToXlsx(products);
            return File(bytes, "text/xls", "products.xlsx");
        }

        [PermissionAuthorizeAction(PermissionActionName.Import)]
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
        {
            //a vendor ans staff cannot import products
            if (_workContext.CurrentVendor != null || _workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _importManager.ImportProductsFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }

        }

        #endregion

        #region Bulk editing

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> BulkEdit()
        {
            var model = await _productViewModelService.PrepareBulkEditListModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> BulkEditSelect(DataSourceRequest command, BulkEditListModel model)
        {
            var (bulkEditProductModels, totalCount) = await _productViewModelService.PrepareBulkEditProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = bulkEditProductModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> BulkEditUpdate(IEnumerable<BulkEditProductModel> products)
        {
            if (products != null)
            {
                await _productViewModelService.UpdateBulkEdit(products.ToList());
            }
            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> BulkEditDelete(IEnumerable<BulkEditProductModel> products)
        {
            if (products != null)
            {
                await _productViewModelService.DeleteBulkEdit(products.ToList());
            }
            return new NullJsonResult();
        }

        #endregion

        #region Tier prices

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> TierPriceList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var tierPricesModel = await _productViewModelService.PrepareTierPriceModel(product);
            var gridModel = new DataSourceResult {
                Data = tierPricesModel,
                Total = tierPricesModel.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> TierPriceCreatePopup(string productId)
        {
            var model = new ProductModel.TierPriceModel {
                ProductId = productId
            };
            await _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> TierPriceCreatePopup(ProductModel.TierPriceModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(model.ProductId);
                if (product == null)
                    throw new ArgumentException("No product found with the specified id");

                var tierPrice = model.ToEntity(_dateTimeHelper);
                await _productService.InsertTierPrice(tierPrice);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            else
            {
                ErrorNotification(ModelState);
            }
            //If we got this far, something failed, redisplay form
            await _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> TierPriceEditPopup(string id, string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var tierPrice = product.TierPrices.Where(x => x.Id == id).FirstOrDefault();
            if (tierPrice == null)
                return Content("Empty tier price");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            var model = tierPrice.ToModel(_dateTimeHelper);
            model.ProductId = productId;
            await _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> TierPriceEditPopup(string productId, ProductModel.TierPriceModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(productId, true);
                if (product == null)
                    throw new ArgumentException("No product found with the specified id");

                var tierPrice = product.TierPrices.Where(x => x.Id == model.Id).FirstOrDefault();
                if (tierPrice == null)
                    return Content("Empty tier price");

                tierPrice = model.ToEntity(tierPrice, _dateTimeHelper);
                await _productService.UpdateTierPrice(tierPrice);

                ViewBag.RefreshPage = true;
                return View(model);
            }
            ErrorNotification(ModelState);
            //stores
            await _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> TierPriceDelete(ProductModel.TierPriceModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(model.ProductId, true);
                if (product == null)
                    throw new ArgumentException("No product found with the specified id");

                var tierPrice = product.TierPrices.Where(x => x.Id == model.Id).FirstOrDefault();
                if (tierPrice == null)
                    throw new ArgumentException("No tier price found with the specified id");
                tierPrice.ProductId = product.Id;

                await _productService.DeleteTierPrice(tierPrice);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Product attributes

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeMappingList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var attributesModel = await _productViewModelService.PrepareProductAttributeMappingModels(product);
            var gridModel = new DataSourceResult {
                Data = attributesModel,
                Total = attributesModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAttributeMappingPopup(string productId, string productAttributeMappingId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return Content(permission.message);

            if (string.IsNullOrEmpty(productAttributeMappingId))
            {
                var model = await _productViewModelService.PrepareProductAttributeMappingModel(product);
                return View(model);
            }
            else
            {
                var productAttributeMapping = product.ProductAttributeMappings.FirstOrDefault(x => x.Id == productAttributeMappingId);
                var model = await _productViewModelService.PrepareProductAttributeMappingModel(product, productAttributeMapping);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeMappingPopup(ProductModel.ProductAttributeMappingModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductById(model.ProductId);
                if (product == null)
                    throw new ArgumentException("No product found with the specified id");

                if (string.IsNullOrEmpty(model.Id))
                    await _productViewModelService.InsertProductAttributeMappingModel(model);
                else
                    await _productViewModelService.UpdateProductAttributeMappingModel(model);

                ViewBag.RefreshPage = true;
                return View(model);
            }
            ErrorNotification(ModelState);
            model = await _productViewModelService.PrepareProductAttributeMappingModel(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeMappingDelete(string id, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == id).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            productAttributeMapping.ProductId = productId;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return ErrorForKendoGridJson(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            await productAttributeService.DeleteProductAttributeMapping(productAttributeMapping);
            return new NullJsonResult();
        }

        //edit
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAttributeValidationRulesPopup(string id, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return Content(permission.message);

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == id).FirstOrDefault();
            if (productAttributeMapping == null)
                return Content("No attribute value found with the specified id");


            var model = await _productViewModelService.PrepareProductAttributeMappingModel(productAttributeMapping);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeValidationRulesPopup(ProductModel.ProductAttributeMappingModel model)
        {
            var product = await _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No attribute value found with the specified id");

            if (ModelState.IsValid)
            {
                await _productViewModelService.UpdateProductAttributeValidationRulesModel(productAttributeMapping, model);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            ErrorNotification(ModelState);
            model = await _productViewModelService.PrepareProductAttributeMappingModel(productAttributeMapping);
            return View(model);
        }

        #endregion

        #region Product attributes. Condition

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAttributeConditionPopup(string productId, string productAttributeMappingId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return Content(permission.message);

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return Content("No attribute value found with the specified id");

            var model = await _productViewModelService.PrepareProductAttributeConditionModel(product, productAttributeMapping);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeConditionPopup(ProductAttributeConditionModel model, IFormCollection form)
        {
            var product = await _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                return Content("No attribute value found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Content(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            var formcollection = new Dictionary<string, string>();
            foreach (var item in form)
            {
                formcollection.Add(item.Key, item.Value);
            }
            await _productViewModelService.UpdateProductAttributeConditionModel(product, productAttributeMapping, model, formcollection);
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Product attribute values

        //list
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> EditAttributeValues(string productAttributeMappingId, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List", "Product");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!(!product.LimitedToStores || (product.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && product.LimitedToStores)))
                    return Content("This is not your product");
            }
            var productAttribute = await productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId);
            var model = new ProductModel.ProductAttributeValueListModel {
                ProductName = product.Name,
                ProductId = product.Id,
                ProductAttributeName = productAttribute.Name,
                ProductAttributeMappingId = productAttributeMappingId,
            };

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeValueList(string productAttributeMappingId, string productId, DataSourceRequest command)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            var values = await _productViewModelService.PrepareProductAttributeValueModels(product, productAttributeMapping);
            var gridModel = new DataSourceResult {
                Data = values,
                Total = values.Count()
            };
            return Json(gridModel);
        }

        //create
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAttributeValueCreatePopup(string productAttributeMappingId, string productId, [FromServices] IPictureService pictureService)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return Content(permission.message);

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            var model = new ProductModel.ProductAttributeValueModel {
                ProductAttributeMappingId = productAttributeMappingId,
                ProductId = productId,

                //color squares
                DisplayColorSquaresRgb = productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares,
                ColorSquaresRgb = "#000000",
                //image squares
                DisplayImageSquaresPicture = productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares,

                //default qantity for associated product
                Quantity = 1
            };

            //locales
            await AddLocales(_languageService, model.Locales);

            //pictures
            foreach (var x in product.ProductPictures)
            {
                model.ProductPictureModels.Add(new ProductModel.ProductPictureModel {
                    Id = x.Id,
                    ProductId = product.Id,
                    PictureId = x.PictureId,
                    PictureUrl = await pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                });
            }
            return View(model);
        }

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAttributeValueCreatePopup(ProductModel.ProductAttributeValueModel model)
        {
            var product = await _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No product attribute found with the specified id
                return RedirectToAction("List", "Product");

            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && string.IsNullOrEmpty(model.ImageSquaresPictureId))
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
            {
                await _productViewModelService.InsertProductAttributeValueModel(model);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            await _productViewModelService.PrepareProductAttributeValueModel(product, model);
            return View(model);
        }

        //edit
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAttributeValueEditPopup(string id, string productId, string productAttributeMappingId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var pa = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (pa == null)
                return RedirectToAction("List", "Product");

            var pav = pa.ProductAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var model = await _productViewModelService.PrepareProductAttributeValueModel(pa, pav);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pav.GetLocalized(x => x.Name, languageId, false, false);
            });
            //pictures
            await _productViewModelService.PrepareProductAttributeValueModel(product, model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeValueEditPopup(string productId, ProductModel.ProductAttributeValueModel model)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pav = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault().ProductAttributeValues.Where(x => x.Id == model.Id).FirstOrDefault();
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && String.IsNullOrEmpty(model.ImageSquaresPictureId))
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
            {
                await _productViewModelService.UpdateProductAttributeValueModel(pav, model);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            await _productViewModelService.PrepareProductAttributeValueModel(product, model);
            return View(model);
        }

        //delete
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeValueDelete(string Id, string pam, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pav = product.ProductAttributeMappings.Where(x => x.Id == pam).FirstOrDefault().ProductAttributeValues.Where(x => x.Id == Id).FirstOrDefault();
            if (pav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            pav.ProductAttributeMappingId = pam;
            pav.ProductId = productId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    ModelState.AddModelError("", _localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            if (ModelState.IsValid)
            {
                await productAttributeService.DeleteProductAttributeValue(pav);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public async Task<IActionResult> AssociateProductToAttributeValuePopup()
        {
            var model = await _productViewModelService.PrepareAssociateProductToAttributeValueModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AssociateProductToAttributeValuePopupList(DataSourceRequest command,
            ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            var (products, totalCount) = await _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> AssociateProductToAttributeValuePopup(ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            var associatedProduct = await _productService.GetProductById(model.AssociatedToProductId);
            if (associatedProduct == null)
                return Content("Cannot load a product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            ViewBag.productId = associatedProduct.Id;
            ViewBag.productName = associatedProduct.Name;
            return View(model);
        }

        #endregion

        #region Product attribute combinations

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeCombinationList(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var combinationsModel = await _productViewModelService.PrepareProductAttributeCombinationModel(product);
            var gridModel = new DataSourceResult {
                Data = combinationsModel,
                Total = combinationsModel.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeCombinationDelete(string id, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var combination = product.ProductAttributeCombinations.Where(x => x.Id == id).FirstOrDefault();
            if (combination == null)
                throw new ArgumentException("No product attribute combination found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Content(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            combination.ProductId = productId;

            await productAttributeService.DeleteProductAttributeCombination(combination);
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var pr = await _productService.GetProductById(productId);
                pr.StockQuantity = pr.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                await _productService.UpdateStockProduct(pr, false);
            }

            return new NullJsonResult();
        }

        //edit
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AttributeCombinationPopup(string productId, string Id)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return Content(permission.message);

            var model = await _productViewModelService.PrepareProductAttributeCombinationModel(product, Id);
            await _productViewModelService.PrepareAddProductAttributeCombinationModel(model, product);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AttributeCombinationPopup(string productId,
            ProductAttributeCombinationModel model, IFormCollection form)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List", "Product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Content(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            var formcollection = new Dictionary<string, string>();
            foreach (var item in form)
            {
                formcollection.Add(item.Key, item.Value);
            }
            var warnings = await _productViewModelService.InsertOrUpdateProductAttributeCombinationPopup(product, model, formcollection);
            if (!warnings.Any())
            {
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            await _productViewModelService.PrepareAddProductAttributeCombinationModel(model, product);
            model.Warnings = warnings;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> GenerateAllAttributeCombinations(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Content(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            await _productViewModelService.GenerateAllAttributeCombinations(product);

            return Json(new { Success = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ClearAllAttributeCombinations(string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    ModelState.AddModelError("", _localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            if (ModelState.IsValid)
            {
                foreach (var combination in product.ProductAttributeCombinations.ToList())
                {
                    combination.ProductId = productId;
                    await productAttributeService.DeleteProductAttributeCombination(combination);
                }

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                {
                    product.StockQuantity = 0;
                    await _productService.UpdateStockProduct(product, false);
                }
                return Json(new { Success = true });
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #region Product Attribute combination - tier prices

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeCombinationTierPriceList(DataSourceRequest command, string productId, string productAttributeCombinationId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var tierPriceModel = await _productViewModelService.PrepareProductAttributeCombinationTierPricesModel(product, productAttributeCombinationId);
            var gridModel = new DataSourceResult {
                Data = tierPriceModel,
                Total = tierPriceModel.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeCombinationTierPriceInsert(string productId, string productAttributeCombinationId, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Content("", _localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
                await _productViewModelService.InsertProductAttributeCombinationTierPricesModel(product, combination, model);

            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeCombinationTierPriceUpdate(string productId, string productAttributeCombinationId, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Content("", _localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
                await _productViewModelService.UpdateProductAttributeCombinationTierPricesModel(product, combination, model);

            return new NullJsonResult();
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAttributeCombinationTierPriceDelete(string productId, string productAttributeCombinationId, string id)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Content("This is not your product");

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return ErrorForKendoGridJson(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
            {
                var tierPrice = combination.TierPrices.FirstOrDefault(x => x.Id == id);
                if (tierPrice != null)
                {
                    await _productViewModelService.DeleteProductAttributeCombinationTierPrices(product, combination, tierPrice);
                }
            }
            return new NullJsonResult();
        }
        #endregion

        #endregion

        #region Activity log

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ListActivityLog(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var (activityLogModels, totalCount) = await _productViewModelService.PrepareActivityLogModel(productId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = activityLogModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Reservation

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ListReservations(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);

            var permission = CheckAccessToProduct(product);
            if (!permission.allow)
                return ErrorForKendoGridJson(permission.message);

            var reservations = await _productReservationService.GetProductReservationsByProductId(productId, null, null, command.Page - 1, command.PageSize);
            var reservationModel = reservations
                .Select(x => new ProductModel.ReservationModel {
                    ReservationId = x.Id,
                    Date = x.Date,
                    OrderId = x.OrderId,
                    ProductId = x.ProductId,
                    Parameter = x.Parameter,
                    Resource = x.Resource,
                    Duration = x.Duration
                }).ToList();

            var gridModel = new DataSourceResult {
                Data = reservationModel,
                Total = reservations.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> GenerateCalendar(string productId, ProductModel.GenerateCalendarModel model)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { errors = "This is not your product" });

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Json(new { errors = _localizationService.GetResource("Admin.Catalog.Products.Permisions") });

            var reservations = await _productReservationService.GetProductReservationsByProductId(productId, null, null);
            if (reservations.Any())
            {
                if (((product.IntervalUnitType == IntervalUnit.Minute || product.IntervalUnitType == IntervalUnit.Hour) && (IntervalUnit)model.Interval == IntervalUnit.Day) ||
                    (product.IntervalUnitType == IntervalUnit.Day) && (((IntervalUnit)model.IntervalUnit == IntervalUnit.Minute || (IntervalUnit)model.IntervalUnit == IntervalUnit.Hour)))
                {
                    return Json(new { errors = _localizationService.GetResource("Admin.Catalog.Products.Calendar.CannotChangeInterval") });
                }
            }
            await _productService.UpdateIntervalProperties(productId, model.Interval, (IntervalUnit)model.IntervalUnit, model.IncBothDate);

            if (!ModelState.IsValid)
            {
                Dictionary<string, Dictionary<string, object>> error = (Dictionary<string, Dictionary<string, object>>)ModelState.SerializeErrors();
                string s = "";
                foreach (var error1 in error)
                {
                    foreach (var error2 in error1.Value)
                    {
                        string[] v = (string[])error2.Value;
                        s += v[0] + "\n";
                    }
                }

                return Json(new { errors = s });
            }

            int minutesToAdd = 0;
            if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Minute)
            {
                minutesToAdd = model.Interval;
            }
            else if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Hour)
            {
                minutesToAdd = model.Interval * 60;
            }
            else if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Day)
            {
                minutesToAdd = model.Interval * 60 * 24;
            }

            int _hourFrom = model.StartTime.Hour;
            int _minutesFrom = model.StartTime.Minute;
            int _hourTo = model.EndTime.Hour;
            int _minutesTo = model.EndTime.Minute;
            DateTime _dateFrom = new DateTime(model.StartDate.Value.Year, model.StartDate.Value.Month, model.StartDate.Value.Day, 0, 0, 0, 0);
            DateTime _dateTo = new DateTime(model.EndDate.Value.Year, model.EndDate.Value.Month, model.EndDate.Value.Day, 23, 59, 59, 999);
            if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Day)
            {
                model.Quantity = 1;
                model.Parameter = "";
            }
            else
            {
                model.Resource = "";
            }

            List<DateTime> dates = new List<DateTime>();
            int counter = 0;
            for (DateTime iterator = _dateFrom; iterator <= _dateTo; iterator += new TimeSpan(0, minutesToAdd, 0))
            {
                if ((IntervalUnit)model.IntervalUnit != IntervalUnit.Day)
                {
                    if (iterator.Hour >= _hourFrom && iterator.Hour <= _hourTo)
                    {
                        if (iterator.Hour == _hourTo)
                        {
                            if (iterator.Minute > _minutesTo)
                            {
                                continue;
                            }
                        }
                        if (iterator.Hour == _hourFrom)
                        {
                            if (iterator.Minute < _minutesFrom)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                if ((iterator.DayOfWeek == DayOfWeek.Monday && !model.Monday) ||
                   (iterator.DayOfWeek == DayOfWeek.Tuesday && !model.Tuesday) ||
                   (iterator.DayOfWeek == DayOfWeek.Wednesday && !model.Wednesday) ||
                   (iterator.DayOfWeek == DayOfWeek.Thursday && !model.Thursday) ||
                   (iterator.DayOfWeek == DayOfWeek.Friday && !model.Friday) ||
                   (iterator.DayOfWeek == DayOfWeek.Saturday && !model.Saturday) ||
                   (iterator.DayOfWeek == DayOfWeek.Sunday && !model.Sunday))
                {
                    continue;
                }

                for (int i = 0; i < model.Quantity; i++)
                {
                    dates.Add(iterator);
                    try
                    {
                        var insert = true;
                        if (((IntervalUnit)model.IntervalUnit) == IntervalUnit.Day)
                        {
                            if (reservations.Where(x => x.Resource == model.Resource && x.Date == iterator).Any())
                                insert = false;
                        }
                        if (insert)
                        {
                            if (counter++ > 1000)
                                break;

                            await _productReservationService.InsertProductReservation(new ProductReservation {
                                OrderId = "",
                                Date = iterator,
                                ProductId = productId,
                                Resource = model.Resource,
                                Parameter = model.Parameter,
                                Duration = model.Interval + " " + ((IntervalUnit)model.IntervalUnit).GetLocalizedEnum(_localizationService, _workContext),
                            });
                        }
                    }
                    catch { }
                }
            }

            return Json(new { success = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ClearCalendar(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { errors = "This is not your product" });

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Json(new { errors = _localizationService.GetResource("Admin.Catalog.Products.Permisions") });

            var toDelete = await _productReservationService.GetProductReservationsByProductId(productId, true, null);
            foreach (var record in toDelete)
            {
                await _productReservationService.DeleteProductReservation(record);
            }

            return Json("");
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ClearOld(string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { errors = "This is not your product" });

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Json(new { errors = _localizationService.GetResource("Admin.Catalog.Products.Permisions") });

            var toDelete = (await _productReservationService.GetProductReservationsByProductId(productId, true, null)).Where(x => x.Date < DateTime.UtcNow);
            foreach (var record in toDelete)
            {
                await _productReservationService.DeleteProductReservation(record);
            }

            return Json("");
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductReservationDelete(ProductModel.ReservationModel model)
        {
            var product = await _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { errors = "This is not your product" });

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return ErrorForKendoGridJson(_localizationService.GetResource("Admin.Catalog.Products.Permisions"));

            var toDelete = await _productReservationService.GetProductReservation(model.ReservationId);
            if (toDelete != null)
            {
                if (string.IsNullOrEmpty(toDelete.OrderId))
                    await _productReservationService.DeleteProductReservation(toDelete);
                else
                    return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.ProductReservations.CantDeleteWithOrder") });
            }

            return Json("");
        }

        #endregion

        #region Bids

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> ListBids(DataSourceRequest command, string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { errors = "This is not your product" });

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Json(new { errors = _localizationService.GetResource("Admin.Catalog.Products.Permisions") });

            var (bidModels, totalCount) = await _productViewModelService.PrepareBidMode(productId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = bidModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> BidDelete(ProductModel.BidModel model, [FromServices] ICustomerActivityService customerActivityService)
        {
            var product = await _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { errors = "This is not your product" });

            if (_workContext.CurrentCustomer.IsStaff())
                if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.Products.Permisions") });

            var toDelete = await _auctionService.GetBid(model.BidId);
            if (toDelete != null)
            {
                if (string.IsNullOrEmpty(toDelete.OrderId))
                {
                    //activity log
                    await customerActivityService.InsertActivity("DeleteBid", toDelete.ProductId, _localizationService.GetResource("ActivityLog.DeleteBid"), product.Name);
                    //delete bid
                    await _auctionService.DeleteBid(toDelete);
                    return Json("");
                }
                else
                    return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.Products.Bids.CantDeleteWithOrder") });
            }
            return Json(new DataSourceResult { Errors = "Bid not exists" });
        }

        #endregion

        #endregion
    }
}