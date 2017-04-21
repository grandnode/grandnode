using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using Grand.Web.Validators.Common;

namespace Grand.Web.Models.Common
{
    [Validator(typeof(ContactVendorValidator))]
    public partial class ContactVendorModel : BaseGrandModel
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("ContactVendor.Email")]
        public string Email { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("ContactVendor.Subject")]
        public string Subject { get; set; }
        public bool SubjectEnabled { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("ContactVendor.Enquiry")]
        public string Enquiry { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("ContactVendor.FullName")]
        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}