using Grand.Api.DTOs.Customers;
using MediatR;

namespace Grand.Api.Queries.Models.Customers
{
    public class GetCustomerQuery : IRequest<CustomerDto>
    {
        public string Email { get; set; }
    }
}
