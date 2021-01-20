using Grand.Domain.Customers;
using Grand.Services.Commands.Models.Customers;
using Grand.Services.Customers;
using Grand.Services.Notifications.Customers;
using Grand.Services.Vendors;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Customers
{
    public class ActiveVendorCommandHandler : IRequestHandler<ActiveVendorCommand, bool>
    {
        private readonly IVendorService _vendorService;
        private readonly ICustomerService _customerService;
        private readonly IMediator _mediator;

        public ActiveVendorCommandHandler(
            IVendorService vendorService,
            ICustomerService customerService,
            IMediator mediator)
        {
            _vendorService = vendorService;
            _customerService = customerService;
            _mediator = mediator;
        }

        public async Task<bool> Handle(ActiveVendorCommand request, CancellationToken cancellationToken)
        {
            //update vendor - set active
            request.Vendor.Active = request.Active;
            await _vendorService.UpdateVendor(request.Vendor);

            //assign vendor role for customers
            var vendorRole = await _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Vendors);

            foreach (var item in request.CustomerIds)
            {
                var customer = await _customerService.GetCustomerById(item);
                if (customer != null && !customer.Deleted && customer.Active && !customer.IsSystemAccount &&
                    !customer.IsAdmin()
                    )
                {
                    if (vendorRole != null)
                    {
                        vendorRole.CustomerId = item;
                        if (request.Active)
                        {
                            if (!customer.IsVendor())
                                await _customerService.InsertCustomerRoleInCustomer(vendorRole);
                        }
                        else
                        {
                            if (customer.IsVendor())
                                await _customerService.DeleteCustomerRoleInCustomer(vendorRole);
                        }
                    }
                }
            }

            //raise event       
            await _mediator.Publish(new VendorActivationEvent(request.Vendor));

            return true;
        }
    }
}
