using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetSearch : IRequest<SearchModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public Currency Currency { get; set; }
        public SearchModel Model { get; set; }
        public CatalogPagingFilteringModel Command { get; set; }
        public bool IsSearchTermSpecified { get; set; }
    }
}
