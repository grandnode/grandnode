using Grand.Core;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Admin.Extensions;
using Grand.Admin.Interfaces;
using Grand.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Admin.Services
{
    public partial class GiftCardViewModelService : IGiftCardViewModelService
    {
        #region Fields
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Constructors

        public GiftCardViewModelService(
            IGiftCardService giftCardService, IOrderService orderService,
            IPriceFormatter priceFormatter, IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper,
            ICurrencyService currencyService, CurrencySettings currencySettings,
            LocalizationSettings localizationSettings,
            ILocalizationService localizationService, ILanguageService languageService,
            ICustomerActivityService customerActivityService, IWorkContext workContext)
        {
            _giftCardService = giftCardService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _workflowMessageService = workflowMessageService;
            _dateTimeHelper = dateTimeHelper;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _localizationSettings = localizationSettings;
            _localizationService = localizationService;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
            _workContext = workContext;
        }

        #endregion

        public virtual async Task<GiftCardModel> PrepareGiftCardModel()
        {
            var model = new GiftCardModel();
            foreach (var currency in await _currencyService.GetAllCurrencies())
                model.AvailableCurrencies.Add(new SelectListItem { Text = currency.Name, Value = currency.CurrencyCode });
            return model;
        }
        public virtual async Task<GiftCardModel> PrepareGiftCardModel(GiftCardModel model)
        {
            foreach (var currency in await _currencyService.GetAllCurrencies())
                model.AvailableCurrencies.Add(new SelectListItem { Text = currency.Name, Value = currency.CurrencyCode });

            return model;
        }

        public virtual GiftCardListModel PrepareGiftCardListModel()
        {
            var model = new GiftCardListModel();
            model.ActivatedList.Add(new SelectListItem {
                Value = "",
                Text = _localizationService.GetResource("Admin.GiftCards.List.Activated.All")
            });
            model.ActivatedList.Add(new SelectListItem {
                Value = "1",
                Text = _localizationService.GetResource("Admin.GiftCards.List.Activated.ActivatedOnly")
            });
            model.ActivatedList.Add(new SelectListItem {
                Value = "2",
                Text = _localizationService.GetResource("Admin.GiftCards.List.Activated.DeactivatedOnly")
            });
            return model;
        }
        public virtual async Task<(IEnumerable<GiftCardModel> giftCardModels, int totalCount)> PrepareGiftCardModel(GiftCardListModel model, int pageIndex, int pageSize)
        {
            bool? isGiftCardActivated = null;
            if (model.ActivatedId == 1)
                isGiftCardActivated = true;
            else if (model.ActivatedId == 2)
                isGiftCardActivated = false;
            var giftCards = await _giftCardService.GetAllGiftCards(isGiftCardActivated: isGiftCardActivated,
                giftCardCouponCode: model.CouponCode,
                recipientName: model.RecipientName,
                pageIndex: pageIndex - 1, pageSize: pageSize);

            var giftcards = new List<GiftCardModel>();
            foreach (var item in giftCards)
            {
                var gift = item.ToModel();
                var currency = await _currencyService.GetCurrencyByCode(item.CurrencyCode);
                if (currency == null)
                    currency = await _currencyService.GetPrimaryStoreCurrency();

                gift.RemainingAmountStr = _priceFormatter.FormatPrice(item.GetGiftCardRemainingAmount(), true, currency, _workContext.WorkingLanguage, true, false);
                gift.AmountStr = _priceFormatter.FormatPrice(item.Amount, true, currency, _workContext.WorkingLanguage, true, false);
                gift.CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc);

                giftcards.Add(gift);
            }

            return (giftcards, giftCards.TotalCount);
        }
        public virtual async Task<GiftCard> InsertGiftCardModel(GiftCardModel model)
        {
            var giftCard = model.ToEntity();
            giftCard.CreatedOnUtc = DateTime.UtcNow;
            await _giftCardService.InsertGiftCard(giftCard);

            //activity log
            await _customerActivityService.InsertActivity("AddNewGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.AddNewGiftCard"), giftCard.GiftCardCouponCode);
            return giftCard;
        }
        public virtual async Task<Order> FillGiftCardModel(GiftCard giftCard, GiftCardModel model)
        {
            Order order = null;
            if (giftCard.PurchasedWithOrderItem != null)
                order = await _orderService.GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id);

            var currency = await _currencyService.GetCurrencyByCode(giftCard.CurrencyCode);
            if (currency == null)
                currency = await _currencyService.GetPrimaryStoreCurrency();

            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? order.Id : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, currency, _workContext.WorkingLanguage, true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, currency, _workContext.WorkingLanguage, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.CurrencyCode = giftCard.CurrencyCode;
            return order;
        }
        public virtual async Task NotifyRecipient(GiftCard giftCard, GiftCardModel model)
        {
            model = giftCard.ToModel();
            var order = await FillGiftCardModel(giftCard, model);
            var languageId = "";
            if (order != null)
            {
                var customerLang = await _languageService.GetLanguageById(order.CustomerLanguageId);
                if (customerLang == null)
                    customerLang = (await _languageService.GetAllLanguages()).FirstOrDefault();
                if (customerLang != null)
                    languageId = customerLang.Id;
            }
            else
            {
                languageId = _localizationSettings.DefaultAdminLanguageId;
            }
            int queuedEmailId = await _workflowMessageService.SendGiftCardNotification(giftCard, order, languageId);
            if (queuedEmailId > 0)
            {
                giftCard.IsRecipientNotified = true;
                await _giftCardService.UpdateGiftCard(giftCard);
                model.IsRecipientNotified = true;
            }

        }
        public virtual async Task<GiftCard> UpdateGiftCardModel(GiftCard giftCard, GiftCardModel model)
        {

            giftCard = model.ToEntity(giftCard);
            await _giftCardService.UpdateGiftCard(giftCard);
            //activity log
            await _customerActivityService.InsertActivity("EditGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.EditGiftCard"), giftCard.GiftCardCouponCode);

            return giftCard;
        }
        public virtual async Task DeleteGiftCard(GiftCard giftCard)
        {
            await _giftCardService.DeleteGiftCard(giftCard);
            //activity log
            await _customerActivityService.InsertActivity("DeleteGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.DeleteGiftCard"), giftCard.GiftCardCouponCode);
        }
        public virtual async Task<GiftCardModel> PrepareGiftCardModel(GiftCard giftCard)
        {
            var model = giftCard.ToModel();
            Order order = null;
            if (giftCard.PurchasedWithOrderItem != null)
                order = await _orderService.GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id);

            var currency = await _currencyService.GetCurrencyByCode(giftCard.CurrencyCode);
            if (currency == null)
                currency = await _currencyService.GetPrimaryStoreCurrency();

            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? order?.Id : null;
            model.PurchasedWithOrderNumber = order?.OrderNumber ?? 0;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, currency, _workContext.WorkingLanguage, true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, currency, _workContext.WorkingLanguage, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.CurrencyCode = giftCard.CurrencyCode;
            return model;
        }
        public virtual async Task<(IEnumerable<GiftCardModel.GiftCardUsageHistoryModel> giftCardUsageHistoryModels, int totalCount)> PrepareGiftCardUsageHistoryModels(GiftCard giftCard, int pageIndex, int pageSize)
        {
            var currency = await _currencyService.GetCurrencyByCode(giftCard.CurrencyCode);
            if (currency == null)
                currency = await _currencyService.GetPrimaryStoreCurrency();

            var items = new List<GiftCardModel.GiftCardUsageHistoryModel>();
            foreach (var x in giftCard.GiftCardUsageHistory.OrderByDescending(gcuh => gcuh.CreatedOnUtc))
            {
                var order = await _orderService.GetOrderById(x.UsedWithOrderId);
                items.Add(new GiftCardModel.GiftCardUsageHistoryModel {
                    Id = x.Id,
                    OrderId = x.UsedWithOrderId,
                    OrderNumber = order != null ? order.OrderNumber : 0,
                    UsedValue = _priceFormatter.FormatPrice(x.UsedValue, true, currency, _workContext.WorkingLanguage, true, false),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return (items.Skip((pageIndex - 1) * pageSize).Take(pageSize), items.Count);
        }

    }
}
