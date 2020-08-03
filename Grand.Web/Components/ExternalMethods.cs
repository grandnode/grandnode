using Grand.Core;
using Grand.Framework.Components;
using Grand.Services.Authentication.External;
using Grand.Web.Models.Customer;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class ExternalMethodsViewComponent : BaseViewComponent
    {
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public ExternalMethodsViewComponent(
            IExternalAuthenticationService externalAuthenticationService,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _externalAuthenticationService = externalAuthenticationService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var models = _externalAuthenticationService
                .LoadActiveExternalAuthenticationMethods(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id)
                .Select(authenticationMethod =>
                {
                    authenticationMethod.GetPublicViewComponent(out string viewComponentName);

                    return new ExternalAuthenticationMethodModel {
                        ViewComponentName = viewComponentName
                    };
                }).ToList();

            return View(models);

        }
    }
}