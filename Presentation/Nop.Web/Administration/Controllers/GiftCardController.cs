using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Core.Infrastructure;

namespace Nop.Admin.Controllers
{
    public partial class GiftCardController : BaseAdminController
    {
        #region Fields

        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public GiftCardController(IGiftCardService giftCardService, IOrderService orderService,
            IPriceFormatter priceFormatter, IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper, LocalizationSettings localizationSettings,
            ICurrencyService currencyService, CurrencySettings currencySettings,
            ILocalizationService localizationService, ILanguageService languageService,
            ICustomerActivityService customerActivityService, IPermissionService permissionService)
        {
            this._giftCardService = giftCardService;
            this._orderService = orderService;
            this._priceFormatter = priceFormatter;
            this._workflowMessageService = workflowMessageService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationSettings = localizationSettings;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Methods

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

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
            return View(model);
        }

        [HttpPost]
        public ActionResult GiftCardList(DataSourceRequest command, GiftCardListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            bool? isGiftCardActivated = null;
            if (model.ActivatedId == 1)
                isGiftCardActivated = true;
            else if (model.ActivatedId == 2)
                isGiftCardActivated = false;
            var giftCards = _giftCardService.GetAllGiftCards(isGiftCardActivated: isGiftCardActivated,
                giftCardCouponCode: model.CouponCode,
                recipientName: model.RecipientName,
                pageIndex: command.Page - 1, pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = giftCards.Select(x =>
                {
                    var m = x.ToModel();
                    m.RemainingAmountStr = _priceFormatter.FormatPrice(x.GetGiftCardRemainingAmount(), true, false);
                    m.AmountStr = _priceFormatter.FormatPrice(x.Amount, true, false);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    return m;
                }),
                Total = giftCards.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var model = new GiftCardModel();
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(GiftCardModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var giftCard = model.ToEntity();
                giftCard.CreatedOnUtc = DateTime.UtcNow;
                _giftCardService.InsertGiftCard(giftCard);

                //activity log
                _customerActivityService.InsertActivity("AddNewGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.AddNewGiftCard"), giftCard.GiftCardCouponCode);

                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = giftCard.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                //No gift card found with the specified id
                return RedirectToAction("List");

            var model = giftCard.ToModel();
            Order order = null;
            if(giftCard.PurchasedWithOrderItem!=null)
                order = _orderService.GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id);

            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? order.Id : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(GiftCardModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(model.Id);
            var order = _orderService.GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id);
            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? order.Id : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            if (ModelState.IsValid)
            {
                giftCard = model.ToEntity(giftCard);
                _giftCardService.UpdateGiftCard(giftCard);

                //activity log
                _customerActivityService.InsertActivity("EditGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.EditGiftCard"), giftCard.GiftCardCouponCode);

                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit",  new {id = giftCard.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        
        [HttpPost]
        public ActionResult GenerateCouponCode()
        {
            return Json(new { CouponCode = _giftCardService.GenerateGiftCardCode() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notifyRecipient")]
        public ActionResult NotifyRecipient(GiftCardModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(model.Id);

            model = giftCard.ToModel();
            var order = _orderService.GetOrderByOrderItemId(giftCard.PurchasedWithOrderItem.Id);
            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? order.Id : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            try
            {
                if (!CommonHelper.IsValidEmail(giftCard.RecipientEmail))
                    throw new NopException("Recipient email is not valid");
                if (!CommonHelper.IsValidEmail(giftCard.SenderEmail))
                    throw new NopException("Sender email is not valid");

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
                    languageId = _localizationSettings.DefaultAdminLanguageId;
                }
                int queuedEmailId = _workflowMessageService.SendGiftCardNotification(giftCard, languageId);
                if (queuedEmailId > 0)
                {
                    giftCard.IsRecipientNotified = true;
                    _giftCardService.UpdateGiftCard(giftCard);
                    model.IsRecipientNotified = true;
                }
            }
            catch (Exception exc)
            {
                ErrorNotification(exc, false);
            }

            return View(model);
        }
        
        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                //No gift card found with the specified id
                return RedirectToAction("List");

            _giftCardService.DeleteGiftCard(giftCard);

            //activity log
            _customerActivityService.InsertActivity("DeleteGiftCard", giftCard.Id, _localizationService.GetResource("ActivityLog.DeleteGiftCard"), giftCard.GiftCardCouponCode);

            SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Deleted"));
            return RedirectToAction("List");
        }
        
        //Gif card usage history
        [HttpPost]
        public ActionResult UsageHistoryList(string giftCardId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(giftCardId);
            if (giftCard == null)
                throw new ArgumentException("No gift card found with the specified id");

            var usageHistoryModel = giftCard.GiftCardUsageHistory.OrderByDescending(gcuh => gcuh.CreatedOnUtc)
                .Select(x => new GiftCardModel.GiftCardUsageHistoryModel
                {
                    Id = x.Id,
                    OrderId = x.UsedWithOrderId,
                    UsedValue = _priceFormatter.FormatPrice(x.UsedValue, true, false),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                })
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = usageHistoryModel.PagedForCommand(command),
                Total = usageHistoryModel.Count
            };

            return Json(gridModel);
        }

        #endregion
    }
}
