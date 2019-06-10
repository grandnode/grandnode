using Grand.Core;
using Grand.Core.Domain.Customers;
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
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ReturnRequests)]
    public partial class ReturnRequestController : BaseAdminController
    {
        #region Fields

        private readonly IReturnRequestViewModelService _returnRequestViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Constructors

        public ReturnRequestController(
            IReturnRequestViewModelService returnRequestViewModelService,
            ILocalizationService localizationService,
            IReturnRequestService returnRequestService,
            IWorkContext workContext)
        {
            _returnRequestViewModelService = returnRequestViewModelService;
            _localizationService = localizationService;
            _returnRequestService = returnRequestService;
            _workContext = workContext;
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
        public async Task<IActionResult> List(DataSourceRequest command, ReturnReqestListModel model)
        {
            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }
            var returnRequestModels = await _returnRequestViewModelService.PrepareReturnRequestModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = returnRequestModels.returnRequestModels,
                Total = returnRequestModels.totalCount,
            };

            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-returnrequest")]
        public async Task<IActionResult> GoToId(ReturnReqestListModel model)
        {
            if (model.GoDirectlyToId == null)
                return RedirectToAction("List", "ReturnRequest");

            int id = int.Parse(model.GoDirectlyToId);

            //try to load a product entity
            var returnRequest = await _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //not found
                return RedirectToAction("List", "ReturnRequest");

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List", "ReturnRequest");
            }

            return RedirectToAction("Edit", "ReturnRequest", new { id = returnRequest.Id });
        }

        [HttpPost]
        public async Task<IActionResult> ProductsForReturnRequest(string returnRequestId, DataSourceRequest command)
        {
            var returnRequest = await _returnRequestService.GetReturnRequestById(returnRequestId);
            if (returnRequest == null)
                return ErrorForKendoGridJson("Return request not found");

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return ErrorForKendoGridJson("Return request is not your");
            }
            var items = await _returnRequestViewModelService.PrepareReturnRequestItemModel(returnRequestId);
            var gridModel = new DataSourceResult {
                Data = items,
                Total = items.Count,
            };

            return Json(gridModel);
        }

        //edit
        public async Task<IActionResult> Edit(string id)
        {
            var returnRequest = await _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List", "ReturnRequest");
            }

            var model = new ReturnRequestModel();
            await _returnRequestViewModelService.PrepareReturnRequestModel(model, returnRequest, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(ReturnRequestModel model, bool continueEditing, IFormCollection form,
            [FromServices] IAddressAttributeService addressAttributeService,
            [FromServices] IAddressAttributeParser addressAttributeParser,
            [FromServices] OrderSettings orderSettings
            )
        {
            var returnRequest = await _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List", "ReturnRequest");
            }

            var customAddressAttributes = string.Empty;
            if (orderSettings.ReturnRequests_AllowToSpecifyPickupAddress)
            {
                customAddressAttributes = await form.ParseCustomAddressAttributes(addressAttributeParser, addressAttributeService);
                var customAddressAttributeWarnings = await addressAttributeParser.GetAttributeWarnings(customAddressAttributes);
                foreach (var error in customAddressAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
            }
            if (ModelState.IsValid)
            {
                returnRequest = await _returnRequestViewModelService.UpdateReturnRequestModel(returnRequest, model, customAddressAttributes);

                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = returnRequest.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            await _returnRequestViewModelService.PrepareReturnRequestModel(model, returnRequest, false);
            return View(model);
        }

        //delete
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var returnRequest = await _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List", "ReturnRequest");
            }

            if (ModelState.IsValid)
            {
                await _returnRequestViewModelService.DeleteReturnRequest(returnRequest);
                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = returnRequest.Id });
        }

        #endregion
    }
}
