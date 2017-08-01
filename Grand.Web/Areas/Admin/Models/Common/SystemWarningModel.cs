using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class SystemWarningModel : BaseGrandModel
    {
        public SystemWarningLevel Level { get; set; }

        public string Text { get; set; }
    }

    public enum SystemWarningLevel
    {
        Pass,
        Warning,
        Fail
    }
}