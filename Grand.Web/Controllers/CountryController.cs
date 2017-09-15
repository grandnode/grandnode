using System;
using Microsoft.AspNetCore.Mvc;
using Grand.Services.Localization;
using System.Collections.Generic;
using Grand.Web.Services;
using Grand.Framework.Mvc.Filters;

namespace Grand.Web.Controllers
{
    public partial class CountryController : BasePublicController
	{
		#region Fields

        private readonly ICountryWebService _countryWebService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Constructors

        public CountryController(ICountryWebService countryWebService, ILocalizationService localizationService)
		{
            this._countryWebService = countryWebService;
            this._localizationService = localizationService;

        }

        #endregion

        #region States / provinces

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            //this action method gets called via an ajax request
            if (String.IsNullOrEmpty(countryId))
            {
                return Json(new List<dynamic>() { new { id = "", name = _localizationService.GetResource("Address.SelectState") } });
            }
            var model = _countryWebService.PrepareModel(countryId, addSelectStateItem);
            return Json(model);
        }

        #endregion
    }
}
