using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Grand.Core;
using Grand.Services.Authentication.External;
using Grand.Web.Models.Customer;
using Grand.Web.Services;

namespace Grand.Web.Controllers
{
    public partial class ExternalAuthenticationController : BasePublicController
    {
        #region Fields

        private readonly IExternalAuthenticationWebService _externalAuthenticationWebService;

        #endregion

		#region Constructors

        public ExternalAuthenticationController(IExternalAuthenticationWebService externalAuthenticationWebService)
        {
            this._externalAuthenticationWebService = externalAuthenticationWebService;
        }

        #endregion

        #region Methods

        public RedirectResult RemoveParameterAssociation(string returnUrl)
        {
            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");

            ExternalAuthorizerHelper.RemoveParameters();
            return Redirect(returnUrl);
        }

        [ChildActionOnly]
        public virtual ActionResult ExternalMethods()
        {
            var model = _externalAuthenticationWebService.PrepereExternalAuthenticationMethodModel();
            return PartialView(model);
        }

        #endregion
    }
}
