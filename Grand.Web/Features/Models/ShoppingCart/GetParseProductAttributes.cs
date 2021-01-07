using Grand.Domain.Catalog;
using Grand.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.ShoppingCart
{
    public class GetParseProductAttributes : IRequest<IList<CustomAttribute>>
    {
        public Product Product { get; set; }
        public IFormCollection Form { get; set; }
    }
}
