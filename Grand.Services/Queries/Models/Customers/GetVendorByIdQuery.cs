using Grand.Domain.Vendors;
using MediatR;
namespace Grand.Services.Queries.Models.Customers
{
    public class GetVendorByIdQuery : IRequest<Vendor>
    {
        public string Id { get; set; }
    }
}
