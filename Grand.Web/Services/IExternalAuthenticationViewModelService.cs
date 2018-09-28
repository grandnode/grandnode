using Grand.Web.Models.Customer;
using System.Collections.Generic;

namespace Grand.Web.Services
{
    public partial interface IExternalAuthenticationViewModelService
    {
        List<ExternalAuthenticationMethodModel> PrepereExternalAuthenticationMethodModel();
    }
}