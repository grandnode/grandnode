using Grand.Api.DTOs.Customers;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Core.Data;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public partial class CustomerRoleApiService : ICustomerRoleApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        private readonly IMongoCollection<CustomerRoleDto> _customerRole;

        public CustomerRoleApiService(IMongoDBContext mongoDBContext, ICustomerService customerService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService)
        {
            _mongoDBContext = mongoDBContext;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;

            _customerRole = _mongoDBContext.Database().GetCollection<CustomerRoleDto>(typeof(Core.Domain.Customers.CustomerRole).Name);
        }
        public virtual Task<CustomerRoleDto> GetById(string id)
        {
            return _customerRole.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public virtual IMongoQueryable<CustomerRoleDto> GetCustomerRoles()
        {
            return _customerRole.AsQueryable();
        }
        public virtual async Task<CustomerRoleDto> InsertOrUpdateCustomerRole(CustomerRoleDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = await InsertCustomerRole(model);
            else
                model = await UpdateCustomerRole(model);

            return model;
        }
        public virtual async Task<CustomerRoleDto> InsertCustomerRole(CustomerRoleDto model)
        {
            var customerRole = model.ToEntity();
            await _customerService.InsertCustomerRole(customerRole);

            //activity log
            await _customerActivityService.InsertActivity("AddNewCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerRole"), customerRole.Name);

            return customerRole.ToModel();
        }

        public virtual async Task<CustomerRoleDto> UpdateCustomerRole(CustomerRoleDto model)
        {
            var customerRole = await _customerService.GetCustomerRoleById(model.Id);
            customerRole = model.ToEntity(customerRole);
            await _customerService.UpdateCustomerRole(customerRole);

            //activity log
            await _customerActivityService.InsertActivity("EditCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.EditCustomerRole"), customerRole.Name);

            return customerRole.ToModel();
        }

        public virtual async Task DeleteCustomerRole(CustomerRoleDto model)
        {
            var customerRole = await _customerService.GetCustomerRoleById(model.Id);
            if (customerRole != null)
            {
                await _customerService.DeleteCustomerRole(customerRole);

                //activity log
                await _customerActivityService.InsertActivity("DeleteCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerRole"), customerRole.Name);
            }
        }


    }
}
