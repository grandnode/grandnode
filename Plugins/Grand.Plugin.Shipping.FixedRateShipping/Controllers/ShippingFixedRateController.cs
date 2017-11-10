using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Plugin.Shipping.FixedRateShipping.Models;
using Grand.Services.Configuration;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Grand.Plugin.Shipping.FixedRateShipping.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class ShippingFixedRateController : BaseShippingController
    {
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        public ShippingFixedRateController(IShippingService shippingServicee,
            ISettingService settingService, 
            IPermissionService permissionService)
        {
            this._shippingService = shippingServicee;
            this._settingService = settingService;
            this._permissionService = permissionService;
        }
        
        public IActionResult Configure()
        {
            return View("~/Plugins/Shipping.FixedRateShipping/Views/ShippingFixedRate/Configure.cshtml");
        }

        [HttpPost]
        public IActionResult Configure(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            var rateModels = new List<FixedShippingRateModel>();
            foreach (var shippingMethod in _shippingService.GetAllShippingMethods())
                rateModels.Add(new FixedShippingRateModel
                {
                    ShippingMethodId = shippingMethod.Id,
                    ShippingMethodName = shippingMethod.Name,
                    Rate = GetShippingRate(shippingMethod.Id)
                });

            var gridModel = new DataSourceResult
            {
                Data = rateModels,
                Total = rateModels.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult ShippingRateUpdate(FixedShippingRateModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            string shippingMethodId = model.ShippingMethodId;
            decimal rate = model.Rate;

            _settingService.SetSetting(string.Format("ShippingRateComputationMethod.FixedRate.Rate.ShippingMethodId{0}", shippingMethodId), rate);

            return new NullJsonResult();
        }

        [NonAction]
        protected decimal GetShippingRate(string shippingMethodId)
        {
            var rate = this._settingService.GetSettingByKey<decimal>(string.Format("ShippingRateComputationMethod.FixedRate.Rate.ShippingMethodId{0}", shippingMethodId));
            return rate;
        }
    }
}