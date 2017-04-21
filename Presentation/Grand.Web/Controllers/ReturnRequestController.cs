using System;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Web.Framework.Security;
using Grand.Web.Models.Order;
using Grand.Web.Services;

namespace Grand.Web.Controllers
{
    public partial class ReturnRequestController : BasePublicController
    {
        #region Fields
        private readonly IReturnRequestWebService _returnRequestWebService;

        private readonly IReturnRequestService _returnRequestService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Constructors

        public ReturnRequestController(
            IReturnRequestWebService returnRequestWebService,
            IReturnRequestService returnRequestService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings)
        {
            this._returnRequestWebService = returnRequestWebService;
            this._returnRequestService = returnRequestService;
            this._orderService = orderService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._productService = productService;
        }

        #endregion


        #region Methods

        [GrandHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult CustomerReturnRequests()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return new HttpUnauthorizedResult();

            var model = _returnRequestWebService.PrepareCustomerReturnRequests();

            return View(model);
        }

        [GrandHttpsRequirement(SslRequirement.Yes)]
        public virtual ActionResult ReturnRequest(string orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            var model = new SubmitReturnRequestModel();
            model = _returnRequestWebService.PrepareReturnRequest(model, order);
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")]
        [ValidateInput(false)]
        [PublicAntiForgery]
        public virtual ActionResult ReturnRequestSubmit(string orderId, SubmitReturnRequestModel model, FormCollection form)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            int count = 0;
            foreach (var orderItem in order.OrderItems)
            {
                var product = _productService.GetProductById(orderItem.ProductId);
                if (!product.NotReturnable)
                {
                    int quantity = 0; //parse quantity
                    foreach (string formKey in form.AllKeys)
                        if (formKey.Equals(string.Format("quantity{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out quantity);
                            break;
                        }
                    if (quantity > 0)
                    {
                        var rrr = _returnRequestService.GetReturnRequestReasonById(model.ReturnRequestReasonId);
                        var rra = _returnRequestService.GetReturnRequestActionById(model.ReturnRequestActionId);

                        var rr = new ReturnRequest
                        {
                            StoreId = _storeContext.CurrentStore.Id,
                            OrderId = order.Id,
                            OrderItemId = orderItem.Id,
                            Quantity = quantity,
                            CustomerId = _workContext.CurrentCustomer.Id,
                            ReasonForReturn = rrr != null ? rrr.GetLocalized(x => x.Name) : "not available",
                            RequestedAction = rra != null ? rra.GetLocalized(x => x.Name) : "not available",
                            CustomerComments = model.Comments,
                            StaffNotes = string.Empty,
                            ReturnRequestStatus = ReturnRequestStatus.Pending,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        };

                        _returnRequestService.InsertReturnRequest(rr);
                        //notify store owner here (email)
                        _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, orderItem, _localizationSettings.DefaultAdminLanguageId);
                        //notify customer
                        _workflowMessageService.SendNewReturnRequestCustomerNotification(rr, orderItem, order.CustomerLanguageId);

                        count++;
                    }
                }
            }

            model = _returnRequestWebService.PrepareReturnRequest(model, order);
            if (count > 0)
                model.Result = _localizationService.GetResource("ReturnRequests.Submitted");
            else
                model.Result = _localizationService.GetResource("ReturnRequests.NoItemsSubmitted");

            return View(model);
        }

        #endregion
    }
}
