using Grand.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class BannerMappingExtensions
    {
        public static BannerModel ToModel(this Banner entity)
        {
            return entity.MapTo<Banner, BannerModel>();
        }

        public static Banner ToEntity(this BannerModel model)
        {
            return model.MapTo<BannerModel, Banner>();
        }

        public static Banner ToEntity(this BannerModel model, Banner destination)
        {
            return model.MapTo(destination);
        }
    }
}