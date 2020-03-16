using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Features.Models.Common
{
    public class GetSitemapXml : IRequest<string>
    {
        public int? Id { get; set; }
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public IUrlHelper UrlHelper { get; set; }
    }
}
