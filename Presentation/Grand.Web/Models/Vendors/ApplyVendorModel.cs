using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using Grand.Web.Validators.Vendors;

namespace Grand.Web.Models.Vendors
{
    [Validator(typeof(ApplyVendorValidator))]
    public partial class ApplyVendorModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Vendors.ApplyAccount.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [GrandResourceDisplayName("Vendors.ApplyAccount.Email")]
        [AllowHtml]
        public string Email { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool DisableFormInput { get; set; }
        public string Result { get; set; }
    }
}