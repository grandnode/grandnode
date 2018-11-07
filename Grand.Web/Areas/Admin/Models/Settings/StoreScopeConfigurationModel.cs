using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class StoreScopeConfigurationModel : BaseGrandModel
    {
        public StoreScopeConfigurationModel()
        {
            Stores = new List<StoreModel>();
        }

        public string StoreId { get; set; }
        public IList<StoreModel> Stores { get; set; }
    }
}