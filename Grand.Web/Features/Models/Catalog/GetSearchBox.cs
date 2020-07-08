using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetSearchBox : IRequest<SearchBoxModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
    }
}
