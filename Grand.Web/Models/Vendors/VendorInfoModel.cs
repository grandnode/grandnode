using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Models.Vendors
{
    public partial class VendorInfoModel : BaseModel
    {
        public VendorInfoModel()
        {
            Address = new VendorAddressModel();
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