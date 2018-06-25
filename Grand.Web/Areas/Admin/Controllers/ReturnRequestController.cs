using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using System.Linq;
using Grand.Core.Data;
using Grand.Services.Catalog;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ReturnRequestController : BaseAdminController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<ReturnRequest> _returnRequest;
        private readonly IReturnRequestService _returnRequestService;
        #endregion Fields

        #region Constructors

        public ReturnRequestController(IOrderService orderService,
            IProductService productService,
            ICustomerService customerService, IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, IWorkContext workContext,
            IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings,
            ICustomerActivityService customerActivityService, IPermissionService permissionService,
            IRepository<ReturnRequest> returnRequest,
            IReturnRequestService returnRequestService)
        {
            this._orderService = orderService;
            this._productService = productService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._returnRequest = returnRequest;
            this._returnRequestService = returnRequestService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual bool PrepareReturnRequestModel(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var order = _orderService.GetOrderById(returnRequest.OrderId);
            var orderItem = order.OrderItems.Where(x=>x.Id == returnRequest.OrderItemId).FirstOrDefault();
            if (orderItem == null)
                return false;
            var product = _productService.GetProductByIdIncludeArch(orderItem.ProductId);
            model.Id = returnRequest.Id;
            model.ProductId = orderItem.ProductId;
            model.ProductName = product.Name;
            model.OrderId = order.Id;
            model.OrderNumber = order.OrderNumber;
            model.ReturnNumber = returnRequest.ReturnNumber;
            model.CustomerId = returnRequest.CustomerId;
            var customer = _customerService.GetCustomerById(returnRequest.CustomerId);
            model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.Quantity = returnRequest.Quantity;
            model.ReturnRequestStatusStr = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc);
            if (!excludeProperties)
            {
                model.ReasonForReturn = returnRequest.ReasonForReturn;
                model.RequestedAction = returnRequest.RequestedAction;
                model.CustomerComments = returnRequest.CustomerComments;
                model.StaffNotes = returnRequest.StaffNotes;
                model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;
            }
            //model is successfully prepared
            return true;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequests = _returnRequestService.SearchReturnRequests("", "", "", null, command.Page - 1, command.PageSize);
            var returnRequestModels = new List<ReturnRequestModel>();
            foreach (var rr in returnRequests)
            {
                var m = new ReturnRequestModel();
                if (PrepareReturnRequestModel(m, rr, false))
                    returnRequestModels.Add(m);
            }
            var gridModel = new DataSourceResult
            {
                Data = returnRequestModels,
                Total = returnRequests.TotalCount,
            };

            return Json(gridModel);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");
            
            var model = new ReturnRequestModel();
            PrepareReturnRequestModel(model, returnRequest, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(ReturnRequestModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                returnRequest.Quantity = model.Quantity;
                returnRequest.ReasonForReturn = model.ReasonForReturn;
                returnRequest.RequestedAction = model.RequestedAction;
                returnRequest.CustomerComments = model.CustomerComments;
                returnRequest.StaffNotes = model.StaffNotes;
                returnRequest.ReturnRequestStatusId = model.ReturnRequestStatusId;
                returnRequest.UpdatedOnUtc = DateTime.UtcNow;

                _returnRequest.Update(returnRequest);
                //_customerService.UpdateCustomer(returnRequest.Customer);

                //activity log
                _customerActivityService.InsertActivity("EditReturnRequest", returnRequest.Id, _localizationService.GetResource("ActivityLog.EditReturnRequest"), returnRequest.Id);

                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = returnRequest.Id}) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            PrepareReturnRequestModel(model, returnRequest, true);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notify-customer")]
        public IActionResult NotifyCustomer(ReturnRequestModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            //var customer = returnRequest.Customer;
            var order = _orderService.GetOrderById(returnRequest.OrderId);
            var orderItem = order.OrderItems.Where(x=>x.Id == returnRequest.OrderItemId).FirstOrDefault();
            int queuedEmailId = _workflowMessageService.SendReturnRequestStatusChangedCustomerNotification(returnRequest, orderItem, _localizationSettings.DefaultAdminLanguageId);
            if (queuedEmailId > 0)
                SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Notified"));
            return RedirectToAction("Edit",  new {id = returnRequest.Id});
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
                return AccessDeniedView();

            var returnRequest = _returnRequestService.GetReturnRequestById(id);
            if (returnRequest == null)
                //No return request found with the specified id
                return RedirectToAction("List");

            _returnRequestService.DeleteReturnRequest(returnRequest);

            //activity log
            _customerActivityService.InsertActivity("DeleteReturnRequest", returnRequest.Id, _localizationService.GetResource("ActivityLog.DeleteReturnRequest"), returnRequest.Id);

            SuccessNotification(_localizationService.GetResource("Admin.ReturnRequests.Deleted"));
            return RedirectToAction("List");
        }

        #endregion
    }
}
