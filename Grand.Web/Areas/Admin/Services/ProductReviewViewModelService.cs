using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Notifications.Catalog;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class ProductReviewViewModelService : IProductReviewViewModelService
    {
        private readonly IProductService _productService;
        private readonly IProductReviewService _productReviewService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IMediator _mediator;

        public ProductReviewViewModelService(
            IProductService productService,
            IProductReviewService productReviewService,
            ICustomerService customerService,
            IStoreService storeService, 
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, 
            IMediator mediator)
        {
            _productService = productService;
            _productReviewService = productReviewService;
            _customerService = customerService;
            _storeService = storeService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _mediator = mediator;
        }

        public virtual async Task PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productReview == null)
                throw new ArgumentNullException("productReview");
            var product = await _productService.GetProductById(productReview.ProductId);
            var customer = await _customerService.GetCustomerById(productReview.CustomerId);
            var store = await _storeService.GetStoreById(productReview.StoreId);
            model.Id = productReview.Id;
            model.StoreName = store != null ? store.Shortcut : "";
            model.ProductId = productReview.ProductId;
            model.ProductName = product.Name;
            model.CustomerId = productReview.CustomerId;
            model.CustomerInfo = customer != null ? customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";
            model.Rating = productReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
            model.Signature = productReview.Signature;
            if (!excludeProperties)
            {
                model.Title = productReview.Title;
                if (formatReviewText)
                {
                    model.ReviewText = Core.Html.HtmlHelper.FormatText(productReview.ReviewText, false, true, false, false, false, false);
                    model.ReplyText = Core.Html.HtmlHelper.FormatText(productReview.ReplyText, false, true, false, false, false, false);
                }
                else
                {
                    model.ReviewText = productReview.ReviewText;
                    model.ReplyText = productReview.ReplyText;
                }
                model.IsApproved = productReview.IsApproved;
            }
        }

        public virtual async Task<ProductReviewListModel> PrepareProductReviewListModel(string storeId)
        {
            var model = new ProductReviewListModel();

            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var stores = (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)).Select(st => new SelectListItem() { Text = st.Shortcut, Value = st.Id.ToString() });
            foreach (var selectListItem in stores)
                model.AvailableStores.Add(selectListItem);
            return model;
        }

        public virtual async Task<(IEnumerable<ProductReviewModel> productReviewModels, int totalCount)> PrepareProductReviewsModel(ProductReviewListModel model, int pageIndex, int pageSize)
        {
            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var productReviews = await _productReviewService.GetAllProductReviews("", null,
                createdOnFromValue, createdToFromValue, model.SearchText, model.SearchStoreId, model.SearchProductId);

            var items = new List<ProductReviewModel>();
            foreach (var x in productReviews.PagedForCommand(pageIndex, pageSize))
            {
                var m = new ProductReviewModel();
                await PrepareProductReviewModel(m, x, false, true);
                items.Add(m);
            }
            return (items, productReviews.Count);
        }

        public virtual async Task<ProductReview> UpdateProductReview(ProductReview productReview, ProductReviewModel model)
        {
            productReview = model.ToEntity(productReview);
            await _productReviewService.UpdateProductReview(productReview);

            //update product totals
            var product = await _productService.GetProductById(productReview.ProductId);

            //update product totals
            await _mediator.Send(new UpdateProductReviewTotalsCommand() { Product = product });

            return productReview;
        }

        public virtual async Task DeleteProductReview(ProductReview productReview)
        {
            await _productReviewService.DeleteProductReview(productReview);

            var product = await _productService.GetProductById(productReview.ProductId);
            //update product totals
            await _mediator.Send(new UpdateProductReviewTotalsCommand() { Product = product });
        }

        public virtual async Task ApproveSelected(IList<string> selectedIds, string storeId)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idProduct = id.Split(':').Last().ToString();
                var product = await _productService.GetProductById(idProduct);
                var productReview = await _productReviewService.GetProductReviewById(idReview);
                if (productReview != null && (string.IsNullOrEmpty(storeId) || productReview.StoreId == storeId))
                {
                    var previousIsApproved = productReview.IsApproved;
                    productReview.IsApproved = true;
                    await _productReviewService.UpdateProductReview(productReview);
                    //update product totals
                    await _mediator.Send(new UpdateProductReviewTotalsCommand() { Product = product });


                    //raise event (only if it wasn't approved before)
                    if (!previousIsApproved)
                        await _mediator.Publish(new ProductReviewApprovedEvent(productReview));
                }
            }
        }

        public virtual async Task DisapproveSelected(IList<string> selectedIds, string storeId)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idProduct = id.Split(':').Last().ToString();

                var product = await _productService.GetProductById(idProduct);
                var productReview = await _productReviewService.GetProductReviewById(idReview);
                if (productReview != null && (string.IsNullOrEmpty(storeId) || productReview.StoreId == storeId))
                {
                    productReview.IsApproved = false;
                    await _productReviewService.UpdateProductReview(productReview);
                    //update product totals
                    await _mediator.Send(new UpdateProductReviewTotalsCommand() { Product = product });
                }
            }
        }
    }
}
