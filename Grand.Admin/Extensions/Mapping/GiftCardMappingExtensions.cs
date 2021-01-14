using Grand.Domain.Orders;
using Grand.Admin.Models.Orders;

namespace Grand.Admin.Extensions
{
    public static class GiftCardMappingExtensions
    {
        public static GiftCardModel ToModel(this GiftCard entity)
        {
            return entity.MapTo<GiftCard, GiftCardModel>();
        }

        public static GiftCard ToEntity(this GiftCardModel model)
        {
            return model.MapTo<GiftCardModel, GiftCard>();
        }

        public static GiftCard ToEntity(this GiftCardModel model, GiftCard destination)
        {
            return model.MapTo(destination);
        }
    }
}