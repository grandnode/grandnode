using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;

namespace Grand.Admin.Models.Messages
{
    public partial class ContactFormModel: BaseEntityModel
    {
        public override string Id { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.Store")]
        public string Store { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.Email")]
        public string Email { get; set; }
        public string FullName { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.IpAddress")]
        public string IpAddress { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.Subject")]
        public string Subject { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.Enquiry")]
        public string Enquiry { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.ContactAttributeDescription")]
        public string ContactAttributeDescription { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.Fields.EmailAccountName")]
        
        public string EmailAccountName { get; set; }
    }
}