using Grand.Domain.Catalog;
using Grand.Domain.Common;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Commands.Models.Catalog
{
    public class SendNotificationsToSubscribersCommand : IRequest<IList<BackInStockSubscription>>
    {
        public Product Product { get; set; }
        public IList<CustomAttribute> Attributes { get; set; }
        public string Warehouse { get; set; }
    }
}
