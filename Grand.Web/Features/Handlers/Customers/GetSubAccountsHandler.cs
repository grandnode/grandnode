using Grand.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetSubAccountsHandler : IRequestHandler<GetSubAccounts, IList<SubAccountModel>>
    {
        private readonly ICustomerService _customerService;
        public GetSubAccountsHandler(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IList<SubAccountModel>> Handle(GetSubAccounts request, CancellationToken cancellationToken)
        {
            var model = new List<SubAccountModel>();

            var subaccouns = await _customerService.GetAllCustomers(ownerId: request.Customer.Id);
            foreach (var item in subaccouns)
            {
                model.Add(new SubAccountModel() {
                    CustomerId = item.Id,
                    Email = item.Email,
                    Active = item.Active,
                    FirstName = item.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName),
                    LastName = item.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName)
                });
            }

            return model;
        }
    }
}
