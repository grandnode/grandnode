using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;

namespace Grand.Plugin.DiscountRules.CustomerRoles.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.DiscountRules.CustomerRoles.Fields.CustomerRole")]
        public string CustomerRoleId { get; set; }

        public string DiscountId { get; set; }

        public string RequirementId { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}