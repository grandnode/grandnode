using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Web.Models.Customer;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public interface ICustomerCustomAttributes
    {
        Task<IList<CustomerAttributeModel>> PrepareCustomAttributes(Customer customer, Language language,
            string overrideAttributesXml = "");
        Task<string> ParseCustomAttributes(IFormCollection form);
    }
}
