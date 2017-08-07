using Grand.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Plugin.DiscountRules.CustomerRoles.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Plugins.DiscountRules.CustomerRoles.Fields.CustomerRole")]
        public string CustomerRoleId { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}