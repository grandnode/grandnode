using Grand.Core;
using Grand.Services.Authentication.External;
using Grand.Web.Models.Customer;
using System.Collections.Generic;
using System.Web.Routing;

namespace Grand.Web.Services
{
    public partial class ExternalAuthenticationWebService: IExternalAuthenticationWebService
    {
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly IStoreContext _storeContext;

        public ExternalAuthenticationWebService(IOpenAuthenticationService openAuthenticationService,
            IStoreContext storeContext)
        {
            this._openAuthenticationService = openAuthenticationService;
            this._storeContext = storeContext;
        }

        public virtual List<ExternalAuthenticationMethodModel> PrepereExternalAuthenticationMethodModel()
        {
            //model
            var model = new List<ExternalAuthenticationMethodModel>();

            foreach (var eam in _openAuthenticationService
                .LoadActiveExternalAuthenticationMethods(_storeContext.CurrentStore.Id))
            {
                var eamModel = new ExternalAuthenticationMethodModel();

                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                eam.GetPublicInfoRoute(out actionName, out controllerName, out routeValues);
                eamModel.ActionName = actionName;
                eamModel.ControllerName = controllerName;
                eamModel.RouteValues = routeValues;

                model.Add(eamModel);
            }
            return model;
        }
    }
}