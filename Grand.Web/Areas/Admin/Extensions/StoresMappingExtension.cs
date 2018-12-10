using Grand.Core.Domain.Stores;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.Models;
using Grand.Services.Stores;
using System.Linq;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class StoresMappingExtension
    {
        public static void PrepareStoresMappingModel<T>(this T baseGrandEntityModel, IStoreMappingSupported storeMapping, bool excludeProperties, IStoreService _storeService)
            where T : BaseGrandEntityModel, IStoreMappingModel
        {
            baseGrandEntityModel.AvailableStores = _storeService
               .GetAllStores()
               .Select(s => new StoreModel { Id = s.Id, Name = s.Name })
               .ToList();
            if (!excludeProperties)
            {
                if (storeMapping != null)
                {
                    baseGrandEntityModel.SelectedStoreIds = storeMapping.Stores.ToArray();
                }
            }
        }       
    }
}
