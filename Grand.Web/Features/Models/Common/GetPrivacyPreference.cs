using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Web.Models.Common;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Common
{
    public class GetPrivacyPreference : IRequest<IList<PrivacyPreferenceModel>>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
    }
}
