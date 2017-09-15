using Microsoft.AspNetCore.Mvc;
using Grand.Services.Authentication.External;
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

        #endregion
    }
}
