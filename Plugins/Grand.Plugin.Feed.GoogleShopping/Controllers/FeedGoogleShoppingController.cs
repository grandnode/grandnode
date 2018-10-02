using Grand.Core;
using Grand.Core.Domain.Stores;
using Grand.Core.Plugins;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Plugin.Feed.GoogleShopping.Domain;
using Grand.Plugin.Feed.GoogleShopping.Models;
using Grand.Plugin.Feed.GoogleShopping.Services;
using Grand.Services.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Plugin.Feed.GoogleShopping.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class FeedGoogleShoppingController : BasePluginController
    {
        private readonly IGoogleService _googleService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IStoreService _storeService;
        private readonly GoogleShoppingSettings _GoogleShoppingSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        public FeedGoogleShoppingController(IGoogleService googleService,
            IProductService productService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPluginFinder pluginFinder,
            ILogger logger,
            IWebHelper webHelper,
            IStoreService storeService,
            GoogleShoppingSettings GoogleShoppingSettings,
            ISettingService settingService,
            IPermissionService permissionService)
        {
            this._googleService = googleService;
            this._productService = productService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._pluginFinder = pluginFinder;
            this._logger = logger;
            this._webHelper = webHelper;
            this._storeService = storeService;
            this._GoogleShoppingSettings = GoogleShoppingSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
        }

        public IActionResult Configure()
        {
            var model = new FeedGoogleShoppingModel();
            model.ProductPictureSize = _GoogleShoppingSettings.ProductPictureSize;
            model.PassShippingInfoWeight = _GoogleShoppingSettings.PassShippingInfoWeight;
            model.PassShippingInfoDimensions = _GoogleShoppingSettings.PassShippingInfoDimensions;
            model.PricesConsiderPromotions = _GoogleShoppingSettings.PricesConsiderPromotions;
            //stores
            model.StoreId = _GoogleShoppingSettings.StoreId;
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            //currencies
            model.CurrencyId = _GoogleShoppingSettings.CurrencyId;
            foreach (var c in _currencyService.GetAllCurrencies())
                model.AvailableCurrencies.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //Google categories
            model.DefaultGoogleCategory = _GoogleShoppingSettings.DefaultGoogleCategory;
            model.AvailableGoogleCategories.Add(new SelectListItem {Text = "Select a category", Value = ""});
            foreach (var gc in _googleService.GetTaxonomyList())
                model.AvailableGoogleCategories.Add(new SelectListItem { Text = gc, Value = gc });

            //file paths
            foreach (var store in _storeService.GetAllStores())
            {
                var appPath = CommonHelper.MapPath("wwwroot/content/files/exportimport");
                var localFilePath = System.IO.Path.Combine(appPath, store.Id + "-" + _GoogleShoppingSettings.StaticFileName);           
                if (System.IO.File.Exists(localFilePath))
                    model.GeneratedFiles.Add(new FeedGoogleShoppingModel.GeneratedFileModel
                    {
                        StoreName = store.Name,
                        FileUrl = string.Format("{0}content/files/exportimport/{1}-{2}", _webHelper.GetStoreLocation(false), store.Id, _GoogleShoppingSettings.StaticFileName)
                    });
            }

            return View("~/Plugins/Feed.GoogleShopping/Views/FeedGoogleShopping/Configure.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult Configure(FeedGoogleShoppingModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            //save settings
            _GoogleShoppingSettings.ProductPictureSize = model.ProductPictureSize;
            _GoogleShoppingSettings.PassShippingInfoWeight = model.PassShippingInfoWeight;
            _GoogleShoppingSettings.PassShippingInfoDimensions = model.PassShippingInfoDimensions;
            _GoogleShoppingSettings.PricesConsiderPromotions = model.PricesConsiderPromotions;
            _GoogleShoppingSettings.CurrencyId = model.CurrencyId;
            _GoogleShoppingSettings.StoreId = model.StoreId;
            _GoogleShoppingSettings.DefaultGoogleCategory = model.DefaultGoogleCategory;
            _settingService.SaveSetting(_GoogleShoppingSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            //redisplay the form
            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("generate")]
        public IActionResult GenerateFeed(FeedGoogleShoppingModel model)
        {
            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("PromotionFeed.GoogleShopping");
                if (pluginDescriptor == null)
                    throw new Exception("Cannot load the plugin");

                //plugin
                var plugin = pluginDescriptor.Instance() as GoogleShoppingService;
                if (plugin == null)
                    throw new Exception("Cannot load the plugin");

                var stores = new List<Store>();
                var storeById = _storeService.GetStoreById(_GoogleShoppingSettings.StoreId);
                if (storeById != null)
                    stores.Add(storeById);
                else
                    stores.AddRange(_storeService.GetAllStores());

                foreach (var store in stores)
                    plugin.GenerateStaticFile(store);

                SuccessNotification(_localizationService.GetResource("Plugins.Feed.GoogleShopping.SuccessResult"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                _logger.Error(exc.Message, exc);
            }

            return Configure();
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GoogleProductList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var products = _productService.SearchProducts(pageIndex: command.Page - 1,
                pageSize: command.PageSize, showHidden: true);
            var productsModel = products
                .Select(x =>
                            {
                                var gModel = new FeedGoogleShoppingModel.GoogleProductModel
                                {
                                    ProductId = x.Id,
                                    ProductName = x.Name

                                };
                                var googleProduct = _googleService.GetByProductId(x.Id);
                                if (googleProduct != null)
                                {
                                    gModel.GoogleCategory = googleProduct.Taxonomy;
                                    gModel.Gender = googleProduct.Gender;
                                    gModel.AgeGroup = googleProduct.AgeGroup;
                                    gModel.Color = googleProduct.Color;
                                    gModel.GoogleSize = googleProduct.Size;
                                    gModel.CustomGoods = googleProduct.CustomGoods;
                                }

                                return gModel;
                            })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productsModel,
                Total = products.TotalCount
            };

            return new JsonResult(gridModel);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult GoogleProductUpdate(FeedGoogleShoppingModel.GoogleProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var googleProduct = _googleService.GetByProductId(model.ProductId);
            if (googleProduct != null)
            {

                googleProduct.Taxonomy = model.GoogleCategory;
                googleProduct.Gender = model.Gender;
                googleProduct.AgeGroup = model.AgeGroup;
                googleProduct.Color = model.Color;
                googleProduct.Size = model.GoogleSize;
                googleProduct.CustomGoods = model.CustomGoods;
                _googleService.UpdateGoogleProductRecord(googleProduct);
            }
            else
            {
                //insert
                googleProduct = new GoogleProductRecord
                {
                    ProductId = model.ProductId,
                    Taxonomy = model.GoogleCategory,
                    Gender = model.Gender,
                    AgeGroup = model.AgeGroup,
                    Color = model.Color,
                    Size = model.GoogleSize,
                    CustomGoods = model.CustomGoods
                };
                _googleService.InsertGoogleProductRecord(googleProduct);
            }

            return new NullJsonResult();
        }
    }
}
