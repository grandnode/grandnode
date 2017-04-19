using System;
using System.Web.Mvc;
using Grand.Services.Localization;
using Grand.Web.Framework;
using System.Collections.Generic;
using Grand.Web.Services;

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

        [AcceptVerbs(HttpVerbs.Get)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        public virtual ActionResult GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            //this action method gets called via an ajax request
            if (String.IsNullOrEmpty(countryId))
            {
                return Json(new List<dynamic>() { new { id = "", name = _localizationService.GetResource("Address.SelectState") } }, JsonRequestBehavior.AllowGet);
            }
            var model = _countryWebService.PrepareModel(countryId, addSelectStateItem);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
