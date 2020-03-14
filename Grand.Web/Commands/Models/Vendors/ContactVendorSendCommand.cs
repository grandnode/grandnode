using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Common;
using MediatR;

namespace Grand.Web.Commands.Models.Vendors
{
    public class ContactVendorSendCommand : IRequest<ContactVendorModel>
    {
        public Vendor Vendor { get; set; }
        public ContactVendorModel Model { get; set; }

    }
}
