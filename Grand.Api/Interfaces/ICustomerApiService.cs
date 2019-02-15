using Grand.Api.DTOs.Customers;
using MongoDB.Driver.Linq;

namespace Grand.Api.Interfaces
{
    public interface ICustomerApiService
    {
        CustomerDto GetByEmail(string email);
        CustomerDto InsertOrUpdateCustomer(CustomerDto model);
        CustomerDto InsertCustomer(CustomerDto model);
        CustomerDto UpdateCustomer(CustomerDto model);
        void DeleteCustomer(CustomerDto model);
        AddressDto InsertAddress(CustomerDto customer, AddressDto model);
        AddressDto UpdateAddress(CustomerDto customer, AddressDto model);
        void DeleteAddress(CustomerDto customer, AddressDto model);
        VendorDto GetVendorById(string id);
        IMongoQueryable<VendorDto> GetVendors();
    }
}
