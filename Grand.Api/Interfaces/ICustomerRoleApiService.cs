using Grand.Api.DTOs.Customers;
using MongoDB.Driver.Linq;

namespace Grand.Api.Interfaces
{
    public interface ICustomerRoleApiService
    {
        CustomerRoleDto GetById(string id);
        IMongoQueryable<CustomerRoleDto> GetCustomerRoles();
        CustomerRoleDto InsertOrUpdateCustomerRole(CustomerRoleDto model);
        CustomerRoleDto InsertCustomerRole(CustomerRoleDto model);
        CustomerRoleDto UpdateCustomerRole(CustomerRoleDto model);
        void DeleteCustomerRole(CustomerRoleDto model);
    }
}
