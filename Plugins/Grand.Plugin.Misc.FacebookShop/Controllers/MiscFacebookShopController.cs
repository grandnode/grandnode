﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Plugin.Misc.FacebookShop.Infrastructure.Cache;
using Grand.Plugin.Misc.FacebookShop.Models;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Security;
using MongoDB.Driver.Linq;

namespace Grand.Plugin.Misc.FacebookShop.Controllers
{
    [NopHttpsRequirement(SslRequirement.NoMatter)]
    public class MiscFacebookShopController : BasePluginController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICacheManager _cacheManager;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;

        #endregion
        
        #region Ctor

        public MiscFacebookShopController(IAclService aclService,
            ICacheManager cacheManager,
            CatalogSettings catalogSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IWorkContext workContext)
        {
            this._aclService = aclService;
            this._cacheManager = cacheManager;
            this._catalogSettings = catalogSettings;
            this._categoryService = categoryService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productService = productService;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._taxService = taxService;
            this._workContext = workContext;
        }
        
        #endregion
        
        #region Utilities

        //just copy this method from CatalogController (removed some redundant code)
        [NonAction]
        protected IList<CategoryModel> PrepareCategorySimpleModels(string rootCategoryId,
            IList<string> loadSubCategoriesForIds, bool validateIncludeInTopMenu)
        {
            var result = new List<CategoryModel>();
            foreach (var category in _categoryService.GetAllCategoriesByParentCategoryId(rootCategoryId))
            {
                if (validateIncludeInTopMenu && !category.IncludeInTopMenu)
                {
                    continue;
                }

                var categoryModel = new CategoryModel
                {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name),
                    SeName = category.GetSeName()
                };

                //load subcategories?
                bool loadSubCategories = false;
                if (loadSubCategoriesForIds == null)
                {
                    //load all subcategories
                    loadSubCategories = true;
                }
                else
                {
                    //we load subcategories only for certain categories
                    for (int i = 0; i <= loadSubCategoriesForIds.Count - 1; i++)
                    {
                        if (loadSubCategoriesForIds[i] == category.Id)
                        {
                            loadSubCategories = true;
                            break;
                        }
                    }
                }
                if (loadSubCategories)
                {
                    var subCategories = PrepareCategorySimpleModels(category.Id, loadSubCategoriesForIds, validateIncludeInTopMenu);
                    categoryModel.SubCategories.AddRange(subCategories);
                }
                result.Add(categoryModel);
            }

