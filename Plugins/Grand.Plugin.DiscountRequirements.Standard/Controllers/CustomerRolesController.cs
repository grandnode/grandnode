using Grand.Core.Domain.Discounts;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Plugin.DiscountRequirements.CustomerRoles.Models;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Discounts;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Grand.Plugin.DiscountRequirements.CustomerRoles.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class CustomerRolesController : BasePluginController
    {
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        public CustomerRolesController(IDiscountService discountService,
            ICustomerService customerService, ISettingService settingService,
            IPermissionService permissionService)
        {
            this._discountService = discountService;
            this._customerService = customerService;
            this._settingService = settingService;
            this._permissionService = permissionService;
        }

        public IActionResult Configure(string discountId, string discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (!String.IsNullOrEmpty(discountRequirementId))
            {
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var restrictedToCustomerRoleId = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirements.Standard.MustBeAssignedToCustomerRole-{0}-{1}", discount.Id, !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : ""));
            
            var model = new RequirementModel();
            model.RequirementId = !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : "";
            model.DiscountId = discountId;
            model.CustomerRoleId = restrictedToCustomerRoleId;
            //customer roles
            //TODO localize "Select customer role"
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = "Select customer role", Value = "" });
            foreach (var cr in _customerService.GetAllCustomerRoles(true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = cr.Name, Value = cr.Id.ToString(), Selected = discountRequirement != null && cr.Id == restrictedToCustomerRoleId });

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRequirementsCustomerRoles{0}", !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : "");

            return View("~/Plugins/DiscountRequirements.Standard/Views/CustomerRoles/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(string discountId, string discountRequirementId, string customerRoleId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (!String.IsNullOrEmpty(discountRequirementId))
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);

            if (discountRequirement != null)
            {
                //update existing rule
                _settingService.SetSetting(string.Format("DiscountRequirements.Standard.MustBeAssignedToCustomerRole-{0}-{1}",discount.Id, discountRequirement.Id), customerRoleId);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirements.Standard.MustBeAssignedToCustomerRole"
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);

                _settingService.SetSetting(string.Format("DiscountRequirements.Standard.MustBeAssignedToCustomerRole-{0}-{1}", discount.Id, discountRequirement.Id), customerRoleId);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }
    }
}