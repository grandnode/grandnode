using MediatR;

namespace Grand.Services.Commands.Models.Common
{
    public class InsertContactUsCommand : IRequest<bool>
    {
        public string CustomerId { get; set; }
        public string StoreId { get; set; }
        public string VendorId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Subject { get; set; }
        public string Enquiry { get; set; }
        public string ContactAttributeDescription { get; set; }
        public string ContactAttributesXml { get; set; }
        public string EmailAccountId { get; set; }
    }

}
