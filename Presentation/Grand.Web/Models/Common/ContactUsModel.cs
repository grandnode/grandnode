using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using Grand.Web.Validators.Common;

namespace Grand.Web.Models.Common
{
    [Validator(typeof(ContactUsValidator))]
    public partial class ContactUsModel : BaseGrandModel
    {
        [AllowHtml]
        [GrandResourceDisplayName("ContactUs.Email")]
        public string Email { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("ContactUs.Subject")]
        public string Subject { get; set; }
        public bool SubjectEnabled { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("ContactUs.FullName")]
        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}