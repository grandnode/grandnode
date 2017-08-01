using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public partial class LogoModel : BaseGrandModel
    {
        public string StoreName { get; set; }

        public string LogoPath { get; set; }
    }
}