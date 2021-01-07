using Grand.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Customers
{
    public class GetParseCustomAttributes : IRequest<IList<CustomAttribute>>
    {
        public IFormCollection Form { get; set; }
    }
}
