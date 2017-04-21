using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Logging
{
    public partial class LogListModel : BaseGrandModel
    {
        public LogListModel()
        {
            AvailableLogLevels = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.System.Log.List.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.List.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.List.Message")]
        [AllowHtml]
        public string Message { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.List.LogLevel")]
        public int LogLevelId { get; set; }


        public IList<SelectListItem> AvailableLogLevels { get; set; }
    }
}