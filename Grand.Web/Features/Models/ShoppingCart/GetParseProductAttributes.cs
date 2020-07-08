using Grand.Domain.Catalog;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Features.Models.ShoppingCart
{
    public class GetParseProductAttributes : IRequest<string>
    {
        public Product Product { get; set; }
        public IFormCollection Form { get; set; }
    }
}
