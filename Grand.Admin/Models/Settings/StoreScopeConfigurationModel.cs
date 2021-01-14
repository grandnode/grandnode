using Grand.Core.Models;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Settings
{
    public partial class StoreScopeConfigurationModel : BaseModel
    {
        public StoreScopeConfigurationModel()
        {
            Stores = new List<StoreModel>();
        }

        public string StoreId { get; set; }
        public IList<StoreModel> Stores { get; set; }
    }
}