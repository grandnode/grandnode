using Grand.Domain.Stores;
using Grand.Web.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Commands.Models.Common
{
    public class ContactUsSendCommand : IRequest<(ContactUsModel model, IList<string> errors)>
    {
        public ContactUsModel Model { get; set; }
        public IFormCollection Form { get; set; }
        public bool CaptchaValid { get; set; }
        public Store Store { get; set; }
    }
}
