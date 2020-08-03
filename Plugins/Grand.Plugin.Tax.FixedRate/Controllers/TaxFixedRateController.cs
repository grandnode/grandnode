using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Plugin.Tax.FixedRate.Models;
using Grand.Services.Configuration;
using Grand.Services.Security;
using Grand.Services.Tax;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.Tax.FixedRate.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.TaxSettings)]
    public class TaxFixedRateController : BasePluginController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;

        public TaxFixedRateController(ITaxCategoryService taxCategoryService, ISettingService settingService)
        {
            _taxCategoryService = taxCategoryService;
            _settingService = settingService;
        }


        public IActionResult Configure()
        {
            return View("~/Plugins/Tax.FixedRate/Views/Configure.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Configure(DataSourceRequest command)
        {
            var taxRateModels = new List<FixedTaxRateModel>();
            foreach (var taxCategory in await _taxCategoryService.GetAllTaxCategories())
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
        public async Task<IActionResult> TaxRateUpdate(FixedTaxRateModel model)
        {
            string taxCategoryId = model.TaxCategoryId;
            decimal rate = model.Rate;

            await _settingService.SetSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId), rate);

            return new NullJsonResult();
        }

        [NonAction]
        protected decimal GetTaxRate(string taxCategoryId)
        {
            var rate = _settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId));
            return rate;
        }
    }
}
