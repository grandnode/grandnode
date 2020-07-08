using Grand.Domain.Vendors;
using Grand.Web.Areas.Admin.Models.Vendors;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class VendorMappingExtensions
    {
        public static VendorModel ToModel(this Vendor entity)
        {
            return entity.MapTo<Vendor, VendorModel>();
        }

        public static Vendor ToEntity(this VendorModel model)
        {
            return model.MapTo<VendorModel, Vendor>();
        }

        public static Vendor ToEntity(this VendorModel model, Vendor destination)
        {
            return model.MapTo(destination);
        }
    }
}