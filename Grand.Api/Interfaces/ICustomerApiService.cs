using Grand.Api.DTOs.Customers;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface ICustomerApiService
    {
        Task<CustomerDto> GetByEmail(string email);
        Task<CustomerDto> InsertOrUpdateCustomer(CustomerDto model);
        Task<CustomerDto> InsertCustomer(CustomerDto model);
        Task<CustomerDto> UpdateCustomer(CustomerDto model);
        Task DeleteCustomer(CustomerDto model);
        Task<AddressDto> InsertAddress(CustomerDto customer, AddressDto model);
        Task<AddressDto> UpdateAddress(CustomerDto customer, AddressDto model);
        Task DeleteAddress(CustomerDto customer, AddressDto model);
        Task<VendorDto> GetVendorById(string id);
        IMongoQueryable<VendorDto> GetVendors();
    }
}
