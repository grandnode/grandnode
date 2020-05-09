using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public partial class FooterCleanModel : BaseGrandModel
    {
        public string StoreName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyHours { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public bool HidePoweredByGrandNode { get; set; }
    }
}
