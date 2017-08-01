using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Framework.Mvc;

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