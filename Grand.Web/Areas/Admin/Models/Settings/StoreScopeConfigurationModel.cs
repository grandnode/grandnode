using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Stores;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class StoreScopeConfigurationModel : BaseGrandModel
    {
        public StoreScopeConfigurationModel()
        {
            Stores = new List<Grand.Web.Areas.Admin.Models.Stores.StoreModel>();
        }

        public string StoreId { get; set; }
        public IList<Grand.Web.Areas.Admin.Models.Stores.StoreModel> Stores { get; set; }
    }
}