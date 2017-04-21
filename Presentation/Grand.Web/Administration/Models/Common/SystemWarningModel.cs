using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Common
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