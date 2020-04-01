using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
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
using System;
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
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Constructors

        public ReturnRequestController(
            IReturnRequestViewModelService returnRequestViewModelService,
            ILocalizationService localizationService,
            IReturnRequestService returnRequestService,
            IOrderService orderService,
            IWorkContext workContext)
        {
            _returnRequestViewModelService = returnRequestViewModelService;
            _localizationService = localizationService;
            _returnRequestService = returnRequestService;
            _orderService = orderService;
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

            //a vendor should have access only to his return request
            if (_workContext.CurrentVendor != null && returnRequest.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "ReturnRequest");

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

            //a vendor should have access only to his return request
            if (_workContext.CurrentVendor != null && returnRequest.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "ReturnRequest");

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

            //a vendor can't delete return request
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("List", "ReturnRequest");

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

        #region Generic attributes

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-generic-attributes")]
        public async Task<IActionResult> EditGenericAttributes(string id, ReturnRequestModel model)
        {
            var returnrequest = await _returnRequestService.GetReturnRequestById(id);
            if (returnrequest == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", new { id = returnrequest.Id });

            if (_workContext.CurrentCustomer.IsStaff() && returnrequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", new { id = returnrequest.Id });
            }

            if (_workContext.CurrentVendor != null && _workContext.CurrentVendor.Id != returnrequest.VendorId)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.VendorAccess"));
                return RedirectToAction("Edit", new { id = returnrequest.Id });
            }
            returnrequest.GenericAttributes = model.GenericAttributes;
            await _returnRequestService.UpdateReturnRequest(returnrequest);

            //selected tab
            await SaveSelectedTabIndex(4);

            return RedirectToAction("Edit", new { id = returnrequest.Id });
        }

        #endregion

        #region Return request notes

        [HttpPost]
        public async Task<IActionResult> ReturnRequestNotesSelect(string returnRequestId, DataSourceRequest command)
        {
            var returnRequest = await _returnRequestService.GetReturnRequestById(returnRequestId);
            if (returnRequest == null)
                throw new ArgumentException("No return request found with the specified id");

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Content("");
            }
            //a vendor should have access only to his return request
            if (_workContext.CurrentVendor != null && returnRequest.VendorId != _workContext.CurrentVendor.Id)
                return Content("");

            //return request notes
            var returnRequestNoteModels = await _returnRequestViewModelService.PrepareReturnRequestNotes(returnRequest);
            var gridModel = new DataSourceResult {
                Data = returnRequestNoteModels,
                Total = returnRequestNoteModels.Count
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> ReturnRequestNoteAdd(string returnRequestId, string orderId, string downloadId, bool displayToCustomer, string message)
        {
            var returnRequest = await _returnRequestService.GetReturnRequestById(returnRequestId);
            if (returnRequest == null)
                return Json(new { Result = false });

            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false });

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Json(new { Result = false });
            }

            //a vendor should have access only to his return request
            if (_workContext.CurrentVendor != null && returnRequest.VendorId != _workContext.CurrentVendor.Id)
                return Json(new { Result = false });

            await _returnRequestViewModelService.InsertReturnRequestNote(returnRequest, order, downloadId, displayToCustomer, message);

            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> ReturnRequestNoteDelete(string id, string returnRequestId)
        {
            var returnRequest = await _returnRequestService.GetReturnRequestById(returnRequestId);
            if (returnRequest == null)
                throw new ArgumentException("No return request found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { Result = false });

            if (_workContext.CurrentCustomer.IsStaff() && returnRequest.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Json(new { Result = false });
            }

            await _returnRequestViewModelService.DeleteReturnRequestNote(returnRequest, id);

            return new NullJsonResult();
        }
        #endregion
    }
}
