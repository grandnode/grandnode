using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Common;
using MediatR;

namespace Grand.Web.Features.Models.Common
{
    public class GetSitemap : IRequest<SitemapModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
    }
}
