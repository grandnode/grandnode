using Grand.Domain.Catalog;
using MediatR;

namespace Grand.Services.Commands.Models.Catalog
{
    public class SendQuantityBelowStoreOwnerNotificationCommand : IRequest<bool>
    {
        public Product Product { get; set; }
        public ProductAttributeCombination ProductAttributeCombination { get; set; }

    }
}
