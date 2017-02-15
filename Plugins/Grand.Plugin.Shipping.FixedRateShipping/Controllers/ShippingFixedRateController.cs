using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Core;
using Grand.Plugin.Shipping.FixedRateShipping.Models;
using Grand.Services.Configuration;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using Grand.Web.Framework.Security;

namespace Grand.Plugin.Shipping.FixedRateShipping.Controllers
{
    [AdminAuthorize]
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
        
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            //little hack here
            //always set culture to 'en-US' (Telerik has a bug related to editing decimal values in other cultures). Like currently it's done for admin area in Global.asax.cs
            CommonHelper.SetTelerikCulture();

            base.Initialize(requestContext);
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            return View("~/Plugins/Shipping.FixedRateShipping/Views/ShippingFixedRate/Configure.cshtml");
        }

        [HttpPost]
        public ActionResult Configure(DataSourceRequest command)
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
        public ActionResult ShippingRateUpdate(FixedShippingRateModel model)
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


        public override IList<string> ValidateShippingForm(FormCollection form)
        {
            //you can implement here any validation logic
            return new List<string>();
        }

        public override JsonResult GetFormPartialView(string shippingOption)
        {
            //you can use here any view 
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}
