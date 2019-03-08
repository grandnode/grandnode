using Grand.Core.Domain.Stores;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Stores;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class StoreViewModelService : IStoreViewModelService
    {
        private readonly ILanguageService _languageService;
        private readonly IShippingService _shippingService;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;

        public StoreViewModelService(ILanguageService languageService, IShippingService shippingService, IStoreService storeService, ISettingService settingService)
        {
            _languageService = languageService;
            _shippingService = shippingService;
            _storeService = storeService;
            _settingService = settingService;
        }

        public virtual void PrepareLanguagesModel(StoreModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //templates
            model.AvailableLanguages.Add(new SelectListItem
            {
                Text = "---",
                Value = ""
            });
            var languages = _languageService.GetAllLanguages(true);
            foreach (var language in languages)
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Text = language.Name,
                    Value = language.Id.ToString()
                });
            }
        }

        public virtual void PrepareWarehouseModel(StoreModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //templates
            model.AvailableWarehouses.Add(new SelectListItem
            {
                Text = "---",
                Value = ""
            });
            var warehouses = _shippingService.GetAllWarehouses();
            foreach (var warehouse in warehouses)
            {
                model.AvailableWarehouses.Add(new SelectListItem
                {
                    Text = warehouse.Name,
                    Value = warehouse.Id.ToString()
                });
            }
        }
        public virtual StoreModel PrepareStoreModel()
        {
            var model = new StoreModel();
            return model;
        }
        public virtual Store InsertStoreModel(StoreModel model)
        {
            var store = model.ToEntity();
            //ensure we have "/" at the end
            if (!store.Url.EndsWith("/"))
                store.Url += "/";

            if (!string.IsNullOrEmpty(store.SecureUrl) && !store.SecureUrl.EndsWith("/"))
                store.SecureUrl += "/";

            _storeService.InsertStore(store);
            return store;
        }
        public virtual Store UpdateStoreModel(Store store, StoreModel model)
        {
            store = model.ToEntity(store);
            //ensure we have "/" at the end
            if (!store.Url.EndsWith("/"))
                store.Url += "/";
            if (!string.IsNullOrEmpty(store.SecureUrl) && !store.SecureUrl.EndsWith("/"))
                store.SecureUrl += "/";

            _storeService.UpdateStore(store);
            return store;
        }
        public virtual void DeleteStore(Store store)
        {
            _storeService.DeleteStore(store);

            //when we delete a store we should also ensure that all "per store" settings will also be deleted
            var settingsToDelete = _settingService
                .GetAllSettings()
                .Where(s => s.StoreId == store.Id)
                .ToList();
            foreach (var setting in settingsToDelete)
                _settingService.DeleteSetting(setting);
            //when we had two stores and now have only one store, we also should delete all "per store" settings
            var allStores = _storeService.GetAllStores();
            if (allStores.Count == 1)
            {
                settingsToDelete = _settingService
                    .GetAllSettings()
                    .Where(s => s.StoreId == allStores[0].Id)
                    .ToList();
                foreach (var setting in settingsToDelete)
                    _settingService.DeleteSetting(setting);
            }
        }
    }
}
