using Grand.Framework.Mvc.Models;

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