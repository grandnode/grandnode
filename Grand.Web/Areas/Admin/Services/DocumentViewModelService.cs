using Grand.Core.Domain.Documents;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Documents;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Documents;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public class DocumentViewModelService : IDocumentViewModelService
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentTypeService _documentTypeService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;

        public DocumentViewModelService(IDocumentService documentService, IDocumentTypeService documentTypeService, ICustomerService customerService,
            IOrderService orderService, ILocalizationService localizationService, IProductService productService)
        {
            _documentService = documentService;
            _documentTypeService = documentTypeService;
            _customerService = customerService;
            _orderService = orderService;
            _localizationService = localizationService;
            _productService = productService;
        }

        public virtual async Task<(IEnumerable<DocumentModel> documetListModel, int totalCount)> PrepareDocumentListModel(DocumentListModel model, int pageIndex, int pageSize)
        {
            var documents = await _documentService.GetAll(customerId: "", name: model.SearchName, number: model.SearchNumber,
                email: model.SearchEmail, status: model.StatusId, pageIndex: pageIndex - 1, pageSize: pageSize);

            var documentListModel = new List<DocumentModel>();
            foreach (var x in documents)
            {
                var docModel = x.ToModel();
                documentListModel.Add(docModel);
            }
            return (documentListModel, documents.TotalCount);
        }

        public virtual async Task<DocumentModel> PrepareDocumentModel(DocumentModel documentModel, Document document, SimpleDocumentModel simpleModel)
        {
            var model = documentModel == null ? new DocumentModel() { Published = true } : documentModel;
            if (document != null)
                model = document.ToModel();
            else
            {
                if (simpleModel != null)
                {
                    model.CustomerId = simpleModel.CustomerId;
                    if (!string.IsNullOrEmpty(simpleModel.OrderId))
                    {
                        model.ObjectId = simpleModel.OrderId;
                        model.ReferenceId = (int)Reference.Order;
                        var order = await _orderService.GetOrderById(simpleModel.OrderId);
                        if (order != null)
                        {
                            model.Number = order.OrderNumber.ToString();
                            model.TotalAmount = order.OrderTotal;
                            model.OutstandAmount = order.PaymentStatus == Core.Domain.Payments.PaymentStatus.Paid ? 0 : order.OrderTotal;
                            model.CurrencyCode = order.CustomerCurrencyCode;
                            model.Name = string.Format(_localizationService.GetResource("Order.Document"), model.Number);
                            model.DocDate = order.CreatedOnUtc;
                            model.DueDate = order.CreatedOnUtc;
                            model.Quantity = 1;
                            model.Username = $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}";
                        }
                    }
                    if (!string.IsNullOrEmpty(model.CustomerId))
                        model.CustomerEmail = (await _customerService.GetCustomerById(simpleModel.CustomerId))?.Email;

                    if (!string.IsNullOrEmpty(simpleModel.ProductId))
                    {
                        model.ObjectId = simpleModel.ProductId;
                        model.ReferenceId = (int)Reference.Product;
                        var product = await _productService.GetProductById(simpleModel.ProductId);
                        if (product != null)
                        {
                            model.Name = product.Name;
                            model.Number = product.Sku;
                            model.Quantity = 1;
                        }
                    }

                }
            }
            var types = await _documentTypeService.GetAll();
            foreach (var item in types)
            {
                model.AvailableDocumentTypes.Add(new SelectListItem { Text = item.Name, Value = item.Id });
            }
            return model;
        }


    }
}
