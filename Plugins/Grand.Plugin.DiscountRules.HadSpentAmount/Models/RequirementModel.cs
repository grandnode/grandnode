using Grand.Framework;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Plugin.DiscountRules.HadSpentAmount.Models
{
    public class RequirementModel
    {
        [GrandResourceDisplayName("Plugins.DiscountRules.HadSpentAmount.Fields.Amount")]
        public decimal SpentAmount { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }
    }
}