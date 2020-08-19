using Grand.Core;
using Grand.Domain.Stores;
using Grand.Core.Plugins;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
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
using System.Threading.Tasks;

namespace Grand.Plugin.Feed.GoogleShopping.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    [PermissionAuthorize(PermissionSystemName.Plugins)]
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
        private readonly IServiceProvider _serviceProvider;

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
            IPermissionService permissionService,
            IServiceProvider serviceProvider)
        {
            _googleService = googleService;
            _productService = productService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _pluginFinder = pluginFinder;
            _logger = logger;
            _webHelper = webHelper;
            _storeService = storeService;
            _GoogleShoppingSettings = GoogleShoppingSettings;
            _settingService = settingService;
            _permissionService = permissionService;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Configure()
        {
            var model = new FeedGoogleShoppingModel();
            model.ProductPictureSize = _GoogleShoppingSettings.ProductPictureSize;
            model.PassShippingInfoWeight = _GoogleShoppingSettings.PassShippingInfoWeight;
            model.PassShippingInfoDimensions = _GoogleShoppingSettings.PassShippingInfoDimensions;
            model.PricesConsiderPromotions = _GoogleShoppingSettings.PricesConsiderPromotions;
            //stores
            model.StoreId = _GoogleShoppingSettings.StoreId;
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
            //currencies
            model.CurrencyId = _GoogleShoppingSettings.CurrencyId;
            foreach (var c in await _currencyService.GetAllCurrencies())
                model.AvailableCurrencies.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //Google categories
            model.DefaultGoogleCategory = _GoogleShoppingSettings.DefaultGoogleCategory;
            model.AvailableGoogleCategories.Add(new SelectListItem { Text = "Select a category", Value = "" });
            foreach (var gc in await _googleService.GetTaxonomyList())
                model.AvailableGoogleCategories.Add(new SelectListItem { Text = gc, Value = gc });

            //file paths
            foreach (var store in await _storeService.GetAllStores())
            {
                var appPath = CommonHelper.WebMapPath("content/files/exportimport");
                var localFilePath = System.IO.Path.Combine(appPath, store.Id + "-" + _GoogleShoppingSettings.StaticFileName);
                if (System.IO.File.Exists(localFilePath))
                    model.GeneratedFiles.Add(new FeedGoogleShoppingModel.GeneratedFileModel
                    {
                        StoreName = store.Shortcut,
                        FileUrl = string.Format("{0}content/files/exportimport/{1}-{2}", _webHelper.GetStoreLocation(false), store.Id, _GoogleShoppingSettings.StaticFileName)
                    });
            }

            return View("~/Plugins/Feed.GoogleShopping/Views/FeedGoogleShopping/Configure.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(FeedGoogleShoppingModel model)
        {
            if (!ModelState.IsValid)
            {
                return await Configure();
            }

            //save settings
            _GoogleShoppingSettings.ProductPictureSize = model.ProductPictureSize;
            _GoogleShoppingSettings.PassShippingInfoWeight = model.PassShippingInfoWeight;
            _GoogleShoppingSettings.PassShippingInfoDimensions = model.PassShippingInfoDimensions;
            _GoogleShoppingSettings.PricesConsiderPromotions = model.PricesConsiderPromotions;
            _GoogleShoppingSettings.CurrencyId = model.CurrencyId;
            _GoogleShoppingSettings.StoreId = model.StoreId;
            _GoogleShoppingSettings.DefaultGoogleCategory = model.DefaultGoogleCategory;
            await _settingService.SaveSetting(_GoogleShoppingSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            //redisplay the form
            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("generate")]
        public async Task<IActionResult> GenerateFeed(FeedGoogleShoppingModel model)
        {
            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("PromotionFeed.GoogleShopping");
                if (pluginDescriptor == null)
                    throw new Exception("Cannot load the plugin");

                //plugin
                var plugin = pluginDescriptor.Instance(_serviceProvider) as GoogleShoppingService;
                if (plugin == null)
                    throw new Exception("Cannot load the plugin");

                var stores = new List<Store>();
                var storeById = await _storeService.GetStoreById(_GoogleShoppingSettings.StoreId);
                if (storeById != null)
                    stores.Add(storeById);
                else
                    stores.AddRange(await _storeService.GetAllStores());

                foreach (var store in stores)
                    await plugin.GenerateStaticFile(store);

                SuccessNotification(_localizationService.GetResource("Plugins.Feed.GoogleShopping.SuccessResult"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                _logger.Error(exc.Message, exc);
            }

            return await Configure();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GoogleProductList(DataSourceRequest command)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var products = (await _productService.SearchProducts(pageIndex: command.Page - 1,
                pageSize: command.PageSize, showHidden: true)).products;
            var productsModel = new List<FeedGoogleShoppingModel.GoogleProductModel>();
            foreach (var x in products)
            {

                var gModel = new FeedGoogleShoppingModel.GoogleProductModel
                {
                    ProductId = x.Id,
                    ProductName = x.Name

                };
                var googleProduct = await _googleService.GetByProductId(x.Id);
                if (googleProduct != null)
                {
                    gModel.GoogleCategory = googleProduct.Taxonomy;
                    gModel.Gender = googleProduct.Gender;
                    gModel.AgeGroup = googleProduct.AgeGroup;
                    gModel.Color = googleProduct.Color;
                    gModel.GoogleSize = googleProduct.Size;
                    gModel.CustomGoods = googleProduct.CustomGoods;
                }

                productsModel.Add(gModel);
            }
            var gridModel = new DataSourceResult
            {
                Data = productsModel,
                Total = products.TotalCount
            };

            return new JsonResult(gridModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GoogleProductUpdate(FeedGoogleShoppingModel.GoogleProductModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var googleProduct = await _googleService.GetByProductId(model.ProductId);
            if (googleProduct != null)
            {

                googleProduct.Taxonomy = model.GoogleCategory;
                googleProduct.Gender = model.Gender;
                googleProduct.AgeGroup = model.AgeGroup;
                googleProduct.Color = model.Color;
                googleProduct.Size = model.GoogleSize;
                googleProduct.CustomGoods = model.CustomGoods;
                await _googleService.UpdateGoogleProductRecord(googleProduct);
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
                await _googleService.InsertGoogleProductRecord(googleProduct);
            }

            return new NullJsonResult();
        }
    }
}
