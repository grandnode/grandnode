﻿using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Cms
{
    public partial class RenderWidgetModel : BaseModel
    {
        public string WidgetViewComponentName { get; set; }
        public object WidgetViewComponentArguments { get; set; }
    }
}