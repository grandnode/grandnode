using Grand.Domain.Customers;
using MediatR;
namespace Grand.Services.Queries.Models.Customers
{
    public class GetCustomerByIdQuery : IRequest<Customer>
    {
        public string Id { get; set; }
    }
}
