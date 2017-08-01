using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public partial class CurrencyModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
    }
}