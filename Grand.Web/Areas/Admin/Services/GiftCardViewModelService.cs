using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
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
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Constructors

        public GiftCardViewModelService(
            IGiftCardService giftCardService, IOrderService orderService,
            IPriceFormatter priceFormatter, IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper, 
            ICurrencyService currencyService, CurrencySettings currencySettings,
            ILocalizationService localizationService, ILanguageService languageService,
            ICustomerActivityService customerActivityService)
        {
            _giftCardService = giftCardService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _workflowMessageService = workflowMessageService;
            _dateTimeHelper = dateTimeHelper;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _localizationService = localizationService;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
        }

        #endregion

        public virtual GiftCardModel PrepareGiftCardModel()
        {
            var model = new GiftCardModel();
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            return model;
        }
        public virtual GiftCardModel PrepareGiftCardModel(GiftCardModel model)
        {
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            return model;
        }

        public virtual GiftCardListModel PrepareGiftCardListModel()
        {
            var model = new GiftCardListModel();
            model.ActivatedList.Add(new SelectListItem
            {
                Value = "",
                Text = _localizationService.GetResource("Admin.GiftCards.List.Activated.All")
            });
            model.ActivatedList.Add(new SelectListItem
            {
                Value = "1",
                Text = _localizationService.GetResource("Admin.GiftCards.List.Activated.ActivatedOnly")
            });
            model.ActivatedList.Add(new SelectListItem
            {
                Value = "2",
                Text = _localizationService.GetResource("Admin.GiftCards.List.Activated.DeactivatedOnly")
            });
            return model;
        }
        public virtual (IEnumerable<GiftCardModel> giftCardModels, int totalCount) PrepareGiftCardModel(GiftCardListModel model, int pageIndex, int pageSize)
        {
            bool? isGiftCardActivated = null;
            if (model.ActivatedId == 1)
                isGiftCardActivated = true;
            else if (model.ActivatedId == 2)
                isGiftCardActivated = false;
            var giftCards = _giftCardService.GetAllGiftCards(isGiftCardActivated: isGiftCardActivated,
                giftCardCouponCode: model.CouponCode,
                recipientName: model.RecipientName,
                pageIndex: pageIndex - 1, pageSize: pageSize);
            return (giftCards.Select(x =>
            {
                var m = x.ToModel();
                m.RemainingAmountStr = _priceFormatter.FormatPrice(x.GetGiftCardRemainingAmount(), true, false);
                m.AmountStr = _priceFormatter.FormatPrice(x.Amount, true, false);
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                return m;
            }), giftCards.TotalCount);
        }
        public virtual GiftCard InsertGiftCardModel(GiftCardModel model)
        {
            var giftCard = model.ToEntity();
            giftCard.CreatedOnUtc = DateTime.UtcNow;
            _giftCardService.InsertGiftCard(giftCard);

            //activity log
            _customerActivityService.InsertActivity("AddNewGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.AddNewGiftCard"), giftCard.GiftCardCouponCode);
            return giftCard;
        }
        public virtual Order FillGiftCardModel(GiftCard giftCard, GiftCardModel model)
        {
            Order order = null;
            if (giftCard.PurchasedWithOrderItem != null)
                order = _orderService.GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id);

            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? order.Id : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            return order;
        }
        public virtual void NotifyRecipient(GiftCard giftCard, GiftCardModel model)
        {
            model = giftCard.ToModel();
            var order = FillGiftCardModel(giftCard, model);
            var languageId = "";
            if (order != null)
            {
                var customerLang = _languageService.GetLanguageById(order.CustomerLanguageId);
                if (customerLang == null)
                    customerLang = _languageService.GetAllLanguages().FirstOrDefault();
                if (customerLang != null)
                    languageId = customerLang.Id;
            }
            else
            {
                languageId = Grand.Core.Infrastructure.EngineContext.Current.Resolve<LocalizationSettings>().DefaultAdminLanguageId;
            }
            int queuedEmailId = _workflowMessageService.SendGiftCardNotification(giftCard, languageId);
            if (queuedEmailId > 0)
            {
                giftCard.IsRecipientNotified = true;
                _giftCardService.UpdateGiftCard(giftCard);
                model.IsRecipientNotified = true;
            }

        }
        public virtual GiftCard UpdateGiftCardModel(GiftCard giftCard, GiftCardModel model)
        {
           
            giftCard = model.ToEntity(giftCard);
            _giftCardService.UpdateGiftCard(giftCard);
            //activity log
            _customerActivityService.InsertActivity("EditGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.EditGiftCard"), giftCard.GiftCardCouponCode);

            return giftCard;
        }
        public virtual void DeleteGiftCard(GiftCard giftCard)
        {
            _giftCardService.DeleteGiftCard(giftCard);
            //activity log
            _customerActivityService.InsertActivity("DeleteGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.DeleteGiftCard"), giftCard.GiftCardCouponCode);
        }
        public virtual GiftCardModel PrepareGiftCardModel(GiftCard giftCard)
        {
            var model = giftCard.ToModel();
            Order order = null;
            if (giftCard.PurchasedWithOrderItem != null)
                order = _orderService.GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id);

            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? order?.Id : null;
            model.PurchasedWithOrderNumber = order?.OrderNumber ?? 0;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            return model;
        }
        public (IEnumerable<GiftCardModel.GiftCardUsageHistoryModel> giftCardUsageHistoryModels, int totalCount) PrepareGiftCardUsageHistoryModels(GiftCard giftCard, int pageIndex, int pageSize)
        {
            var usageHistoryModel = giftCard.GiftCardUsageHistory.OrderByDescending(gcuh => gcuh.CreatedOnUtc)
                .Select(x => new GiftCardModel.GiftCardUsageHistoryModel
                {
                    Id = x.Id,
                    OrderId = x.UsedWithOrderId,
                    OrderNumber = _orderService.GetOrderById(x.UsedWithOrderId) != null ? _orderService.GetOrderById(x.UsedWithOrderId).OrderNumber : 0,
                    UsedValue = _priceFormatter.FormatPrice(x.UsedValue, true, false),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                })
                .ToList();
            return (
                 usageHistoryModel.Skip((pageIndex - 1) * pageSize).Take(pageSize),
                 usageHistoryModel.Count);
        }

    }
}
