using Grand.Core.Domain.Discounts;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount.Models;
using Grand.Services.Configuration;
using Grand.Services.Discounts;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class HadSpentAmountController : BasePluginController
    {
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        public HadSpentAmountController(IDiscountService discountService,
            ISettingService settingService, 
            IPermissionService permissionService)
        {
            this._discountService = discountService;
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

            if (!String.IsNullOrEmpty(discountRequirementId))
            {
                var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId);
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var spentAmountRequirement = _settingService.GetSettingByKey<decimal>(string.Format("DiscountRequirements.Standard.HadSpentAmount-{0}-{1}", discount.Id, !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : ""));

            var model = new RequirementModel();
            model.RequirementId = !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : "";
            model.DiscountId = discountId;
            model.SpentAmount = spentAmountRequirement;

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRequirementsHadSpentAmount{0}-{1}",discount.Id,  !String.IsNullOrEmpty(discountRequirementId) ? discountRequirementId : "");

            return View("~/Plugins/DiscountRequirements.Standard/Views/HadSpentAmount/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(string discountId, string discountRequirementId, decimal spentAmount)
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
                _settingService.SetSetting(string.Format("DiscountRequirements.Standard.HadSpentAmount-{0}-{1}", discount.Id, discountRequirement.Id), spentAmount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirements.Standard.HadSpentAmount"
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
                
                _settingService.SetSetting(string.Format("DiscountRequirements.Standard.HadSpentAmount-{0}-{1}", discount.Id, discountRequirement.Id), spentAmount);
            }
            return new JsonResult(new { Result = true, NewRequirementId = discountRequirement.Id });
        }
        
    }
}