using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public partial class StoreSelectorModel : BaseGrandModel
    {
        public StoreSelectorModel()
        {
            AvailableStores = new List<StoreModel>();
        }

        public IList<StoreModel> AvailableStores { get; set; }

        public string CurrentStoreId { get; set; }

    }
}