using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Messages;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Messages
{
    public partial class ContactFormModel: BaseNopEntityModel
    {
        public override string Id { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.Fields.Store")]
        public string Store { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }
        public string FullName { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.Fields.IpAddress")]
        public string IpAddress { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.Fields.Subject")]
        [AllowHtml]
        public string Subject { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.Fields.Enquiry")]
        [AllowHtml]
        public string Enquiry { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.Fields.EmailAccountName")]
        [AllowHtml]
        public string EmailAccountName { get; set; }
    }
}