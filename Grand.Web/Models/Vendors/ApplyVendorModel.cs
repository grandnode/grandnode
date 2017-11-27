using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Vendors;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Vendors
{
    [Validator(typeof(ApplyVendorValidator))]
    public partial class ApplyVendorModel : BaseGrandModel
    {

        public ApplyVendorModel()
        {
            this.Address = new VendorAddressModel();
        }

        public VendorAddressModel Address { get; set; }

        [GrandResourceDisplayName("Vendors.ApplyAccount.Name")]
        public string Name { get; set; }
        [GrandResourceDisplayName("Vendors.ApplyAccount.Email")]
        public string Email { get; set; }
        [GrandResourceDisplayName("Vendors.ApplyAccount.Description")]
        public string Description { get; set; }
        public bool DisplayCaptcha { get; set; }
        public bool TermsOfServiceEnabled { get; set; }
        public bool TermsOfServicePopup { get; set; }
        public bool DisableFormInput { get; set; }
        public string Result { get; set; }
    }
}