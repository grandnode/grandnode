using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetHomepageCategory : IRequest<IList<CategoryModel>>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
    }
}