            return result;
        }

        //just copy this method from CatalogController (removed some redundant code)
        [NonAction]
        protected IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription),
                    SeName = product.GetSeName(),
                };
                //price
                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel();

                    switch (product.ProductType)
                    {
                        case ProductType.GroupedProduct:
                            {
                                #region Grouped product

                                var associatedProducts = _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);

                                switch (associatedProducts.Count)
                                {
                                    case 0:
                                        {
                                            //no associated products
                                            priceModel.OldPrice = null;
                                            priceModel.Price = null;
                                            priceModel.DisableBuyButton = true;
                                            priceModel.DisableWishlistButton = true;
                                            priceModel.AvailableForPreOrder = false;
                                        }
                                        break;
                                    default:
                                        {
                                            //we have at least one associated product
                                            priceModel.DisableBuyButton = true;
                                            priceModel.DisableWishlistButton = true;
                                            priceModel.AvailableForPreOrder = false;

                                            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                            {
                                                //find a minimum possible price
                                                decimal? minPossiblePrice = null;
                                                Product minPriceProduct = null;
                                                foreach (var associatedProduct in associatedProducts)
                                                {
                                                    //calculate for the maximum quantity (in case if we have tier prices)
                                                    var tmpPrice = _priceCalculationService.GetFinalPrice(associatedProduct,
                                                        _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);
                                                    if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
                                                    {
                                                        minPriceProduct = associatedProduct;
                                                        minPossiblePrice = tmpPrice;
                                                    }
                                                }
                                                if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
                                                {
                                                    if (minPriceProduct.CallForPrice)
                                                    {
                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                                    }
                                                    else if (minPossiblePrice.HasValue)
                                                    {
                                                        //calculate prices
                                                        decimal taxRate;
                                                        decimal finalPriceBase = _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                                                        decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));

                                                    }
                                                    else
                                                    {
                                                        //Actually it's not possible (we presume that minimalPrice always has a value)
                                                        //We never should get here
                                                        Debug.WriteLine("Cannot calculate minPrice for product #{0}", product.Id);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //hide prices
                                                priceModel.OldPrice = null;
                                                priceModel.Price = null;
                                            }
                                        }
                                        break;
                                }

                                #endregion
                            }
                            break;
                        case ProductType.SimpleProduct:
                        default:
                            {
                                #region Simple product

                                //add to cart button
                                priceModel.DisableBuyButton = product.DisableBuyButton ||
                                    !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                                    !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //add to wishlist button
                                priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                    !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                                    !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                                //rental
                                priceModel.IsRental = product.IsRental;

                                //pre-order
                                if (product.AvailableForPreOrder)
                                {
                                    priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                        product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                                    priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
                                }

                                //prices
                                if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                {
                                    if (!product.CustomerEntersPrice)
                                    {
                                        if (product.CallForPrice)
                                        {
                                            //call for price
                                            priceModel.OldPrice = null;
                                            priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                        }
                                        else
                                        {
                                            //prices

                                            //calculate for the maximum quantity (in case if we have tier prices)
                                            decimal minPossiblePrice = _priceCalculationService.GetFinalPrice(product,
                                                _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

                                            decimal taxRate;
                                            decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                                            decimal finalPriceBase = _taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

                                            decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                            decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                            //do we have tier prices configured?
                                            var tierPrices = new List<TierPrice>();
                                            if (product.HasTierPrices)
                                            {
                                                tierPrices.AddRange(product.TierPrices
                                                    .OrderBy(tp => tp.Quantity)
                                                    .ToList()
                                                    .FilterByStore(_storeContext.CurrentStore.Id)
                                                    .FilterForCustomer(_workContext.CurrentCustomer)
                                                    .RemoveDuplicatedQuantities());
                                            }
                                            //When there is just one tier (with  qty 1), 
                                            //there are no actual savings in the list.
                                            bool displayFromMessage = tierPrices.Count > 0 &&
                                                !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                            if (displayFromMessage)
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
                                            }
                                            else
                                            {
                                                if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                                {
                                                    priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice);
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                                }
                                                else
                                                {
                                                    priceModel.OldPrice = null;
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                                }
                                            }
                                            if (product.IsRental)
                                            {
                                                //rental product
                                                priceModel.OldPrice = _priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
                                                priceModel.Price = _priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //hide prices
                                    priceModel.OldPrice = null;
                                    priceModel.Price = null;
                                }

                                #endregion
                            }
                            break;
                    }

                    model.ProductPrice = priceModel;

                    #endregion
                }

                //picture
                if (preparePictureModel)
                {
                    #region Prepare product picture

                    //If a size has been set in the view, we use it in priority
                    int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : 125;
                    //prepare picture model
                    //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                    var picture = product.ProductPictures.FirstOrDefault(); //_pictureService.GetPictureById(product.prodId, 1).FirstOrDefault();
                    if (picture == null)
                        picture = new ProductPicture();
                    model.DefaultPictureModel = new PictureModel
                        {
                            ImageUrl = _pictureService.GetPictureUrl(picture.PictureId, pictureSize),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture.PictureId)
                        };
                    //"title" attribute
                    model.DefaultPictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                        picture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
                    //"alt" attribute
                    model.DefaultPictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                        picture.AltAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);
                        
                    #endregion
                }

                models.Add(model);
            }
            return models;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        [AdminAuthorize]
        public ActionResult Configure()
        {
            return View("~/Plugins/Misc.FacebookShop/Views/MiscFacebookShop/Configure.cshtml");
        }

        public ActionResult Index()
        {
            return View("~/Plugins/Misc.FacebookShop/Views/MiscFacebookShop/Index.cshtml");
        }

        public ActionResult HomePageProducts()
        {
            var products = _productService.GetAllProductsDisplayedOnHomePage();
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            var model = PrepareProductOverviewModels(products).ToList();
            return PartialView("~/Plugins/Misc.FacebookShop/Views/MiscFacebookShop/HomePageProducts.cshtml", model);
        }

        public ActionResult CategoryNavigation()
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NAVIGATION_MODEL_KEY, 
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var model = _cacheManager.Get(cacheKey, () => PrepareCategorySimpleModels("", null, true).ToList());

            return PartialView("~/Plugins/Misc.FacebookShop/Views/MiscFacebookShop/CategoryNavigation.cshtml", model);
        }

        public ActionResult Category(string categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null )
                return Content("");

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return Content("");

            //ACL (access control list)
            if (!_aclService.Authorize(category))
                return Content("");

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return Content("");

            if (command.PageSize == 0)
            {
                command.PageSize = 8;
            }
            if (command.PageNumber <= 0)
            {
                command.PageNumber = 1;
            }

            var model = new CategoryModel
            {
                Id = category.Id,
                Name = category.GetLocalized(x => x.Name),
                SeName = category.GetSeName(),
                Description = category.GetLocalized(x => x.Description)
            };

            //subcategories
            model.SubCategories = _categoryService
                .GetAllCategoriesByParentCategoryId(categoryId)
                .Select(x =>
                {
                    var subCatName = x.GetLocalized(y => y.Name);
                    var subCatModel = new CategoryModel
                    {
                        Id = x.Id,
                        Name = subCatName,
                        SeName = x.GetSeName(),
                    };

                    //prepare picture model
                    int pictureSize = 125;
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    subCatModel.PictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), subCatName),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), subCatName)
                        };

                    return subCatModel;
                })
                .ToList();

            var categoryIds = new List<string>();
            if (!String.IsNullOrEmpty(categoryId))
            {
                categoryIds.Add(categoryId);
            }
            //products
            IList<string> filterableSpecificationAttributeOptionIds;
            var products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds,
                categoryIds: categoryIds,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: null,
                orderBy: ProductSortingEnum.Position,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            return PartialView("~/Plugins/Misc.FacebookShop/Views/MiscFacebookShop/Category.cshtml", model);
        }
        
        [ValidateInput(false)]
        public ActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            if (model == null)
                model = new SearchModel();

            if (command.PageSize == 0)
            {
                command.PageSize = 8;
            }
            if (command.PageNumber <= 0)
            {
                command.PageNumber = 1;
            }

            if (model.Q == null)
                model.Q = "";
            model.Q = model.Q.Trim();

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            if (Request.Params["Q"] != null)
            {
                if (model.Q.Length < _catalogSettings.ProductSearchTermMinimumLength)
                {
                    model.Warning = string.Format(_localizationService.GetResource("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    //products
                    products = _productService.SearchProducts(
                        storeId: _storeContext.CurrentStore.Id,
                        visibleIndividuallyOnly: true,
                        keywords: model.Q,
                        languageId: _workContext.WorkingLanguage.Id,
                        pageIndex: command.PageNumber - 1,
                        pageSize: command.PageSize);
                    model.Products = PrepareProductOverviewModels(products).ToList();

                    model.NoResults = !model.Products.Any();
                }
            }

            model.PagingFilteringContext.LoadPagedList(products);

            return View("~/Plugins/Misc.FacebookShop/Views/MiscFacebookShop/Search.cshtml", model);
        }

        public ActionResult Footer()
        {
            var model = new FooterModel
            {
                StoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name)
            };

            return PartialView("~/Plugins/Misc.FacebookShop/Views/MiscFacebookShop/Footer.cshtml", model);
        }

        #endregion
    }
}   