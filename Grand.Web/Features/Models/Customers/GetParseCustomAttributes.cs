using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Features.Models.Customers
{
    public class GetParseCustomAttributes : IRequest<string>
    {
        public IFormCollection Form { get; set; }
    }
}
