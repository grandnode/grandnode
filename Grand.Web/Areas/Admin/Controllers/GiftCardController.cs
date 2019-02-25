using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.GiftCards)]
    public partial class GiftCardController : BaseAdminController
    {
        #region Fields
        private readonly IGiftCardViewModelService _giftCardViewModelService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors

        public GiftCardController(
            IGiftCardViewModelService giftCardViewModelService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService)
        {
            this._giftCardViewModelService = giftCardViewModelService;
            this._giftCardService = giftCardService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _giftCardViewModelService.PrepareGiftCardListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult GiftCardList(DataSourceRequest command, GiftCardListModel model)
        {
            var giftCards = _giftCardViewModelService.PrepareGiftCardModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = giftCards.giftCardModels.ToList(),
                Total = giftCards.totalCount
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = _giftCardViewModelService.PrepareGiftCardModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(GiftCardModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var giftCard = _giftCardViewModelService.InsertGiftCardModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = giftCard.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model = _giftCardViewModelService.PrepareGiftCardModel(model);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var giftCard = _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                //No gift card found with the specified id
                return RedirectToAction("List");

            var model = _giftCardViewModelService.PrepareGiftCardModel(giftCard);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(GiftCardModel model, bool continueEditing)
        {
            var giftCard = _giftCardService.GetGiftCardById(model.Id);
            if (giftCard == null)
                return RedirectToAction("List");

            _giftCardViewModelService.FillGiftCardModel(giftCard, model);

            if (ModelState.IsValid)
            {
                giftCard = _giftCardViewModelService.UpdateGiftCardModel(giftCard, model);
                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = giftCard.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult GenerateCouponCode()
        {
            return Json(new { CouponCode = _giftCardService.GenerateGiftCardCode() });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notifyRecipient")]
        public IActionResult NotifyRecipient(GiftCardModel model)
        {
            var giftCard = _giftCardService.GetGiftCardById(model.Id);

            if (!CommonHelper.IsValidEmail(giftCard.RecipientEmail))
                ModelState.AddModelError("", "Recipient email is not valid");
            if (!CommonHelper.IsValidEmail(giftCard.SenderEmail))
                ModelState.AddModelError("", "Sender email is not valid");

            try
            {
                if (ModelState.IsValid)
                {
                    _giftCardViewModelService.NotifyRecipient(giftCard, model);
                }
                else
                    ErrorNotification(ModelState);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc, false);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var giftCard = _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                //No gift card found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                _giftCardViewModelService.DeleteGiftCard(giftCard);
                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = giftCard.Id });
        }

        //Gif card usage history
        [HttpPost]
        public IActionResult UsageHistoryList(string giftCardId, DataSourceRequest command)
        {
            var giftCard = _giftCardService.GetGiftCardById(giftCardId);
            if (giftCard == null)
                throw new ArgumentException("No gift card found with the specified id");

            var usageHistoryModel = _giftCardViewModelService.PrepareGiftCardUsageHistoryModels(giftCard, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = usageHistoryModel.giftCardUsageHistoryModels.ToList(),
                Total = usageHistoryModel.totalCount
            };

            return Json(gridModel);
        }

        #endregion
    }
}
