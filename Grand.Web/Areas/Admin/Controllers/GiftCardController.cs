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
using System.Threading.Tasks;

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
            _giftCardViewModelService = giftCardViewModelService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
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

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> GiftCardList(DataSourceRequest command, GiftCardListModel model)
        {
            var (giftCardModels, totalCount) = await _giftCardViewModelService.PrepareGiftCardModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = giftCardModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = await _giftCardViewModelService.PrepareGiftCardModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(GiftCardModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var giftCard = await _giftCardViewModelService.InsertGiftCardModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = giftCard.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model = await _giftCardViewModelService.PrepareGiftCardModel(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var giftCard = await _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                //No gift card found with the specified id
                return RedirectToAction("List");

            var model = await _giftCardViewModelService.PrepareGiftCardModel(giftCard);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(GiftCardModel model, bool continueEditing)
        {
            var giftCard = await _giftCardService.GetGiftCardById(model.Id);
            if (giftCard == null)
                return RedirectToAction("List");

            await _giftCardViewModelService.FillGiftCardModel(giftCard, model);

            if (ModelState.IsValid)
            {
                giftCard = await _giftCardViewModelService.UpdateGiftCardModel(giftCard, model);
                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = giftCard.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public IActionResult GenerateCouponCode()
        {
            return Json(new { CouponCode = _giftCardService.GenerateGiftCardCode() });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notifyRecipient")]
        public async Task<IActionResult> NotifyRecipient(GiftCardModel model)
        {
            var giftCard = await _giftCardService.GetGiftCardById(model.Id);

            if (!CommonHelper.IsValidEmail(giftCard.RecipientEmail))
                ModelState.AddModelError("", "Recipient email is not valid");
            if (!CommonHelper.IsValidEmail(giftCard.SenderEmail))
                ModelState.AddModelError("", "Sender email is not valid");

            try
            {
                if (ModelState.IsValid)
                {
                    await _giftCardViewModelService.NotifyRecipient(giftCard, model);
                }
                else
                    ErrorNotification(ModelState);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc, false);
            }
            model = await _giftCardViewModelService.PrepareGiftCardModel(giftCard);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var giftCard = await _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                //No gift card found with the specified id
                return RedirectToAction("List");

            if (giftCard.GiftCardUsageHistory.Any())
                ModelState.AddModelError("", _localizationService.GetResource("Admin.GiftCards.PreventDeleted"));

            if (ModelState.IsValid)
            {
                await _giftCardViewModelService.DeleteGiftCard(giftCard);
                SuccessNotification(_localizationService.GetResource("Admin.GiftCards.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = giftCard.Id });
        }

        //Gif card usage history

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> UsageHistoryList(string giftCardId, DataSourceRequest command)
        {
            var giftCard = await _giftCardService.GetGiftCardById(giftCardId);
            if (giftCard == null)
                throw new ArgumentException("No gift card found with the specified id");

            var (giftCardUsageHistoryModels, totalCount) = await _giftCardViewModelService.PrepareGiftCardUsageHistoryModels(giftCard, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = giftCardUsageHistoryModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        #endregion
    }
}
