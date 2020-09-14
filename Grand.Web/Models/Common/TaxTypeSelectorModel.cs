using Grand.Domain.Tax;
using Grand.Core.Models;

namespace Grand.Web.Models.Common
{
    public partial class TaxTypeSelectorModel : BaseModel
    {
        public TaxDisplayType CurrentTaxType { get; set; }
    }
}