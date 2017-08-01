using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Logging
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
        
        public string Message { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.List.LogLevel")]
        public int LogLevelId { get; set; }


        public IList<SelectListItem> AvailableLogLevels { get; set; }
    }
}