using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Features.Models.Common
{
    public class GetParseCustomAddressAttributes : IRequest<string>
    {
        public IFormCollection Form { get; set; }
    }
}
