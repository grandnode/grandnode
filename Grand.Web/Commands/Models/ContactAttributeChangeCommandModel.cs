using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Stores;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Commands.Models
{
    public class ContactAttributeChangeCommandModel : IRequest<(IList<string> enabledAttributeIds, IList<string> disabledAttributeIds)>
    {
        public IFormCollection Form { get; set; }
        public Customer Customer { get; set; }
        public Store Store { get; set; }

    }
}
