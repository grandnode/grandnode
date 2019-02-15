using Grand.Web.Models.Customer;
using System.Collections.Generic;

namespace Grand.Web.Interfaces
{
    public partial interface IExternalAuthenticationViewModelService
    {
        List<ExternalAuthenticationMethodModel> PrepereExternalAuthenticationMethodModel();
    }
}