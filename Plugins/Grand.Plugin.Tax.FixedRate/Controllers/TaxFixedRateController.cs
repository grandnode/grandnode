using System.Collections.Generic;
using Grand.Core;
using Grand.Plugin.Tax.FixedRate.Models;
using Grand.Services.Configuration;
using Grand.Services.Security;
using Grand.Services.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.Tax.FixedRate.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class TaxFixedRateController : BasePluginController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        public TaxFixedRateController(ITaxCategoryService taxCategoryService,
            ISettingService settingService,
            IPermissionService permissionService)
        {
            this._taxCategoryService = taxCategoryService;
            this._settingService = settingService;
            this._permissionService = permissionService;
        }


        public IActionResult Configure()
        {
            return View("~/Plugins/Tax.FixedRate/Views/Configure.cshtml");
        }

        [HttpPost]
        public IActionResult Configure(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return Content("Access denied");

            var taxRateModels = new List<FixedTaxRateModel>();
            foreach (var taxCategory in _taxCategoryService.GetAllTaxCategories())
                taxRateModels.Add(new FixedTaxRateModel
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,
                    Rate = GetTaxRate(taxCategory.Id)
                });

            var gridModel = new DataSourceResult
            {
                Data = taxRateModels,
                Total = taxRateModels.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult TaxRateUpdate(FixedTaxRateModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return Content("Access denied");

            string taxCategoryId = model.TaxCategoryId;
            decimal rate = model.Rate;

            _settingService.SetSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId), rate);

            return new NullJsonResult();
        }

        [NonAction]
        protected decimal GetTaxRate(string taxCategoryId)
        {
            var rate = this._settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId));
            return rate;
        }
    }
}
