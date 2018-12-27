using Grand.Api.DTOs.Customers;

namespace Grand.Api.Services
{
    public interface ICustomerApiService
    {
        CustomerDto GetByEmail(string email);
        CustomerDto InsertOrUpdateCustomer(CustomerDto model);
        CustomerDto InsertCustomer(CustomerDto model);
        CustomerDto UpdateCustomer(CustomerDto model);
        void DeleteCustomer(CustomerDto model);
    }
}
