using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Web.Commands.Models.Customers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Customers
{
    public class SubAccountAddCommandHandler : IRequestHandler<SubAccountAddCommand, CustomerRegistrationResult>
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly CustomerSettings _customerSettings;

        public SubAccountAddCommandHandler(
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            IGenericAttributeService genericAttributeService,
            CustomerSettings customerSettings)
        {
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;
            _genericAttributeService = genericAttributeService;
            _customerSettings = customerSettings;
        }
        public async Task<CustomerRegistrationResult> Handle(SubAccountAddCommand request, CancellationToken cancellationToken)
        {
            var customer = await PrepareCustomer(request);

            var registrationRequest = new CustomerRegistrationRequest(customer, request.Model.Email,
                    request.Model.Email, request.Model.Password,
                    _customerSettings.DefaultPasswordFormat, request.Store.Id, request.Model.Active);

            var customerRegistrationResult = await _customerRegistrationService.RegisterCustomer(registrationRequest);

            if (!customerRegistrationResult.Success)
            {
                await _customerService.DeleteCustomer(customer, true);
            }
            else
            {
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, request.Model.FirstName);
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, request.Model.LastName);
            }
            return customerRegistrationResult;
        }

        protected async Task<Customer> PrepareCustomer(SubAccountAddCommand request)
        {
            var customer = new Customer();
            customer.OwnerId = request.Customer.Id;
            customer.CustomerGuid = Guid.NewGuid();
            customer.StoreId = request.Store.Id;
            customer.CreatedOnUtc = DateTime.UtcNow;
            customer.LastActivityDateUtc = DateTime.UtcNow;

            await _customerService.InsertCustomer(customer);
            return customer;
        }

    }
}
