using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Plugins
{
    public partial class PluginListModel : BaseGrandModel
    {
        public PluginListModel()
        {
            AvailableLoadModes = new List<SelectListItem>();
            AvailableGroups = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.LoadMode")]
        public int SearchLoadModeId { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.Group")]
        public string SearchGroup { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.LoadMode")]
        public IList<SelectListItem> AvailableLoadModes { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.Group")]
        public IList<SelectListItem> AvailableGroups { get; set; }
    }
}