using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Vendors;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Framework.Security;
using Grand.Web.Models.Catalog;
using Grand.Web.Services;
using Grand.Framework.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Grand.Web.Controllers
{
    public partial class CatalogController : BasePublicController
    {
        #region Fields

        private readonly ICatalogWebService _catalogWebService;
        private readonly ICategoryService _categoryService;
        private readonly IProductWebService _productWebService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IProductTagService _productTagService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerActionEventService _customerActionEventService;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly VendorSettings _vendorSettings;
        
        #endregion

		#region Constructors

        public CatalogController(ICatalogWebService catalogWebService,
            ICategoryService categoryService,
            IProductWebService productWebService,
            IManufacturerService manufacturerService,
            IProductService productService, 
            IVendorService vendorService,
            IWorkContext workContext, 
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper, 
            IProductTagService productTagService,
            IGenericAttributeService genericAttributeService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService, 
            ICustomerActivityService customerActivityService,
            ICustomerActionEventService customerActionEventService,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            VendorSettings vendorSettings,
            ICacheManager cacheManager)
        {
            this._catalogWebService = catalogWebService;
            this._categoryService = categoryService;
            this._productWebService = productWebService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._vendorService = vendorService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._productTagService = productTagService;
            this._genericAttributeService = genericAttributeService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._customerActionEventService = customerActionEventService;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._vendorSettings = vendorSettings;
        }

        #endregion

        #region Categories
        
        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Category(string categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(category))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(category))
                return InvokeHttp404();
            
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, 
                SystemCustomerAttributeNames.LastContinueShoppingPage, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = "Admin" }));

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory", category.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name);
            _customerActionEventService.Viewed(_workContext.CurrentCustomer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers["Referer"].ToString() : "");
            //model
            var model = _catalogWebService.PrepareCategory(category, command);

            //template
            var templateViewPath = _catalogWebService.PrepareCategoryTemplateViewPath(category.CategoryTemplateId);

            return View(templateViewPath, model);
        }

        #endregion

        #region Manufacturers

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Manufacturer(string manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null)
                return InvokeHttp404();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a manufacturer before publishing
            if (!manufacturer.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return InvokeHttp404();

            //ACL (access control list)
            if (!_aclService.Authorize(manufacturer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(manufacturer))
                return InvokeHttp404();
            
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, 
                SystemCustomerAttributeNames.LastContinueShoppingPage, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
            
            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                DisplayEditLink(Url.Action("Edit", "Manufacturer", new { id = manufacturer.Id, area = "Admin" }));
            
            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name);
            _customerActionEventService.Viewed(_workContext.CurrentCustomer, this.HttpContext.Request.Path.ToString(), this.Request.Headers[HeaderNames.Referer].ToString() != null ? Request.Headers[HeaderNames.Referer].ToString() : "");

            //model
            var model = _catalogWebService.PrepareManufacturer(manufacturer, command);

            //template
            var templateViewPath = _catalogWebService.PrepareManufacturerTemplateViewPath(manufacturer.ManufacturerTemplateId);

            return View(templateViewPath, model);
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ManufacturerAll()
        {
            var model = _catalogWebService.PrepareManufacturerAll();
            return View(model);
        }

        #endregion

        #region Vendors

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Vendor(string vendorId, CatalogPagingFilteringModel command)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return InvokeHttp404();

            //Vendor is active?
            if (!vendor.Active)
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
            
            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                DisplayEditLink(Url.Action("Edit", "Vendor", new { id = vendor.Id, area = "Admin" }));

            var model = _catalogWebService.PrepareVendor(vendor, command);

            return View(model);
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult VendorAll()
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return RedirectToRoute("HomePage");

            var model = _catalogWebService.PrepareVendorAll();
            return View(model);
        }

        #endregion

        #region Product tags
        
      
        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductsByTag(string productTagId, CatalogPagingFilteringModel command)
        {
            var productTag = _productTagService.GetProductTagById(productTagId);
            if (productTag == null)
                return InvokeHttp404();

            var model = _catalogWebService.PrepareProductsByTag(productTag, command);
            return View(model);
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult ProductTagsAll()
        {
            var model = _catalogWebService.PrepareProductTagsAll();
            return View(model);
        }

        #endregion

        #region Searching

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var searchmodel = _catalogWebService.PrepareSearch(model, command);

            return View(searchmodel);
        }

        public virtual IActionResult SearchTermAutoComplete(string term)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                searchSku: false,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var models =  _productWebService.PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl
                          })
                          .ToList();
            return Json(result);
        }

        #endregion
    }
}
