using Grand.Framework.Mvc.Filters;
using Grand.Services.Localization;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class CountryController : BasePublicController
	{
		#region Fields

        private readonly ICountryViewModelService _countryViewModelService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Constructors

        public CountryController(ICountryViewModelService countryViewModelService, ILocalizationService localizationService)
		{
            this._countryViewModelService = countryViewModelService;
            this._localizationService = localizationService;

        }

        #endregion

        #region States / provinces

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual async Task<IActionResult> GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            //this action method gets called via an ajax request
            if (String.IsNullOrEmpty(countryId))
            {
                return Json(new List<dynamic>() { new { id = "", name = _localizationService.GetResource("Address.SelectState") } });
            }
            var model = await _countryViewModelService.PrepareModel(countryId, addSelectStateItem);
            return Json(model);
        }

        #endregion
    }
}
