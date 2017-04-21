using Grand.Core.Domain.Tax;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Common
{
    public partial class TaxTypeSelectorModel : BaseGrandModel
    {
        public TaxDisplayType CurrentTaxType { get; set; }
    }
}