using Grand.Core.Domain.Tax;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Common
{
    public partial class TaxTypeSelectorModel : BaseGrandModel
    {
        public TaxDisplayType CurrentTaxType { get; set; }
    }
}