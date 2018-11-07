using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Framework.Mapping
{
    public interface IStoreMappingModel
    {
        List<StoreModel> AvailableStores { get; set; }
        string[] SelectedStoreIds { get; set; }
    }
}
