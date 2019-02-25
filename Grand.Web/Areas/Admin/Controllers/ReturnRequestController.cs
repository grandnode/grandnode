using Grand.Core.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ReturnRequests)]
    public partial class ReturnRequestController : BaseAdminController
    {
        #region Fields
        private readonly IReturnRequestViewModelService _returnRequestViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IReturnRequestService _returnRequestService;

        #endregion Fields

        #region Constructors

        public ReturnRequestController(
            IReturnRequestViewModelService returnRequestViewModelService,
            ILocalizationService localizationService,
            IReturnRequestService returnRequestService)
        {
            this._returnRequestViewModelService = returnRequestViewModelService;
            this._localizationService = localizationService;
            this._returnRequestService = returnRequestService;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _returnRequestViewModelService.PrepareReturnReqestListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, ReturnReqestListModel model)
        {
            var returnRequestModels = _returnRequestViewModelService.PrepareReturnRequestModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = returnRequestModels.returnRequestModels,
                Total = returnRequestModels.totalCount,
            };

            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-returnrequest")]
        public IActionResult GoToId(ReturnReqestListModel model)
        {
            int id = int.Parse(model.GoDirectlyToId);

            //try to load a product entity
            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest != null)
                return RedirectToAction("Edit", "ReturnRequest", new { id = returnRequest.Id });

            //not found
            return RedirectToAction("List", "ReturnRequest");
        }

        [HttpPost]
        public IActionResult ProductsForReturnRequest(string returnRequestId, DataSourceRequest command)
        {
            var items = _returnRequestViewModelService.PrepareReturnRequestItemModel(returnRequestId);
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = items.Count,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id)
        {
            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            var model = new ReturnRequestModel();
            _returnRequestViewModelService.PrepareReturnRequestModel(model, returnRequest, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(ReturnRequestModel model, bool continueEditing, IFormCollection form,
            [FromServices] IAddressAttributeService addressAttributeService,
            [FromServices] IAddressAttributeParser addressAttributeParser,
            [FromServices] OrderSettings orderSettings
            )
        {
            var returnRequest = _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            var customAddressAttributes = string.Empty;
            if (orderSettings.ReturnRequests_AllowToSpecifyPickupAddress)
            {
                customAddressAttributes = form.ParseCustomAddressAttributes(addressAttributeParser, addressAttributeService);
                var customAddressAttributeWarnings = addressAttributeParser.GetAttributeWarnings(customAddressAttributes);
                foreach (var error in customAddressAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
            }
            if (ModelState.IsValid)
            {
                returnRequest = _returnRequestViewModelService.UpdateReturnRequestModel(returnRequest, model, customAddressAttributes);

                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = returnRequest.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _returnRequestViewModelService.PrepareReturnRequestModel(model, returnRequest, false);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                _returnRequestViewModelService.DeleteReturnRequest(returnRequest);
                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = returnRequest.Id });
        }

        #endregion
    }
}
