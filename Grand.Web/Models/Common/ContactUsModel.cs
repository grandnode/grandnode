using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Framework;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Common;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Common
{
    [Validator(typeof(ContactUsValidator))]
    public partial class ContactUsModel : BaseGrandModel
    {
        [GrandResourceDisplayName("ContactUs.Email")]
        public string Email { get; set; }

        [GrandResourceDisplayName("ContactUs.Subject")]
        public string Subject { get; set; }
        public bool SubjectEnabled { get; set; }

        [GrandResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        [GrandResourceDisplayName("ContactUs.FullName")]
        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}