using System.Collections.Generic;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Common
{
    public partial class StoreThemeSelectorModel : BaseGrandModel
    {
        public StoreThemeSelectorModel()
        {
            AvailableStoreThemes = new List<StoreThemeModel>();
        }

        public IList<StoreThemeModel> AvailableStoreThemes { get; set; }

        public StoreThemeModel CurrentStoreTheme { get; set; }
    }
}