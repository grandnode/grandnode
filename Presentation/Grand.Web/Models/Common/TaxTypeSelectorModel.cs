using Grand.Core.Domain.Tax;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Common
{
    public partial class TaxTypeSelectorModel : BaseNopModel
    {
        public TaxDisplayType CurrentTaxType { get; set; }
    }
}