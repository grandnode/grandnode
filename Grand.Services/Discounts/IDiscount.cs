using Grand.Core.Plugins;
using System.Collections.Generic;

namespace Grand.Services.Discounts
{
    /// <summary>
    /// Represents a discount requirement rule
    /// </summary>
    public partial interface IDiscount : IPlugin
    {
        IList<IDiscountRequirementRule> GetRequirementRules();
    }
}
