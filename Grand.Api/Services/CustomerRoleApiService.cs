using Grand.Api.DTOs.Customers;
using Grand.Api.Extensions;
using Grand.Data;
using Grand.Services.Customers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace Grand.Api.Services
{
    public partial class CustomerRoleApiService : ICustomerRoleApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly ICustomerService _customerService;
        private readonly IMongoCollection<CustomerRoleDto> _customerRole;

        public CustomerRoleApiService(IMongoDBContext mongoDBContext, ICustomerService customerService)
        {
            _mongoDBContext = mongoDBContext;
            _customerService = customerService;
            _customerRole = _mongoDBContext.Database().GetCollection<CustomerRoleDto>(typeof(Core.Domain.Customers.CustomerRole).Name);
        }
        public virtual CustomerRoleDto GetById(string id)
        {
            return _customerRole.AsQueryable().FirstOrDefault(x => x.Id == id);
        }

        public virtual IMongoQueryable<CustomerRoleDto> GetCustomerRoles()
        {
            return _customerRole.AsQueryable();
        }

        public virtual CustomerRoleDto InsertCustomerRole(CustomerRoleDto model)
        {
            var customerRole = model.ToEntity();
            _customerService.InsertCustomerRole(customerRole);
            return customerRole.ToModel();
        }

        public virtual CustomerRoleDto UpdateCustomerRole(CustomerRoleDto model)
        {
            var customerRole = _customerService.GetCustomerRoleById(model.Id);
            customerRole = model.ToEntity(customerRole);
            _customerService.UpdateCustomerRole(customerRole);
            return customerRole.ToModel();
        }

        public virtual void DeleteCustomerRole(CustomerRoleDto model)
        {
            var customerRole = _customerService.GetCustomerRoleById(model.Id);
            if (customerRole != null)
                _customerService.DeleteCustomerRole(customerRole);
        }


    }
}
