using Grand.Core;
using Grand.Domain.Common;
using Grand.Framework.Components;
using Grand.Services.Stores;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class StoreSelectorViewComponent : BaseViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        private readonly CommonSettings _commonSettings;

        public StoreSelectorViewComponent(
            IStoreContext storeContext,
            IStoreService storeService,
            CommonSettings commonSettings)
        {
            _storeContext = storeContext;
            _storeService = storeService;
            _commonSettings = commonSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareStoreSelector();
            if (model == null || model.AvailableStores.Count == 1)
                Content("");

            return View(model);
        }
        public async Task<StoreSelectorModel> PrepareStoreSelector()
        {
            if (!_commonSettings.AllowToSelectStore)
                return null;

            var availableStores = (await _storeService.GetAllStores())
                .Select(x => new StoreModel {
                    Id = x.Id,
                    Name = x.Shortcut,
                }).ToList();

            var model = new StoreSelectorModel {
                CurrentStoreId = _storeContext.CurrentStore.Id,
                AvailableStores = availableStores,
            };

            return model;
        }

    }
}