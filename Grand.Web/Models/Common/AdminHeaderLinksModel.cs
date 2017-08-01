using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public partial class AdminHeaderLinksModel : BaseGrandModel
    {
        public string ImpersonatedCustomerEmailUsername { get; set; }
        public bool IsCustomerImpersonated { get; set; }
        public bool DisplayAdminLink { get; set; }
        public string EditPageUrl { get; set; }
    }
}