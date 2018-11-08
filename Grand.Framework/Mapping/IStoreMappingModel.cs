using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Framework.Mapping
{
    public interface IStoreMappingModel
    {
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.LimitedToStores")]
        bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.AvailableStores")]
        List<StoreModel> AvailableStores { get; set; }
        string[] SelectedStoreIds { get; set; }
    }
}
