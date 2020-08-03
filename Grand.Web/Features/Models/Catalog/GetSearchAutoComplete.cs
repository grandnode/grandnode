using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetSearchAutoComplete : IRequest<IList<SearchAutoCompleteModel>>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public Currency Currency { get; set; }
        public string Term { get; set; }
        public string CategoryId { get; set; }
    }
}
