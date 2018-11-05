using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Vendors;
using Grand.Framework.Events;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Services.Vendors;
using Grand.Web.Extensions;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial class CatalogViewModelService : ICatalogViewModelService
    {
        private readonly IWebHelper _webHelper;
        private readonly IProductViewModelService _productViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly ITopicService _topicService;
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;
        private readonly IProductTagService _productTagService;
        private readonly ICurrencyService _currencyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISearchTermService _searchTermService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IBlogService _blogService;
        private readonly CatalogSettings _catalogSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly MenuItemSettings _menuItemSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;

        public CatalogViewModelService(
            IWebHelper webHelper,
            IProductViewModelService productViewModelService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICacheManager cacheManager,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            ITopicService topicService,
            IPictureService pictureService,
            IVendorService vendorService,
            IProductTagService productTagService,
            ICurrencyService currencyService,
            IHttpContextAccessor httpContextAccessor,
            ISearchTermService searchTermService,
            IEventPublisher eventPublisher,
            IAclService aclService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            ISpecificationAttributeService specificationAttributeService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
            IPriceFormatter priceFormatter,
            IAddressViewModelService addressViewModelService,
            IBlogService blogService,
            CatalogSettings catalogSettings,
            BlogSettings blogSettings,
            ForumSettings forumSettings,
            MenuItemSettings menuItemSettings,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings)
        {
            this._webHelper = webHelper;
            this._productViewModelService = productViewModelService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._topicService = topicService;
            this._pictureService = pictureService;
            this._vendorService = vendorService;
            this._productTagService = productTagService;
            this._currencyService = currencyService;
            this._httpContextAccessor = httpContextAccessor;
            this._searchTermService = searchTermService;
            this._eventPublisher = eventPublisher;
            this._aclService = aclService;
            this._permissionService = permissionService;
            this._storeMappingService = storeMappingService;
            this._specificationAttributeService = specificationAttributeService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._priceFormatter = priceFormatter;
            this._addressViewModelService = addressViewModelService;
            this._blogService = blogService;
            this._catalogSettings = catalogSettings;
            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._menuItemSettings = menuItemSettings;
            this._mediaSettings = mediaSettings;
            this._vendorSettings = vendorSettings;
        }

        #region Sort,view,size options

        public virtual void PrepareSortingOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException("pagingFilteringModel");

            if (command == null)
                throw new ArgumentNullException("command");

            var allDisabled = _catalogSettings.ProductSortingEnumDisabled.Count == Enum.GetValues(typeof(ProductSortingEnum)).Length;
            pagingFilteringModel.AllowProductSorting = _catalogSettings.AllowProductSorting && !allDisabled;

            var activeOptions = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled)
                .Select((idOption) =>
                {
                    int order;
                    return new KeyValuePair<int, int>(idOption, _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out order) ? order : idOption);
                })
                .OrderBy(x => x.Value);
            if (command.OrderBy == null)
                command.OrderBy = allDisabled ? 0 : activeOptions.First().Key;

            if (pagingFilteringModel.AllowProductSorting)
            {
                foreach (var option in activeOptions)
                {
                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "orderby=" + (option.Key).ToString(), null);

                    var sortValue = ((ProductSortingEnum)option.Key).GetLocalizedEnum(_localizationService, _workContext);
                    pagingFilteringModel.AvailableSortOptions.Add(new SelectListItem
                    {
                        Text = sortValue,
                        Value = sortUrl,
                        Selected = option.Key == command.OrderBy
                    });
                }
            }
        }
        public virtual void PrepareViewModes(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException("pagingFilteringModel");

            if (command == null)
                throw new ArgumentNullException("command");

            pagingFilteringModel.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _catalogSettings.DefaultViewMode;
            pagingFilteringModel.ViewMode = viewMode;
            if (pagingFilteringModel.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource("Catalog.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
                    Selected = viewMode == "grid"
                });
                //list
                pagingFilteringModel.AvailableViewModes.Add(new SelectListItem
                {
                    Text = _localizationService.GetResource("Catalog.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
                    Selected = viewMode == "list"
                });
            }
        }

        public virtual void PreparePageSizeOptions(CatalogPagingFilteringModel pagingFilteringModel, CatalogPagingFilteringModel command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException("pagingFilteringModel");

            if (command == null)
                throw new ArgumentNullException("command");

            if (command.PageNumber <= 0)
            {
                command.PageNumber = 1;
            }
            pagingFilteringModel.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        int temp;
                        if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                        {
                            if (temp > 0)
                            {
                                command.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "pagesize={0}", null);
                    sortUrl = _webHelper.RemoveQueryString(sortUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        int temp;
                        if (!int.TryParse(pageSize, out temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        pagingFilteringModel.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = String.Format(sortUrl, pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.OrdinalIgnoreCase)
                        });
                    }

                    if (pagingFilteringModel.PageSizeOptions.Any())
                    {
                        pagingFilteringModel.PageSizeOptions = pagingFilteringModel.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        pagingFilteringModel.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                        {
                            command.PageSize = int.Parse(pagingFilteringModel.PageSizeOptions.FirstOrDefault().Text);
                        }
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = fixedPageSize;
            }
        }

        #endregion

        #region Category

        public virtual Category GetCategoryById(string categoryId)
        {
            return _categoryService.GetCategoryById(categoryId);
        }

        public virtual List<string> GetChildCategoryIds(string parentCategoryId)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY,
                parentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var categoriesIds = new List<string>();
                var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId);
                foreach (var category in categories)
                {
                    categoriesIds.Add(category.Id);
                    categoriesIds.AddRange(GetChildCategoryIds(category.Id));
                }
                return categoriesIds;
            });
        }


        public virtual List<CategorySimpleModel> PrepareCategorySimpleModels()
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_ALL_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () => PrepareCategorySimpleModels(""));
        }

        public virtual List<CategorySimpleModel> PrepareCategorySimpleModels(string rootCategoryId,
            bool loadSubCategories = true, IList<Category> allCategories = null)
        {
            var result = new List<CategorySimpleModel>();
            var langid = _workContext.WorkingLanguage.Id;
            if (allCategories == null)
            {
                allCategories = _categoryService.GetAllCategories(storeId: _storeContext.CurrentStore.Id);
            }
            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).ToList();
            foreach (var category in categories)
            {
                var categoryModel = new CategorySimpleModel
                {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name, langid),
                    SeName = category.GetSeName(langid),
                    IncludeInTopMenu = category.IncludeInTopMenu,
                    Flag = category.Flag,
                    FlagStyle = category.FlagStyle,
                    Icon = category.Icon,
                    ImageUrl = _pictureService.GetPictureUrl(category.PictureId, _mediaSettings.CategoryThumbPictureSize)
                };

                //product number for each category
                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        category.Id);
                    categoryModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var categoryIds = new List<string>();
                        categoryIds.Add(category.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(GetChildCategoryIds(category.Id));
                        return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                    });
                }

                if (loadSubCategories)
                {
                    var subCategories = PrepareCategorySimpleModels(category.Id, loadSubCategories, allCategories);
                    categoryModel.SubCategories.AddRange(subCategories);
                }
                result.Add(categoryModel);
            }

            return result;
        }

        public virtual CategoryNavigationModel PrepareCategoryNavigation(string currentCategoryId, string currentProductId)
        {
            //get active category
            string activeCategoryId = "";
            if (!String.IsNullOrEmpty(currentCategoryId))
            {
                //category details page
                activeCategoryId = currentCategoryId;
            }
            else if (!String.IsNullOrEmpty(currentProductId))
            {
                //product details page
                var productCategories = _productService.GetProductById(currentProductId).ProductCategories;
                if (productCategories.Any())
                    activeCategoryId = productCategories.FirstOrDefault().CategoryId;
            }
            var cachedModel = PrepareCategorySimpleModels();
            var model = new CategoryNavigationModel
            {
                CurrentCategoryId = activeCategoryId,
                Categories = cachedModel
            };

            return model;
        }

        public virtual string PrepareCategoryTemplateViewPath(string templateId)
        {
            var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, templateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _categoryTemplateService.GetCategoryTemplateById(templateId);
                if (template == null)
                    template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            return templateViewPath;
        }

        public virtual CategoryModel PrepareCategory(Category category, CatalogPagingFilteringModel command)
        {
            var model = category.ToModel();
            var customer = _workContext.CurrentCustomer;
            var storeId = _storeContext.CurrentStore.Id;
            var languageId = _workContext.WorkingLanguage.Id;
            var currency = _workContext.WorkingCurrency;
            var connectionSecured = _webHelper.IsCurrentConnectionSecured();
            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                category.AllowCustomersToSelectPageSize,
                category.PageSizeOptions,
                category.PageSize);

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(category.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, category.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, currency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, currency);
            }


            //category breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCategoryBreadcrumb = true;

                string breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_BREADCRUMB_KEY,
                    category.Id,
                    string.Join(",", customer.GetCustomerRoleIds()),
                    storeId,
                    languageId);
                model.CategoryBreadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                    category
                    .GetCategoryBreadCrumb(_categoryService, _aclService, _storeMappingService)
                    .Select(catBr => new CategoryModel
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name, languageId),
                        SeName = catBr.GetSeName(languageId)
                    })
                    .ToList()
                );
            }

            //subcategories
            string subCategoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_SUBCATEGORIES_KEY,
                category.Id,
                string.Join(",", customer.GetCustomerRoleIds()),
                storeId,
                languageId,
                connectionSecured);
            model.SubCategories = _cacheManager.Get(subCategoriesCacheKey, () => _categoryService.GetAllCategoriesByParentCategoryId(category.Id).Where(x => !x.HideOnCatalog)
                .Select(x =>
                {
                    var subCatModel = new CategoryModel.SubCategoryModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name, languageId),
                        SeName = x.GetSeName(languageId),
                        Description = x.GetLocalized(y => y.Description, languageId),
                        Flag = x.Flag,
                        FlagStyle = x.FlagStyle
                    };
                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    subCatModel.PictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), subCatModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), subCatModel.Name)
                    };
                    return subCatModel;
                })
                .ToList()
            );

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                //We cache a value indicating whether we have featured products
                IPagedList<Product> featuredProducts = null;
                string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY, category.Id,
                    string.Join(",", customer.GetCustomerRoleIds()), storeId);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       categoryIds: new List<string> { category.Id },
                       storeId: storeId,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    hasFeaturedProductsCache = featuredProducts.Any();
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the category has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       categoryIds: new List<string> { category.Id },
                       storeId: storeId,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                }
                if (featuredProducts != null)
                {
                    model.FeaturedProducts = _productViewModelService.PrepareProductOverviewModels(featuredProducts).ToList();
                }
            }


            var categoryIds = new List<string>();
            categoryIds.Add(category.Id);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                //include subcategories
                categoryIds.AddRange(GetChildCategoryIds(category.Id));
            }
            //products
            IList<string> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
            IList<string> filterableSpecificationAttributeOptionIds;
            var products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds, true,
                categoryIds: categoryIds,
                storeId: storeId,
                visibleIndividuallyOnly: true,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                filteredSpecs: alreadyFilteredSpecOptionIds,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = _productViewModelService.PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            //specs
            model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                filterableSpecificationAttributeOptionIds,
                _specificationAttributeService, _webHelper, _workContext, _cacheManager);

            return model;
        }

        public virtual List<CategoryModel> PrepareHomepageCategory()
        {
            string categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_HOMEPAGE_KEY,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());

            var model = _cacheManager.Get(categoriesCacheKey, () =>
                _categoryService.GetAllCategoriesDisplayedOnHomePage()
                .Select(x =>
                {
                    var catModel = x.ToModel();
                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    catModel.PictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                    };
                    return catModel;
                })
                .ToList()
            );
            return model;
        }


        public virtual List<CategoryModel> PrepareCategoryFeaturedProducts()
        {
            string categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_FEATURED_PRODUCTS_HOMEPAGE_KEY,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured());

            var model = _cacheManager.Get(categoriesCacheKey, () =>
                _categoryService.GetAllCategoriesFeaturedProductsOnHomePage()
                .Select(x =>
                {
                    var catModel = x.ToModel();
                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    catModel.PictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                    };
                    return catModel;
                })
                .ToList()
            );

            foreach (var item in model)
            {
                //We cache a value indicating whether we have featured products
                IPagedList<Product> featuredProducts = null;
                string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY, item.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       categoryIds: new List<string> { item.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    hasFeaturedProductsCache = featuredProducts.Any();
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the category has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       categoryIds: new List<string> { item.Id },
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                }
                if (featuredProducts != null)
                {
                    item.FeaturedProducts = _productViewModelService.PrepareProductOverviewModels(featuredProducts).ToList();
                }
            }

            return model;
        }
        #endregion

        #region Top menu

        public virtual TopMenuModel PrepareTopMenu()
        {
            var languageId = _workContext.WorkingLanguage.Id;

            //categories
            var cachedCategoriesModel = PrepareCategorySimpleModels();
            //top menu topics
            string topicCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_TOP_MENU_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cachedTopicModel = _cacheManager.Get(topicCacheKey, () =>
                _topicService.GetAllTopics(_storeContext.CurrentStore.Id)
                .Where(t => t.IncludeInTopMenu)
                .Select(t => new TopMenuModel.TopMenuTopicModel
                {
                    Id = t.Id,
                    Name = t.GetLocalized(x => x.Title, languageId),
                    SeName = t.GetSeName(languageId)
                })
                .ToList()
            );

            string manufacturerCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MENU,
                _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);

            var cachedManufacturerModel = _cacheManager.Get(manufacturerCacheKey, () =>
                _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id)
                    .Where(x => x.IncludeInTopMenu)
                    .Select(t => new TopMenuModel.TopMenuManufacturerModel
                    {
                        Id = t.Id,
                        Name = t.GetLocalized(x => x.Name, languageId),
                        Icon = t.Icon,
                        SeName = t.GetSeName(languageId)
                    })
                    .ToList()
                );


            var model = new TopMenuModel
            {
                Categories = cachedCategoriesModel,
                Topics = cachedTopicModel,
                Manufacturers = cachedManufacturerModel,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                DisplayHomePageMenu = _menuItemSettings.DisplayHomePageMenu,
                DisplayNewProductsMenu = _menuItemSettings.DisplayNewProductsMenu,
                DisplaySearchMenu = _menuItemSettings.DisplaySearchMenu,
                DisplayCustomerMenu = _menuItemSettings.DisplayCustomerMenu,
                DisplayBlogMenu = _menuItemSettings.DisplayBlogMenu,
                DisplayForumsMenu = _menuItemSettings.DisplayForumsMenu,
                DisplayContactUsMenu = _menuItemSettings.DisplayContactUsMenu
            };

            return model;

        }

        #endregion

        #region Manufacturer

        public virtual Manufacturer GetManufacturerById(string manufacturerId)
        {
            return _manufacturerService.GetManufacturerById(manufacturerId);
        }

        public virtual string PrepareManufacturerTemplateViewPath(string templateId)
        {
            var templateCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_TEMPLATE_MODEL_KEY, templateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _manufacturerTemplateService.GetManufacturerTemplateById(templateId);
                if (template == null)
                    template = _manufacturerTemplateService.GetAllManufacturerTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });
            return templateViewPath;
        }

        public virtual List<ManufacturerModel> PrepareManufacturerAll()
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = manufacturer.ToModel();

                //prepare picture model
                int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
                var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                modelMan.PictureModel = new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                    ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                };
                model.Add(modelMan);
            }
            return model;
        }

        public virtual List<ManufacturerModel> PrepareHomepageManufacturers()
        {
            string manufacturersCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_HOMEPAGE_KEY,
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id);

            List<ManufacturerModel> model = _cacheManager.Get(manufacturersCacheKey, () =>
                _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id)
                .Where(x => x.ShowOnHomePage)
                .Select(x =>
                {
                    var manModel = x.ToModel();
                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    manModel.PictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), manModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), manModel.Name)
                    };
                    return manModel;
                })
                .ToList()
            );
            return model;
        }
        public virtual ManufacturerNavigationModel PrepareManufacturerNavigation(string currentManufacturerId)
        {
            var languageId = _workContext.WorkingLanguage.Id;

            string cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY,
                currentManufacturerId, languageId, string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);
                var manufacturers = _manufacturerService.GetAllManufacturers(pageSize: _catalogSettings.ManufacturersBlockItemsToDisplay, storeId: _storeContext.CurrentStore.Id);
                var model = new ManufacturerNavigationModel
                {
                    TotalManufacturers = manufacturers.TotalCount
                };

                foreach (var manufacturer in manufacturers)
                {
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.GetLocalized(x => x.Name, languageId),
                        Icon = manufacturer.Icon,
                        SeName = manufacturer.GetSeName(languageId),
                        IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
                    };
                    model.Manufacturers.Add(modelMan);
                }
                return model;
            });
            return cacheModel;
        }
        public virtual List<ManufacturerModel> PrepareManufacturerFeaturedProducts()
        {
            string manufCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_FEATURED_PRODUCT_HOMEPAGE_KEY,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured());

            var model = _cacheManager.Get(manufCacheKey, () =>
                _manufacturerService.GetAllManufacturerFeaturedProductsOnHomePage()
                .Select(x =>
                {
                    var catModel = x.ToModel();
                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    catModel.PictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                    };
                    return catModel;
                })
                .ToList()
            );

            foreach (var item in model)
            {
                //We cache a value indicating whether we have featured products
                IPagedList<Product> featuredProducts = null;

                string cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY,
                    item.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       manufacturerId: item.Id,
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    hasFeaturedProductsCache = featuredProducts.Any();
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the manufacturer has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       manufacturerId: item.Id,
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                }
                if (featuredProducts != null)
                {
                    item.FeaturedProducts = _productViewModelService.PrepareProductOverviewModels(featuredProducts).ToList();
                }
            }
            return model;
        }


        public virtual ManufacturerModel PrepareManufacturer(Manufacturer manufacturer, CatalogPagingFilteringModel command)
        {
            var model = manufacturer.ToModel();
            var currency = _workContext.WorkingCurrency;

            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                manufacturer.AllowCustomersToSelectPageSize,
                manufacturer.PageSizeOptions,
                manufacturer.PageSize);


            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(manufacturer.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, manufacturer.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, currency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, currency);
            }

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                IPagedList<Product> featuredProducts = null;

                //We cache a value indicating whether we have featured products
                string cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY,
                    manufacturer.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id);
                var hasFeaturedProductsCache = _cacheManager.Get<bool?>(cacheKey);
                if (!hasFeaturedProductsCache.HasValue)
                {
                    //no value in the cache yet
                    //let's load products and cache the result (true/false)
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       manufacturerId: manufacturer.Id,
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                    hasFeaturedProductsCache = featuredProducts.Any();
                    _cacheManager.Set(cacheKey, hasFeaturedProductsCache, 60);
                }
                if (hasFeaturedProductsCache.Value && featuredProducts == null)
                {
                    //cache indicates that the manufacturer has featured products
                    //let's load them
                    featuredProducts = _productService.SearchProducts(
                       pageSize: _catalogSettings.LimitOfFeaturedProducts,
                       manufacturerId: manufacturer.Id,
                       storeId: _storeContext.CurrentStore.Id,
                       visibleIndividuallyOnly: true,
                       featuredProducts: true);
                }
                if (featuredProducts != null)
                {
                    model.FeaturedProducts = _productViewModelService.PrepareProductOverviewModels(featuredProducts).ToList();
                }
            }
            //products
            IList<string> filterableSpecificationAttributeOptionIds;
            var products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds, false,
                manufacturerId: manufacturer.Id,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                priceMin: minPriceConverted,
                priceMax: maxPriceConverted,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = _productViewModelService.PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }
        #endregion

        #region Vendors

        public virtual VendorModel PrepareVendor(Vendor vendor, CatalogPagingFilteringModel command)
        {
            var languageId = _workContext.WorkingLanguage.Id;

            var model = new VendorModel
            {
                Id = vendor.Id,
                Name = vendor.GetLocalized(x => x.Name, languageId),
                Description = vendor.GetLocalized(x => x.Description, languageId),
                MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords, languageId),
                MetaDescription = vendor.GetLocalized(x => x.MetaDescription, languageId),
                MetaTitle = vendor.GetLocalized(x => x.MetaTitle, languageId),
                SeName = vendor.GetSeName(languageId),
                AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors
            };

            _addressViewModelService.PrepareVendorAddressModel(model: model.Address,
            address: vendor.Address,
            excludeProperties: false,
            vendorSettings: _vendorSettings);


            //prepare picture model
            int pictureSize = _mediaSettings.VendorThumbPictureSize;
            var pictureCacheKey = string.Format(ModelCacheEventConsumer.VENDOR_PICTURE_MODEL_KEY, vendor.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            model.PictureModel = _cacheManager.Get(pictureCacheKey, () =>
            {
                var picture = _pictureService.GetPictureById(vendor.PictureId);
                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                    ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), model.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), model.Name)
                };
                return pictureModel;
            });

            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                vendor.AllowCustomersToSelectPageSize,
                vendor.PageSizeOptions,
                vendor.PageSize);

            //products
            IList<string> filterableSpecificationAttributeOptionIds;
            var products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds, true,
                vendorId: vendor.Id,
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = _productViewModelService.PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);
            return model;
        }
        public virtual List<VendorModel> PrepareVendorAll()
        {
            var model = new List<VendorModel>();
            var languageId = _workContext.WorkingLanguage.Id;

            var vendors = _vendorService.GetAllVendors();
            foreach (var vendor in vendors)
            {
                var vendorModel = new VendorModel
                {
                    Id = vendor.Id,
                    Name = vendor.GetLocalized(x => x.Name, languageId),
                    Description = vendor.GetLocalized(x => x.Description, languageId),
                    MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords, languageId),
                    MetaDescription = vendor.GetLocalized(x => x.MetaDescription, languageId),
                    MetaTitle = vendor.GetLocalized(x => x.MetaTitle, languageId),
                    SeName = vendor.GetSeName(languageId),
                    AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors
                };

                //prepare vendor address
                _addressViewModelService.PrepareVendorAddressModel(model: vendorModel.Address,
                address: vendor.Address,
                excludeProperties: false,
                vendorSettings: _vendorSettings);

                //prepare picture model
                int pictureSize = _mediaSettings.VendorThumbPictureSize;
                var pictureCacheKey = string.Format(ModelCacheEventConsumer.VENDOR_PICTURE_MODEL_KEY, vendor.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                vendorModel.PictureModel = _cacheManager.Get(pictureCacheKey, () =>
                {
                    var picture = _pictureService.GetPictureById(vendor.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), vendorModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), vendorModel.Name)
                    };
                    return pictureModel;
                });
                model.Add(vendorModel);
            }

            return model;
        }
        public virtual VendorNavigationModel PrepareVendorNavigation()
        {
            string cacheKey = ModelCacheEventConsumer.VENDOR_NAVIGATION_MODEL_KEY;
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var vendors = _vendorService.GetAllVendors(pageSize: _vendorSettings.VendorsBlockItemsToDisplay);
                var model = new VendorNavigationModel
                {
                    TotalVendors = vendors.TotalCount
                };

                foreach (var vendor in vendors)
                {
                    model.Vendors.Add(new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = vendor.GetLocalized(x => x.Name),
                        SeName = vendor.GetSeName(),
                    });
                }
                return model;
            });
            return cacheModel;
        }

        #endregion

        #region Tags
        public virtual ProductsByTagModel PrepareProductsByTag(ProductTag productTag, CatalogPagingFilteringModel command)
        {
            var languageId = _workContext.WorkingLanguage.Id;

            var model = new ProductsByTagModel
            {
                Id = productTag.Id,
                TagName = productTag.GetLocalized(y => y.Name, languageId),
                TagSeName = productTag.GetSeName(languageId)
            };

            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                _catalogSettings.ProductsByTagAllowCustomersToSelectPageSize,
                _catalogSettings.ProductsByTagPageSizeOptions,
                _catalogSettings.ProductsByTagPageSize);


            //products
            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                productTagId: productTag.Id,
                visibleIndividuallyOnly: true,
                orderBy: (ProductSortingEnum)command.OrderBy,
                pageIndex: command.PageNumber - 1,
                pageSize: command.PageSize);
            model.Products = _productViewModelService.PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }
        public virtual PopularProductTagsModel PreparePopularProductTags()
        {
            var languageId = _workContext.WorkingLanguage.Id;
            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_POPULAR_MODEL_KEY, languageId, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new PopularProductTagsModel();

                //get all tags
                var allTags = _productTagService
                    .GetAllProductTags()
                    .OrderByDescending(x => _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id))
                    .ToList();

                var tags = allTags
                    .Take(_catalogSettings.NumberOfProductTags)
                    .ToList();
                //sorting
                tags = tags.OrderBy(x => x.GetLocalized(y => y.Name)).ToList();

                model.TotalTags = allTags.Count;

                foreach (var tag in tags)
                    model.Tags.Add(new ProductTagModel
                    {
                        Id = tag.Id,
                        Name = tag.GetLocalized(y => y.Name, languageId),
                        SeName = tag.GetSeName(languageId),
                        ProductCount = _productTagService.GetProductCount(tag.Id, _storeContext.CurrentStore.Id)
                    });
                return model;
            });
            return cacheModel;
        }
        public virtual PopularProductTagsModel PrepareProductTagsAll()
        {
            var languageId = _workContext.WorkingLanguage.Id;
            var model = new PopularProductTagsModel();
            model.Tags = _productTagService
                .GetAllProductTags()
                .OrderBy(x => x.GetLocalized(y => y.Name))
                .Select(x =>
                {
                    var ptModel = new ProductTagModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name, languageId),
                        SeName = x.GetSeName(languageId),
                        ProductCount = _productTagService.GetProductCount(x.Id, _storeContext.CurrentStore.Id)
                    };
                    return ptModel;
                })
                .ToList();
            return model;
        }

        #endregion

        #region Search

        public virtual IList<SearchAutoCompleteModel> PrepareSearchAutoComplete(string term, string categoryId)
        {
            var model = new List<SearchAutoCompleteModel>();
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(categoryId))
            {
                categoryIds.Add(categoryId);
                if (_catalogSettings.ShowProductsFromSubcategoriesInSearchBox)
                {
                    //include subcategories
                    categoryIds.AddRange(GetChildCategoryIds(categoryId));
                }
            }

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                categoryIds: categoryIds,
                searchSku: _catalogSettings.SearchBySku,
                searchDescriptions: _catalogSettings.SearchByDescription,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var categories = new List<string>();
            var manufacturers = new List<string>();

            var storeurl = _webHelper.GetStoreLocation();

            foreach (var item in products)
            {
                var pictureUrl = "";
                if (_catalogSettings.ShowProductImagesInSearchAutoComplete)
                {
                    var picture = item.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                    if (picture != null)
                        pictureUrl = _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.AutoCompleteSearchThumbPictureSize);
                }
                model.Add(new SearchAutoCompleteModel()
                {
                    SearchType = "Product",
                    Label = item.GetLocalized(x => x.Name) ?? "",
                    Desc = item.GetLocalized(x => x.ShortDescription) ?? "",
                    PictureUrl = pictureUrl,
                    Url = $"{storeurl}{item.SeName}"
                });
                foreach (var category in item.ProductCategories)
                {
                    categories.Add(category.CategoryId);
                }
                foreach (var manufacturer in item.ProductManufacturers)
                {
                    manufacturers.Add(manufacturer.ManufacturerId);
                }
            }

            foreach (var item in manufacturers.Distinct())
            {
                var manufacturer = _manufacturerService.GetManufacturerById(item);
                if (manufacturer != null && manufacturer.Published)
                {
                    var allow = true;
                    if (!_catalogSettings.IgnoreAcl)
                        if (!_aclService.Authorize(manufacturer))
                            allow = false;
                    if (!_catalogSettings.IgnoreStoreLimitations)
                        if (!_storeMappingService.Authorize(manufacturer))
                            allow = false;
                    if (allow)
                    {
                        var desc = "";
                        if (_catalogSettings.SearchByDescription)
                            desc = "&sid=true";
                        model.Add(new SearchAutoCompleteModel()
                        {
                            SearchType = "Manufacturer",
                            Label = manufacturer.GetLocalized(x => x.Name),
                            Desc = "",
                            PictureUrl = "",
                            Url = $"{storeurl}search?q={term}&adv=true&mid={item}{desc}"
                        });
                    }
                }
            }
            foreach (var item in categories.Distinct())
            {
                var category = _categoryService.GetCategoryById(item);
                if (category != null && category.Published)
                {
                    var allow = true;
                    if (!_catalogSettings.IgnoreAcl)
                        if (!_aclService.Authorize(category))
                            allow = false;
                    if (!_catalogSettings.IgnoreStoreLimitations)
                        if (!_storeMappingService.Authorize(category))
                            allow = false;
                    if (allow)
                    {
                        var desc = "";
                        if (_catalogSettings.SearchByDescription)
                            desc = "&sid=true";
                        model.Add(new SearchAutoCompleteModel()
                        {
                            SearchType = "Category",
                            Label = category.GetLocalized(x => x.Name),
                            Desc = "",
                            PictureUrl = "",
                            Url = $"{storeurl}search?q={term}&adv=true&cid={item}{desc}"
                        });
                    }
                }
            }

            if (_catalogSettings.ShowBlogPostsInSearchAutoComplete)
            {
                var posts = _blogService.GetAllBlogPosts(storeId: _storeContext.CurrentStore.Id, pageSize: productNumber, blogPostName: term);
                foreach (var item in posts)
                {
                    model.Add(new SearchAutoCompleteModel()
                    {
                        SearchType = "Blog",
                        Label = item.GetLocalized(x => x.Title),
                        Desc = "",
                        PictureUrl = "",
                        Url = $"{storeurl}{item.SeName}"
                    });
                }
            }

            return model;
        }

        public virtual SearchBoxModel PrepareSearchBox()
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_ALL_SEARCHBOX,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);

            return _cacheManager.Get(cacheKey, () =>
            {
                var searchbocategories = _categoryService.GetAllCategoriesSearchBox();
                searchbocategories = searchbocategories
                    .Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
                    .ToList();

                var availableCategories = new List<SelectListItem>();
                if (searchbocategories.Any())
                {
                    availableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Common.All"), Value = "" });
                    foreach (var s in searchbocategories)
                        availableCategories.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
                }

                var model = new SearchBoxModel
                {
                    AutoCompleteEnabled = _catalogSettings.ProductSearchAutoCompleteEnabled,
                    ShowProductImagesInSearchAutoComplete = _catalogSettings.ShowProductImagesInSearchAutoComplete,
                    SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength,
                    AvailableCategories = availableCategories
                };

                return model;
            });


        }

        public virtual SearchModel PrepareSearch(SearchModel model, CatalogPagingFilteringModel command)
        {
            if (model == null)
                model = new SearchModel();

            var searchTerms = model.q;
            if (searchTerms == null)
                searchTerms = "";
            searchTerms = searchTerms.Trim();

            //sorting
            PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            PreparePageSizeOptions(model.PagingFilteringContext, command,
                _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                _catalogSettings.SearchPagePageSizeOptions,
                _catalogSettings.SearchPageProductsPerPage);


            string cacheKey = string.Format(ModelCacheEventConsumer.SEARCH_CATEGORIES_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var categories = _cacheManager.Get(cacheKey, () =>
            {
                var categoriesModel = new List<SearchModel.CategoryModel>();
                //all categories
                var allCategories = _categoryService.GetAllCategories(storeId: _storeContext.CurrentStore.Id);
                foreach (var c in allCategories)
                {
                    //generate full category name (breadcrumb)
                    string categoryBreadcrumb = "";
                    var breadcrumb = c.GetCategoryBreadCrumb(allCategories, _aclService, _storeMappingService);
                    for (int i = 0; i <= breadcrumb.Count - 1; i++)
                    {
                        categoryBreadcrumb += breadcrumb[i].GetLocalized(x => x.Name);
                        if (i != breadcrumb.Count - 1)
                            categoryBreadcrumb += " >> ";
                    }
                    categoriesModel.Add(new SearchModel.CategoryModel
                    {
                        Id = c.Id,
                        Breadcrumb = categoryBreadcrumb
                    });
                }
                return categoriesModel;
            });
            if (categories.Any())
            {
                //first empty entry
                model.AvailableCategories.Add(new SelectListItem
                {
                    Value = "",
                    Text = _localizationService.GetResource("Common.All")
                });
                //all other categories
                foreach (var c in categories)
                {
                    model.AvailableCategories.Add(new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Breadcrumb,
                        Selected = model.cid == c.Id
                    });
                }
            }

            var manufacturers = _manufacturerService.GetAllManufacturers();
            if (manufacturers.Any())
            {
                model.AvailableManufacturers.Add(new SelectListItem
                {
                    Value = "",
                    Text = _localizationService.GetResource("Common.All")
                });
                foreach (var m in manufacturers)
                    model.AvailableManufacturers.Add(new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.GetLocalized(x => x.Name),
                        Selected = model.mid == m.Id
                    });
            }

            model.asv = _vendorSettings.AllowSearchByVendor;
            if (model.asv)
            {
                var vendors = _vendorService.GetAllVendors();
                if (vendors.Any())
                {
                    model.AvailableVendors.Add(new SelectListItem
                    {
                        Value = "",
                        Text = _localizationService.GetResource("Common.All")
                    });
                    foreach (var vendor in vendors)
                        model.AvailableVendors.Add(new SelectListItem
                        {
                            Value = vendor.Id.ToString(),
                            Text = vendor.GetLocalized(x => x.Name),
                            Selected = model.vid == vendor.Id
                        });
                }
            }

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            var isSearchTermSpecified = false;
            try
            {
                isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("q");
            }
            catch
            {
                isSearchTermSpecified = !String.IsNullOrEmpty(searchTerms);
            }
            if (isSearchTermSpecified)
            {
                if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
                {
                    model.Warning = string.Format(_localizationService.GetResource("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    var categoryIds = new List<string>();
                    string manufacturerId = "";
                    decimal? minPriceConverted = null;
                    decimal? maxPriceConverted = null;
                    bool searchInDescriptions = false;
                    string vendorId = "";
                    if (model.adv)
                    {
                        //advanced search
                        var categoryId = model.cid;
                        if (!String.IsNullOrEmpty(categoryId))
                        {
                            categoryIds.Add(categoryId);
                            if (model.isc)
                            {
                                //include subcategories
                                categoryIds.AddRange(GetChildCategoryIds(categoryId));
                            }
                        }


                        manufacturerId = model.mid;

                        //min price
                        if (!string.IsNullOrEmpty(model.pf))
                        {
                            decimal minPrice;
                            if (decimal.TryParse(model.pf, out minPrice))
                                minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(minPrice, _workContext.WorkingCurrency);
                        }
                        //max price
                        if (!string.IsNullOrEmpty(model.pt))
                        {
                            decimal maxPrice;
                            if (decimal.TryParse(model.pt, out maxPrice))
                                maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(maxPrice, _workContext.WorkingCurrency);
                        }

                        searchInDescriptions = model.sid;
                        if (model.asv)
                            vendorId = model.vid;
                    }

                    var searchInProductTags = searchInDescriptions;

                    //products
                    products = _productService.SearchProducts(
                        categoryIds: categoryIds,
                        manufacturerId: manufacturerId,
                        storeId: _storeContext.CurrentStore.Id,
                        visibleIndividuallyOnly: true,
                        priceMin: minPriceConverted,
                        priceMax: maxPriceConverted,
                        keywords: searchTerms,
                        searchDescriptions: searchInDescriptions,
                        searchSku: searchInDescriptions,
                        searchProductTags: searchInProductTags,
                        languageId: _workContext.WorkingLanguage.Id,
                        orderBy: (ProductSortingEnum)command.OrderBy,
                        pageIndex: command.PageNumber - 1,
                        pageSize: command.PageSize,
                        vendorId: vendorId);
                    model.Products = _productViewModelService.PrepareProductOverviewModels(products).ToList();

                    model.NoResults = !model.Products.Any();

                    //search term statistics
                    if (!String.IsNullOrEmpty(searchTerms))
                    {
                        var searchTerm = _searchTermService.GetSearchTermByKeyword(searchTerms, _storeContext.CurrentStore.Id);
                        if (searchTerm != null)
                        {
                            searchTerm.Count++;
                            _searchTermService.UpdateSearchTerm(searchTerm);
                        }
                        else
                        {
                            searchTerm = new SearchTerm
                            {
                                Keyword = searchTerms,
                                StoreId = _storeContext.CurrentStore.Id,
                                Count = 1
                            };
                            _searchTermService.InsertSearchTerm(searchTerm);
                        }
                    }

                    //event
                    _eventPublisher.Publish(new ProductSearchEvent
                    {
                        SearchTerm = searchTerms,
                        SearchInDescriptions = searchInDescriptions,
                        CategoryIds = categoryIds,
                        ManufacturerId = manufacturerId,
                        WorkingLanguageId = _workContext.WorkingLanguage.Id,
                        VendorId = vendorId
                    });
                }
            }

            model.PagingFilteringContext.LoadPagedList(products);

            return model;
        }
        #endregion

    }
}