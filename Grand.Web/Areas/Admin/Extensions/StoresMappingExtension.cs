using Grand.Core.Domain.Stores;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.Models;
using Grand.Services.Stores;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class StoresMappingExtension
    {
        public static async Task PrepareStoresMappingModel<T>(this T baseGrandEntityModel, IStoreMappingSupported storeMapping, bool excludeProperties, IStoreService _storeService)
            where T : BaseGrandEntityModel, IStoreMappingModel
        {
            baseGrandEntityModel.AvailableStores = (await _storeService
               .GetAllStores())
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
