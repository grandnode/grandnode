using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Plugins
{
    public partial class OfficialFeedListModel : BaseGrandModel
    {
        public OfficialFeedListModel()
        {
            AvailableVersions = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailablePrices = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.OfficialFeed.Name")]
        
        public string SearchName { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.OfficialFeed.Version")]
        public int SearchVersionId { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.OfficialFeed.Category")]
        public string SearchCategoryId { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.OfficialFeed.Price")]
        public int SearchPriceId { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.Plugins.OfficialFeed.Version")]
        public IList<SelectListItem> AvailableVersions { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.OfficialFeed.Category")]
        public IList<SelectListItem> AvailableCategories { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.OfficialFeed.Price")]
        public IList<SelectListItem> AvailablePrices { get; set; }

        #region Nested classes

        public partial class ItemOverview
        {
            public string Url { get; set; }
            public string Name { get; set; }
            public string CategoryName { get; set; }
            public string SupportedVersions { get; set; }
            public string PictureUrl { get; set; }
            public string Price { get; set; }
        }

        #endregion
    }
}