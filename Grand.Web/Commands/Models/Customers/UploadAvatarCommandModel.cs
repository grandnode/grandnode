using Grand.Core.Domain.Customers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Commands.Models.Customers
{
    public class UploadAvatarCommandModel : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public IFormFile UploadedFile { get; set; }
        public bool Remove { get; set; }
    }
}
