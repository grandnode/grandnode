using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using MediatR;

namespace Grand.Services.Commands.Models.Catalog
{
    public class SendQuantityBelowStoreOwnerNotificationCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Product Product { get; set; }
        public ProductAttributeCombination ProductAttributeCombination { get; set; }

    }
}
