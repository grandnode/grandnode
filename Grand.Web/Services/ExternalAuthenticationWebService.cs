using Grand.Core;
using Grand.Services.Authentication.External;
using Grand.Web.Models.Customer;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial class ExternalAuthenticationWebService: IExternalAuthenticationWebService
    {
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public ExternalAuthenticationWebService(IExternalAuthenticationService externalAuthenticationService,
            IStoreContext storeContext, IWorkContext workContext)
        {
            this._externalAuthenticationService = externalAuthenticationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        public virtual List<ExternalAuthenticationMethodModel> PrepereExternalAuthenticationMethodModel()
        {
            var models = _externalAuthenticationService
                .LoadActiveExternalAuthenticationMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id)
                .Select(authenticationMethod =>
                {
                    authenticationMethod.GetPublicViewComponent(out string viewComponentName);

                    return new ExternalAuthenticationMethodModel
                    {
                        ViewComponentName = viewComponentName
                    };
                }).ToList();

            return models;
        }
    }
}