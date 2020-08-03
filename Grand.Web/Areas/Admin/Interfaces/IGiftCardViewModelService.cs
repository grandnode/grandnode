using Grand.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IGiftCardViewModelService
    {
        GiftCardListModel PrepareGiftCardListModel();
        Task<GiftCardModel> PrepareGiftCardModel();
        Task<GiftCardModel> PrepareGiftCardModel(GiftCardModel model);
        Task<(IEnumerable<GiftCardModel> giftCardModels, int totalCount)> PrepareGiftCardModel(GiftCardListModel model, int pageIndex, int pageSize);
        Task<Order> FillGiftCardModel(GiftCard giftCard, GiftCardModel model);
        Task NotifyRecipient(GiftCard giftCard, GiftCardModel model);
        Task<GiftCard> InsertGiftCardModel(GiftCardModel model);
        Task<GiftCard> UpdateGiftCardModel(GiftCard giftCard, GiftCardModel model);
        Task DeleteGiftCard(GiftCard giftCard);
        Task<GiftCardModel> PrepareGiftCardModel(GiftCard giftCard);
        Task<(IEnumerable<GiftCardModel.GiftCardUsageHistoryModel> giftCardUsageHistoryModels, int totalCount)> PrepareGiftCardUsageHistoryModels(GiftCard giftCard, int pageIndex, int pageSize);
    }
}
