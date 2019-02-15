using Grand.Core.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IGiftCardViewModelService
    {
        GiftCardListModel PrepareGiftCardListModel();
        GiftCardModel PrepareGiftCardModel();
        GiftCardModel PrepareGiftCardModel(GiftCardModel model);
        (IEnumerable<GiftCardModel> giftCardModels, int totalCount) PrepareGiftCardModel(GiftCardListModel model, int pageIndex, int pageSize);
        Order FillGiftCardModel(GiftCard giftCard, GiftCardModel model);
        void NotifyRecipient(GiftCard giftCard, GiftCardModel model);
        GiftCard InsertGiftCardModel(GiftCardModel model);
        GiftCard UpdateGiftCardModel(GiftCard giftCard, GiftCardModel model);
        void DeleteGiftCard(GiftCard giftCard);
        GiftCardModel PrepareGiftCardModel(GiftCard giftCard);
        (IEnumerable<GiftCardModel.GiftCardUsageHistoryModel> giftCardUsageHistoryModels, int totalCount) PrepareGiftCardUsageHistoryModels(GiftCard giftCard, int pageIndex, int pageSize);
    }
}
