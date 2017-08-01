using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Framework;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Common;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Common
{
    [Validator(typeof(ContactVendorValidator))]
    public partial class ContactVendorModel : BaseGrandModel
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }

        [GrandResourceDisplayName("ContactVendor.Email")]
        public string Email { get; set; }

        [GrandResourceDisplayName("ContactVendor.Subject")]
        public string Subject { get; set; }
        public bool SubjectEnabled { get; set; }

        [GrandResourceDisplayName("ContactVendor.Enquiry")]
        public string Enquiry { get; set; }

        [GrandResourceDisplayName("ContactVendor.FullName")]
        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}