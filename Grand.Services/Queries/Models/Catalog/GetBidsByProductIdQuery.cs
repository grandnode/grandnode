using Grand.Domain.Catalog;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Queries.Models.Catalog
{
    public class GetBidsByProductIdQuery : IRequest<IList<Bid>>
    {
        public string ProductId { get; set; }
    }
}
