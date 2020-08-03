using Grand.Api.Commands.Models.Customers;
using Grand.Api.DTOs.Customers;
using Grand.Api.Extensions;
using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Handlers.Customers
{
    public class AddCustomerCommandHandler : IRequestHandler<AddCustomerCommand, CustomerDto>
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;

        public AddCustomerCommandHandler(
            ICustomerService customerService, 
            ICustomerActivityService customerActivityService, 
            ILocalizationService localizationService, 
            IGenericAttributeService genericAttributeService)
        {
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
        }

        public async Task<CustomerDto> Handle(AddCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = request.Model.ToEntity();
            customer.CreatedOnUtc = DateTime.UtcNow;
            customer.LastActivityDateUtc = DateTime.UtcNow;
            if (string.IsNullOrEmpty(customer.Username))
                customer.Username = customer.Email;

            await _customerService.InsertCustomer(customer);
            await SaveCustomerAttributes(request.Model, customer);
            await SaveCustomerRoles(request.Model, customer);

            //activity log
            await _customerActivityService.InsertActivity("AddNewCustomer", customer.Id, _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);
            return customer.ToModel();
        }

        protected async Task SaveCustomerAttributes(CustomerDto model, Customer customer)
        {
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumberStatusId, model.VatNumberStatusId);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);
        }

        protected async Task SaveCustomerRoles(CustomerDto model, Customer customer)
        {
            var insertRoles = model.CustomerRoles.Except(customer.CustomerRoles.Select(x => x.Id)).ToList();
            foreach (var item in insertRoles)
            {
                var role = await _customerService.GetCustomerRoleById(item);
                if (role != null)
                {
                    customer.CustomerRoles.Add(role);
                    role.CustomerId = customer.Id;
                    await _customerService.InsertCustomerRoleInCustomer(role);
                }
            }
            var deleteRoles = customer.CustomerRoles.Select(x => x.Id).Except(model.CustomerRoles).ToList();
            foreach (var item in deleteRoles)
            {
                var role = await _customerService.GetCustomerRoleById(item);
                if (role != null)
                {
                    customer.CustomerRoles.Remove(customer.CustomerRoles.FirstOrDefault(x => x.Id == role.Id));
                    role.CustomerId = customer.Id;
                    await _customerService.DeleteCustomerRoleInCustomer(role);
                }
            }
        }
    }
}
