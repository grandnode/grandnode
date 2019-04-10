using Grand.Api.DTOs.Customers;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface ICustomerRoleApiService
    {
        Task<CustomerRoleDto> GetById(string id);
        IMongoQueryable<CustomerRoleDto> GetCustomerRoles();
        Task<CustomerRoleDto> InsertOrUpdateCustomerRole(CustomerRoleDto model);
        Task<CustomerRoleDto> InsertCustomerRole(CustomerRoleDto model);
        Task<CustomerRoleDto> UpdateCustomerRole(CustomerRoleDto model);
        Task DeleteCustomerRole(CustomerRoleDto model);
    }
}
