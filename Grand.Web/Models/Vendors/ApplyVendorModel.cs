using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Vendors
{
    public partial class ApplyVendorModel : BaseGrandModel
    {

        public ApplyVendorModel()
        {
            Address = new VendorAddressModel();
        }

        public VendorAddressModel Address { get; set; }

        [GrandResourceDisplayName("Vendors.ApplyAccount.Name")]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
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