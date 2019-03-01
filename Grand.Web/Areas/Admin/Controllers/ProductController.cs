using Grand.Core;
using Grand.Core.Domain.Catalog;
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
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            IAuctionService auctionService)
        {
            this._productViewModelService = productViewModelService;
            this._productService = productService;
            this._customerService = customerService;
            this._workContext = workContext;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._exportManager = exportManager;
            this._importManager = importManager;
            this._storeService = storeService;
            this._productReservationService = productReservationService;
            this._auctionService = auctionService;
        }

        #endregion

        #region Methods

        #region Product list / create / edit / delete

        //list products
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _productViewModelService.PrepareProductListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductList(DataSourceRequest command, ProductListModel model)
        {
            var products = _productViewModelService.PrepareProductsModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = products.productModels.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public IActionResult GoToSku(ProductListModel model)
        {
            string sku = model.GoDirectlyToSku;

            //try to load a product entity
            var product = _productService.GetProductBySku(sku);
            if (product != null)
                return RedirectToAction("Edit", "Product", new { id = product.Id });

            //not found
            return RedirectToAction("List", "Product");
        }

        //create product
        public IActionResult Create()
        {
            var model = new ProductModel();
            _productViewModelService.PrepareProductModel(model, null, true, true);
            AddLocales(_languageService, model.Locales);
            model.PrepareACLModel(null, false, _customerService);
            model.PrepareStoresMappingModel(null, false, _storeService);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(ProductModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var product = _productViewModelService.InsertProductModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _productViewModelService.PrepareProductModel(model, null, false, true);
            model.PrepareACLModel(null, true, _customerService);
            model.PrepareStoresMappingModel(null, true, _storeService);

            return View(model);
        }

        //edit product
        public IActionResult Edit(string id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            var model = product.ToModel();
            model.Ticks = product.UpdatedOnUtc.Ticks;

            _productViewModelService.PrepareProductModel(model, product, false, false);
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = product.GetSeName(languageId, false, false);
            });

            model.PrepareACLModel(product, false, _customerService);
            model.PrepareStoresMappingModel(product, false, _storeService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ProductModel model, bool continueEditing)
        {
            var product = _productService.GetProductById(model.Id);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            if (model.Ticks != product.UpdatedOnUtc.Ticks)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.Fields.ChangedWarning"));
                return RedirectToAction("Edit", new { id = product.Id });
            }
            if (ModelState.IsValid)
            {
                product = _productViewModelService.UpdateProductModel(product, model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();
                    return RedirectToAction("Edit", new { id = product.Id });
                }
                return RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            _productViewModelService.PrepareProductModel(model, product, false, true);
            model.PrepareACLModel(product, true, _customerService);
            model.PrepareStoresMappingModel(product, true, _storeService);

            return View(model);
        }
        //delete product
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _productViewModelService.DeleteProduct(product);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        public IActionResult DeleteSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                _productViewModelService.DeleteSelected(selectedIds.ToList());
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult CopyProduct(ProductModel model, [FromServices] ICopyProductService copyProductService)
        {
            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = _productService.GetProductById(copyModel.Id);

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && originalProduct.VendorId != _workContext.CurrentVendor.Id)
                    return RedirectToAction("List");

                var newProduct = copyProductService.CopyProduct(originalProduct,
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

        [HttpPost]

        public IActionResult LoadProductFriendlyNames(string productIds)
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

                var products = _productService.GetProductsByIds(ids.ToArray());
                for (int i = 0; i <= products.Count - 1; i++)
                {
                    result += products[i].Name;
                    if (i != products.Count - 1)
                        result += ", ";
                }
            }

            return Json(new { Text = result });
        }

        public IActionResult RequiredProductAddPopup(string productIdsInput)
        {
            var model = _productViewModelService.PrepareAddRequiredProductModel();
            ViewBag.productIdsInput = productIdsInput;
            return View(model);
        }

        [HttpPost]
        public IActionResult RequiredProductAddPopupList(DataSourceRequest command, ProductModel.AddRequiredProductModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var items = _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = items.products.ToList();
            gridModel.Total = items.totalCount;

            return Json(gridModel);
        }

        #endregion

        #region Product categories

        [HttpPost]
        public IActionResult ProductCategoryList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            var productCategoriesModel = _productViewModelService.PrepareProductCategoryModel(product);
            var gridModel = new DataSourceResult
            {
                Data = productCategoriesModel,
                Total = productCategoriesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductCategoryInsert(ProductModel.ProductCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _productViewModelService.InsertProductCategoryModel(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductCategoryUpdate(ProductModel.ProductCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _productViewModelService.UpdateProductCategoryModel(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductCategoryDelete(string id, string productId)
        {
            if (ModelState.IsValid)
            {
                _productViewModelService.DeleteProductCategory(id, productId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Product manufacturers

        [HttpPost]
        public IActionResult ProductManufacturerList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            var productManufacturersModel = _productViewModelService.PrepareProductManufacturerModel(product);
            var gridModel = new DataSourceResult
            {
                Data = productManufacturersModel,
                Total = productManufacturersModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductManufacturerInsert(ProductModel.ProductManufacturerModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _productViewModelService.InsertProductManufacturer(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductManufacturerUpdate(ProductModel.ProductManufacturerModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _productViewModelService.UpdateProductManufacturer(model);
                    return new NullJsonResult();
                }
                catch (Exception ex)
                {
                    return ErrorForKendoGridJson(ex.Message);
                }
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpPost]
        public IActionResult ProductManufacturerDelete(string id, string productId)
        {
            if (ModelState.IsValid)
            {
                _productViewModelService.DeleteProductManufacturer(id, productId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Related products

        [HttpPost]
        public IActionResult RelatedProductList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var relatedProducts = product.RelatedProducts.OrderBy(x => x.DisplayOrder);
            var relatedProductsModel = relatedProducts
                .Select(x => new ProductModel.RelatedProductModel
                {
                    Id = x.Id,
                    ProductId1 = productId,
                    ProductId2 = x.ProductId2,
                    Product2Name = _productService.GetProductById(x.ProductId2).Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = relatedProductsModel,
                Total = relatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult RelatedProductUpdate(ProductModel.RelatedProductModel model)
        {
            _productViewModelService.UpdateRelatedProductModel(model);
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult RelatedProductDelete(ProductModel.RelatedProductModel model)
        {
            _productViewModelService.DeleteRelatedProductModel(model);
            return new NullJsonResult();
        }

        public IActionResult RelatedProductAddPopup(string productId)
        {
            var model = _productViewModelService.PrepareRelatedProductModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult RelatedProductAddPopupList(DataSourceRequest command, ProductModel.AddRelatedProductModel model)
        {
            var items = _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = items.products.ToList();
            gridModel.Total = items.totalCount;

            return Json(gridModel);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult RelatedProductAddPopup(ProductModel.AddRelatedProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _productViewModelService.InsertRelatedProductModel(model);
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Bundle products

        [HttpPost]
        public IActionResult BundleProductList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            var bundleProducts = product.BundleProducts.OrderBy(x => x.DisplayOrder);
            var bundleProductsModel = bundleProducts
                .Select(x => new ProductModel.BundleProductModel
                {
                    Id = x.Id,
                    ProductBundleId = productId,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId).Name,
                    DisplayOrder = x.DisplayOrder,
                    Quantity = x.Quantity
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = bundleProductsModel,
                Total = bundleProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BundleProductUpdate(ProductModel.BundleProductModel model)
        {
            _productViewModelService.UpdateBundleProductModel(model);
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult BundleProductDelete(ProductModel.BundleProductModel model)
        {
            _productViewModelService.DeleteBundleProductModel(model);
            return new NullJsonResult();
        }

        public IActionResult BundleProductAddPopup(string productId)
        {
            var model = _productViewModelService.PrepareBundleProductModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult BundleProductAddPopupList(DataSourceRequest command, ProductModel.AddBundleProductModel model)
        {
            var items = _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = items.products.ToList();
            gridModel.Total = items.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult BundleProductAddPopup(ProductModel.AddBundleProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _productViewModelService.InsertBundleProductModel(model);
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Cross-sell products

        [HttpPost]
        public IActionResult CrossSellProductList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            var crossSellProducts = product.CrossSellProduct;
            var crossSellProductsModel = crossSellProducts
                .Select(x => new ProductModel.CrossSellProductModel
                {
                    Id = x,
                    Product2Name = _productService.GetProductById(x).Name,
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = crossSellProductsModel,
                Total = crossSellProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CrossSellProductDelete(string id, string productId)
        {
            var product = _productService.GetProductById(productId);

            var crossSellProduct = product.CrossSellProduct.Where(x => x == id).FirstOrDefault();
            if (String.IsNullOrEmpty(crossSellProduct))
                throw new ArgumentException("No cross-sell product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            _productViewModelService.DeleteCrossSellProduct(productId, crossSellProduct);
            return new NullJsonResult();
        }

        public IActionResult CrossSellProductAddPopup(string productId)
        {
            var model = _productViewModelService.PrepareCrossSellProductModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult CrossSellProductAddPopupList(DataSourceRequest command, ProductModel.AddCrossSellProductModel model)
        {
            var items = _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = items.products.ToList();
            gridModel.Total = items.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult CrossSellProductAddPopup(ProductModel.AddCrossSellProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _productViewModelService.InsertCrossSellProductModel(model);
            }
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;

            return View(model);
        }

        #endregion

        #region Associated products

        [HttpPost]
        public IActionResult AssociatedProductList(DataSourceRequest command, string productId)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            //a vendor should have access only to his products
            var vendorId = "";
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var associatedProducts = _productService.GetAssociatedProducts(parentGroupedProductId: productId,
                vendorId: vendorId,
                showHidden: true);
            var associatedProductsModel = associatedProducts
                .Select(x => new ProductModel.AssociatedProductModel
                {
                    Id = x.Id,
                    ProductName = x.Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = associatedProductsModel,
                Total = associatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult AssociatedProductUpdate(ProductModel.AssociatedProductModel model)
        {
            var associatedProduct = _productService.GetProductById(model.Id);
            if (associatedProduct == null)
                throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
            {
                return Content("This is not your product");
            }

            associatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateAssociatedProduct(associatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult AssociatedProductDelete(string id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
                throw new ArgumentException("No associated product found with the specified id");

            _productViewModelService.DeleteAssociatedProduct(product);
            return new NullJsonResult();
        }

        public IActionResult AssociatedProductAddPopup(string productId)
        {
            var model = _productViewModelService.PrepareAssociatedProductModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult AssociatedProductAddPopupList(DataSourceRequest command, ProductModel.AddAssociatedProductModel model)
        {
            var items = _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = items.products.ToList();
            gridModel.Total = items.totalCount;
            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult AssociatedProductAddPopup(ProductModel.AddAssociatedProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _productViewModelService.InsertAssociatedProductModel(model);
            }
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Product pictures
        public IActionResult ProductPictureAdd(string pictureId, int displayOrder,
            string overrideAltAttribute, string overrideTitleAttribute,
            string productId)
        {
            if (String.IsNullOrEmpty(pictureId))
                throw new ArgumentException();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            _productViewModelService.InsertProductPicture(product, pictureId, displayOrder, overrideAltAttribute, overrideTitleAttribute);

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult ProductPictureList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            var productPicturesModel = _productViewModelService.PrepareProductPictureModel(product);
            var gridModel = new DataSourceResult
            {
                Data = productPicturesModel,
                Total = productPicturesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductPictureUpdate(ProductModel.ProductPictureModel model)
        {
            _productViewModelService.UpdateProductPicture(model);
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductPictureDelete(ProductModel.ProductPictureModel model)
        {
            _productViewModelService.DeleteProductPicture(model);
            return new NullJsonResult();
        }
        #endregion

        #region Product specification attributes
        //ajax
        [AcceptVerbs("GET")]
        public IActionResult GetOptionsByAttributeId(string attributeId, [FromServices] ISpecificationAttributeService specificationAttributeService)
        {
            if (String.IsNullOrEmpty(attributeId))
                throw new ArgumentNullException("attributeId");

            var options = specificationAttributeService.GetSpecificationAttributeById(attributeId).SpecificationAttributeOptions;
            var result = (from o in options
                          select new { id = o.Id, name = o.Name }).ToList();
            return Json(result);
        }

        public IActionResult ProductSpecificationAttributeAdd(ProductModel.AddProductSpecificationAttributeModel model, string productId)
        {
            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List");
                }
            }
            _productViewModelService.InsertProductSpecificationAttributeModel(model, product);

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult ProductSpecAttrList(DataSourceRequest command, string productId)
        {
            //a vendor should have access only to his products
            var product = _productService.GetProductById(productId);
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            var productrSpecsModel = _productViewModelService.PrepareProductSpecificationAttributeModel(product);
            var gridModel = new DataSourceResult
            {
                Data = productrSpecsModel,
                Total = productrSpecsModel.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductSpecAttrUpdate(ProductSpecificationAttributeModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            var psa = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeId == model.ProductSpecificationId).Where(x => x.Id == model.Id).FirstOrDefault();
            if (psa == null)
                return Content("No product specification attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            _productViewModelService.UpdateProductSpecificationAttributeModel(product, psa, model);
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductSpecAttrDelete(string id, string ProductSpecificationId, string productId)
        {
            var product = _productService.GetProductById(productId);
            var psa = product.ProductSpecificationAttributes.Where(x => x.Id == id && x.SpecificationAttributeId == ProductSpecificationId).FirstOrDefault();
            if (psa == null)
                throw new ArgumentException("No specification attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            _productViewModelService.DeleteProductSpecificationAttribute(product, psa);
            return new NullJsonResult();
        }

        #endregion
        
        #region Purchased with order

        [HttpPost]
        public IActionResult PurchasedWithOrders(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var orders = _productViewModelService.PrepareOrderModel(productId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.orderModels.ToList(),
                Total = orders.totalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Reviews

        [HttpPost]
        public IActionResult Reviews(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var productReviews = _productService.GetAllProductReviews("", null,
                null, null, "", null, productId);

            var gridModel = new DataSourceResult
            {
                Data = productReviews.PagedForCommand(command).Select(x =>
                {
                    var m = new ProductReviewModel();
                    _productViewModelService.PrepareProductReviewModel(m, x, false, true);
                    return m;
                }),
                Total = productReviews.Count,
            };

            return Json(gridModel);
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("download-catalog-pdf")]
        public IActionResult DownloadCatalogAsPdf(ProductListModel model, [FromServices] IPdfService pdfService)
        {
            var products = _productViewModelService.PrepareProducts(model);
            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    pdfService.PrintProductsToPdf(stream, products);
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


        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public IActionResult ExportXmlAll(ProductListModel model)
        {
            var products = _productViewModelService.PrepareProducts(model);
            try
            {
                var xml = _exportManager.ExportProductsToXml(products);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "products.xml");

            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportXmlSelected(string selectedIds)
        {
            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var xml = _exportManager.ExportProductsToXml(products);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "products.xml");

        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public IActionResult ExportExcelAll(ProductListModel model)
        {
            var products = _productViewModelService.PrepareProducts(model);
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

        [HttpPost]
        public IActionResult ExportExcelSelected(string selectedIds)
        {
            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            byte[] bytes = _exportManager.ExportProductsToXlsx(products);
            return File(bytes, "text/xls", "products.xlsx");
        }

        [HttpPost]
        public IActionResult ImportExcel(IFormFile importexcelfile)
        {
            //a vendor cannot import products
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportProductsFromXlsx(importexcelfile.OpenReadStream());
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

        public IActionResult BulkEdit()
        {
            var model = _productViewModelService.PrepareBulkEditListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult BulkEditSelect(DataSourceRequest command, BulkEditListModel model)
        {
            var items = _productViewModelService.PrepareBulkEditProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = items.bulkEditProductModels.ToList();
            gridModel.Total = items.totalCount;
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BulkEditUpdate(IEnumerable<BulkEditProductModel> products)
        {
            if (products != null)
            {
                _productViewModelService.UpdateBulkEdit(products.ToList());
            }
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult BulkEditDelete(IEnumerable<BulkEditProductModel> products)
        {
            if (products != null)
            {
                _productViewModelService.DeleteBulkEdit(products.ToList());
            }
            return new NullJsonResult();
        }

        #endregion

        #region Tier prices

        [HttpPost]
        public IActionResult TierPriceList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var tierPricesModel = _productViewModelService.PrepareTierPriceModel(product);
            var gridModel = new DataSourceResult
            {
                Data = tierPricesModel,
                Total = tierPricesModel.Count
            };
            return Json(gridModel);
        }

        public IActionResult TierPriceCreatePopup(string productId)
        {
            var model = new ProductModel.TierPriceModel();
            model.ProductId = productId;
            _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult TierPriceCreatePopup(ProductModel.TierPriceModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                var tierPrice = model.ToEntity();
                _productService.InsertTierPrice(tierPrice);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        public IActionResult TierPriceEditPopup(string id, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var tierPrice = product.TierPrices.Where(x => x.Id == id).FirstOrDefault();
            if (tierPrice == null)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = tierPrice.ToModel();
            model.ProductId = productId;
            _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult TierPriceEditPopup(string productId, ProductModel.TierPriceModel model)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var tierPrice = product.TierPrices.Where(x => x.Id == model.Id).FirstOrDefault();
            if (tierPrice == null)
                return RedirectToAction("List", "Product");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                tierPrice = model.ToEntity(tierPrice);
                _productService.UpdateTierPrice(tierPrice);

                ViewBag.RefreshPage = true;
                return View(model);
            }
            //stores
            _productViewModelService.PrepareTierPriceModel(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult TierPriceDelete(string id, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var tierPrice = product.TierPrices.Where(x => x.Id == id).FirstOrDefault();
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");
            tierPrice.ProductId = product.Id;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            if (ModelState.IsValid)
            {
                _productService.DeleteTierPrice(tierPrice);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Product attributes

        [HttpPost]
        public IActionResult ProductAttributeMappingList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var attributesModel = _productViewModelService.PrepareProductAttributeMappingModels(product);
            var gridModel = new DataSourceResult
            {
                Data = attributesModel,
                Total = attributesModel.Count
            };

            return Json(gridModel);
        }

        public IActionResult ProductAttributeMappingPopup(string productId, string productAttributeMappingId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");
            if (string.IsNullOrEmpty(productAttributeMappingId))
            {
                var model = _productViewModelService.PrepareProductAttributeMappingModel(product);
                return View(model);
            }
            else
            {
                var productAttributeMapping = product.ProductAttributeMappings.FirstOrDefault(x => x.Id == productAttributeMappingId);
                var model = _productViewModelService.PrepareProductAttributeMappingModel(product, productAttributeMapping);
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult ProductAttributeMappingPopup(ProductModel.ProductAttributeMappingModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                    _productViewModelService.InsertProductAttributeMappingModel(model);
                else
                    _productViewModelService.UpdateProductAttributeMappingModel(model);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            model = _productViewModelService.PrepareProductAttributeMappingModel(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeMappingDelete(string id, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == id).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            productAttributeMapping.ProductId = productId;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            if (ModelState.IsValid)
            {
                productAttributeService.DeleteProductAttributeMapping(productAttributeMapping);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        //edit
        public IActionResult ProductAttributeValidationRulesPopup(string id, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == id).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = _productViewModelService.PrepareProductAttributeMappingModel(productAttributeMapping);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValidationRulesPopup(ProductModel.ProductAttributeMappingModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                _productViewModelService.UpdateProductAttributeValidationRulesModel(productAttributeMapping, model);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.ValidationRulesAllowed = productAttributeMapping.ValidationRulesAllowed();
            model.AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId;
            return View(model);
        }

        #endregion

        #region Product attributes. Condition

        public IActionResult ProductAttributeConditionPopup(string productId, string productAttributeMappingId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");
            var model = _productViewModelService.PrepareProductAttributeConditionModel(product, productAttributeMapping);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeConditionPopup(ProductAttributeConditionModel model, IFormCollection form)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var formcollection = new Dictionary<string, string>();
            foreach (var item in form)
            {
                formcollection.Add(item.Key, item.Value);
            }
            _productViewModelService.UpdateProductAttributeConditionModel(product, productAttributeMapping, model, formcollection);
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Product attribute values

        //list
        public IActionResult EditAttributeValues(string productAttributeMappingId, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var productAttribute = productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId);
            var model = new ProductModel.ProductAttributeValueListModel
            {
                ProductName = product.Name,
                ProductId = product.Id,
                ProductAttributeName = productAttribute.Name,
                ProductAttributeMappingId = productAttributeMappingId,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValueList(string productAttributeMappingId, string productId, DataSourceRequest command)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var values = _productViewModelService.PrepareProductAttributeValueModels(product, productAttributeMapping);
            var gridModel = new DataSourceResult
            {
                Data = values,
                Total = values.Count()
            };
            return Json(gridModel);
        }

        //create
        public IActionResult ProductAttributeValueCreatePopup(string productAttributeMappingId, string productId, [FromServices] IPictureService pictureService)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = new ProductModel.ProductAttributeValueModel();
            model.ProductAttributeMappingId = productAttributeMappingId;
            model.ProductId = productId;

            //color squares
            model.DisplayColorSquaresRgb = productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";
            //image squares
            model.DisplayImageSquaresPicture = productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares;

            //default qantity for associated product
            model.Quantity = 1;

            //locales
            AddLocales(_languageService, model.Locales);

            //pictures
            model.ProductPictureModels = product.ProductPictures
                .Select(x => new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = product.Id,
                    PictureId = x.PictureId,
                    PictureUrl = pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValueCreatePopup(ProductModel.ProductAttributeValueModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No product attribute found with the specified id
                return RedirectToAction("List", "Product");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && String.IsNullOrEmpty(model.ImageSquaresPictureId))
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
            {
                _productViewModelService.InsertProductAttributeValueModel(model);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            _productViewModelService.PrepareProductAttributeValueModel(product, model);
            return View(model);
        }

        //edit
        public IActionResult ProductAttributeValueEditPopup(string id, string productId, string productAttributeMappingId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pa = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (pa == null)
                return RedirectToAction("List", "Product");

            var pav = pa.ProductAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");
            var model = _productViewModelService.PrepareProductAttributeValueModel(pa, pav);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pav.GetLocalized(x => x.Name, languageId, false, false);
            });
            //pictures
            _productViewModelService.PrepareProductAttributeValueModel(product, model);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValueEditPopup(string productId, ProductModel.ProductAttributeValueModel model)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pav = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault().ProductAttributeValues.Where(x => x.Id == model.Id).FirstOrDefault();
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");
            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && String.IsNullOrEmpty(model.ImageSquaresPictureId))
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
            {
                _productViewModelService.UpdateProductAttributeValueModel(pav, model);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            _productViewModelService.PrepareProductAttributeValueModel(product, model);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult ProductAttributeValueDelete(string Id, string pam, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pav = product.ProductAttributeMappings.Where(x => x.Id == pam).FirstOrDefault().ProductAttributeValues.Where(x => x.Id == Id).FirstOrDefault();
            if (pav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            pav.ProductAttributeMappingId = pam;
            pav.ProductId = productId;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");
            if (ModelState.IsValid)
            {
                productAttributeService.DeleteProductAttributeValue(pav);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult AssociateProductToAttributeValuePopup()
        {
            var model = _productViewModelService.PrepareAssociateProductToAttributeValueModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult AssociateProductToAttributeValuePopupList(DataSourceRequest command,
            ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            var items = _productViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = items.products.ToList();
            gridModel.Total = items.totalCount;
            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult AssociateProductToAttributeValuePopup(ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            var associatedProduct = _productService.GetProductById(model.AssociatedToProductId);
            if (associatedProduct == null)
                return Content("Cannot load a product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            ViewBag.productId = associatedProduct.Id;
            ViewBag.productName = associatedProduct.Name;
            return View(model);
        }

        #endregion

        #region Product attribute combinations

        [HttpPost]
        public IActionResult ProductAttributeCombinationList(DataSourceRequest command, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var combinationsModel = _productViewModelService.PrepareProductAttributeCombinationModel(product);
            var gridModel = new DataSourceResult
            {
                Data = combinationsModel,
                Total = combinationsModel.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationDelete(string id, string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var combination = product.ProductAttributeCombinations.Where(x => x.Id == id).FirstOrDefault();
            if (combination == null)
                throw new ArgumentException("No product attribute combination found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            combination.ProductId = productId;

            productAttributeService.DeleteProductAttributeCombination(combination);
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var pr = _productService.GetProductById(productId);
                pr.StockQuantity = pr.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                _productService.UpdateStockProduct(pr);
            }

            return new NullJsonResult();
        }

        //edit
        public IActionResult AttributeCombinationPopup(string productId, string Id)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");
            var model = _productViewModelService.PrepareProductAttributeCombinationModel(product, Id);
            _productViewModelService.PrepareAddProductAttributeCombinationModel(model, product);
            return View(model);
        }

        [HttpPost]

        public IActionResult AttributeCombinationPopup(string productId,
            ProductAttributeCombinationModel model, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var formcollection = new Dictionary<string, string>();
            foreach (var item in form)
            {
                formcollection.Add(item.Key, item.Value);
            }
            var warnings = _productViewModelService.InsertOrUpdateProductAttributeCombinationPopup(product, model, formcollection);
            if (!warnings.Any())
            {
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            _productViewModelService.PrepareAddProductAttributeCombinationModel(model, product);
            model.Warnings = warnings;
            return View(model);
        }

        [HttpPost]
        public IActionResult GenerateAllAttributeCombinations(string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productViewModelService.GenerateAllAttributeCombinations(product);

            return Json(new { Success = true });
        }

        [HttpPost]
        public IActionResult ClearAllAttributeCombinations(string productId, [FromServices] IProductAttributeService productAttributeService)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");
            if (ModelState.IsValid)
            {
                foreach (var combination in product.ProductAttributeCombinations.ToList())
                {
                    combination.ProductId = productId;
                    productAttributeService.DeleteProductAttributeCombination(combination);
                }

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                {
                    product.StockQuantity = 0;
                    _productService.UpdateStockProduct(product);
                }
                return Json(new { Success = true });
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #region Product Attribute combination - tier prices

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceList(DataSourceRequest command, string productId, string productAttributeCombinationId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var tierPriceModel = _productViewModelService.PrepareProductAttributeCombinationTierPricesModel(product, productAttributeCombinationId);
            var gridModel = new DataSourceResult
            {
                Data = tierPriceModel,
                Total = tierPriceModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceInsert(string productId, string productAttributeCombinationId, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");
            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
                _productViewModelService.InsertProductAttributeCombinationTierPricesModel(product, combination, model);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceUpdate(string productId, string productAttributeCombinationId, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
                _productViewModelService.UpdateProductAttributeCombinationTierPricesModel(product, combination, model);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceDelete(string productId, string productAttributeCombinationId, string id)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
            {
                var tierPrice = combination.TierPrices.FirstOrDefault(x => x.Id == id);
                if (tierPrice != null)
                {
                    _productViewModelService.DeleteProductAttributeCombinationTierPrices(product, combination, tierPrice);
                }
            }
            return new NullJsonResult();
        }
        #endregion
        #endregion

        #region Activity log

        [HttpPost]
        public IActionResult ListActivityLog(DataSourceRequest command, string productId)
        {
            var activityLog = _productViewModelService.PrepareActivityLogModel(productId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.activityLogModels.ToList(),
                Total = activityLog.totalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Reservation

        [HttpPost]
        public IActionResult ListReservations(DataSourceRequest command, string productId)
        {
            var reservations = _productReservationService.GetProductReservationsByProductId(productId, null, null, command.Page - 1, command.PageSize);
            var reservationModel = reservations
                .Select(x => new ProductModel.ReservationModel
                {
                    ReservationId = x.Id,
                    Date = x.Date,
                    OrderId = x.OrderId,
                    Parameter = x.Parameter,
                    Resource = x.Resource,
                    Duration = x.Duration
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = reservationModel,
                Total = reservations.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult GenerateCalendar(string productId, ProductModel.GenerateCalendarModel model)
        {
            var reservations = _productReservationService.GetProductReservationsByProductId(productId, null, null);
            if (reservations.Any())
            {
                var product = _productService.GetProductById(productId);
                if (product == null)
                    throw new ArgumentNullException("product");

                if (((product.IntervalUnitType == IntervalUnit.Minute || product.IntervalUnitType == IntervalUnit.Hour) && (IntervalUnit)model.Interval == IntervalUnit.Day) ||
                    (product.IntervalUnitType == IntervalUnit.Day) && (((IntervalUnit)model.IntervalUnit == IntervalUnit.Minute || (IntervalUnit)model.IntervalUnit == IntervalUnit.Hour)))
                {
                    return Json(new { errors = _localizationService.GetResource("Admin.Catalog.Products.Calendar.CannotChangeInterval") });
                }
            }
            _productService.UpdateIntervalProperties(productId, model.Interval, (IntervalUnit)model.IntervalUnit, model.IncBothDate);

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

                            _productReservationService.InsertProductReservation(new ProductReservation
                            {
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

        public IActionResult ClearCalendar(string productId)
        {
            var toDelete = _productReservationService.GetProductReservationsByProductId(productId, true, null);
            foreach (var record in toDelete)
            {
                _productReservationService.DeleteProductReservation(record);
            }

            return Json("");
        }

        public IActionResult ClearOld(string productId)
        {
            var toDelete = _productReservationService.GetProductReservationsByProductId(productId, true, null).Where(x => x.Date < DateTime.UtcNow);
            foreach (var record in toDelete)
            {
                _productReservationService.DeleteProductReservation(record);
            }

            return Json("");
        }

        [HttpPost]
        public IActionResult ProductReservationDelete(ProductModel.ReservationModel model)
        {
            var toDelete = _productReservationService.GetProductReservation(model.ReservationId);
            if (toDelete != null)
            {
                if (string.IsNullOrEmpty(toDelete.OrderId))
                    _productReservationService.DeleteProductReservation(toDelete);
                else
                    return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.ProductReservations.CantDeleteWithOrder") });
            }

            return Json("");
        }

        #endregion

        #region Bids

        [HttpPost]
        public IActionResult ListBids(DataSourceRequest command, string productId)
        {
            var bids = _productViewModelService.PrepareBidMode(productId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = bids.bidModels.ToList(),
                Total = bids.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BidDelete(ProductModel.BidModel model, [FromServices] ICustomerActivityService customerActivityService)
        {
            var toDelete = _auctionService.GetBid(model.BidId);
            if (toDelete != null)
            {
                var product = _productService.GetProductById(toDelete.ProductId);
                if (product == null)
                    return Json(new DataSourceResult { Errors = "Product not exists" });

                if (string.IsNullOrEmpty(toDelete.OrderId))
                {
                    //activity log
                    customerActivityService.InsertActivity("DeleteBid", toDelete.ProductId, _localizationService.GetResource("ActivityLog.DeleteBid"), product.Name);
                    //delete bid
                    _auctionService.DeleteBid(toDelete);
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