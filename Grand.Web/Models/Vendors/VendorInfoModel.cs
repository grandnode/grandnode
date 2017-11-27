using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Web.Validators.Vendors;

namespace Grand.Web.Models.Vendors
{
    [Validator(typeof(VendorInfoValidator))]
    public partial class VendorInfoModel : BaseGrandModel
    {
        public VendorInfoModel()
        {
            this.Address = new VendorAddressModel();
        }

        [GrandResourceDisplayName("Account.VendorInfo.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Account.VendorInfo.Email")]
        public string Email { get; set; }

        [GrandResourceDisplayName("Account.VendorInfo.Description")]
        public string Description { get; set; }

        [GrandResourceDisplayName("Account.VendorInfo.Picture")]
        public string PictureUrl { get; set; }

        public VendorAddressModel Address { get; set; }
    }
}