using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class ProductReviewViewModelService : IProductReviewViewModelService
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IEventPublisher _eventPublisher;

        public ProductReviewViewModelService(IProductService productService, ICustomerService customerService, IStoreService storeService, IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, IEventPublisher eventPublisher)
        {
            _productService = productService;
            _customerService = customerService;
            _storeService = storeService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _eventPublisher = eventPublisher;
        }

        public virtual void PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productReview == null)
                throw new ArgumentNullException("productReview");
            var product = _productService.GetProductById(productReview.ProductId);
            var customer = _customerService.GetCustomerById(productReview.CustomerId);
            var store = _storeService.GetStoreById(productReview.StoreId);
            model.Id = productReview.Id;
            model.StoreName = store != null ? store.Name : "";
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

        public virtual ProductReviewListModel PrepareProductReviewListModel()
        {
            var model = new ProductReviewListModel();
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            var stores = _storeService.GetAllStores().Select(st => new SelectListItem() { Text = st.Name, Value = st.Id.ToString() });
            foreach (var selectListItem in stores)
                model.AvailableStores.Add(selectListItem);
            return model;
        }

        public virtual (IEnumerable<ProductReviewModel> productReviewModels, int totalCount) PrepareProductReviewsModel(ProductReviewListModel model, int pageIndex, int pageSize)
        {
            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var productReviews = _productService.GetAllProductReviews("", null,
                createdOnFromValue, createdToFromValue, model.SearchText, model.SearchStoreId, model.SearchProductId);
            return (productReviews.PagedForCommand(pageIndex, pageSize).Select(x =>
                {
                    var m = new ProductReviewModel();
                    PrepareProductReviewModel(m, x, false, true);
                    return m;
                }), productReviews.Count);
        }

        public virtual ProductReview UpdateProductReview(ProductReview productReview, ProductReviewModel model)
        {

            productReview = model.ToEntity(productReview);
            //productReview.Title = model.Title;
            //productReview.ReviewText = model.ReviewText;
            //productReview.IsApproved = model.IsApproved;
            //productReview.ReplyText= model.ReplyText;
            //productReview.Signature= model.Signature;

            _productService.UpdateProductReview(productReview);

            //update product totals
            var product = _productService.GetProductById(productReview.ProductId);
            _productService.UpdateProductReviewTotals(product);
            return productReview;
        }

        public void DeleteProductReview(ProductReview productReview)
        {
            _productService.DeleteProductReview(productReview);

            var product = _productService.GetProductById(productReview.ProductId);
            //update product totals
            _productService.UpdateProductReviewTotals(product);
        }

        public virtual void ApproveSelected(IList<string> selectedIds)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idProduct = id.Split(':').Last().ToString();
                var product = _productService.GetProductById(idProduct);
                var productReview = _productService.GetProductReviewById(idReview);
                if (productReview != null)
                {
                    var previousIsApproved = productReview.IsApproved;
                    productReview.IsApproved = true;
                    _productService.UpdateProductReview(productReview);
                    _productService.UpdateProductReviewTotals(product);

                    //raise event (only if it wasn't approved before)
                    if (!previousIsApproved)
                        _eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));
                }
            }
        }

        public virtual void DisapproveSelected(IList<string> selectedIds)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idProduct = id.Split(':').Last().ToString();

                var product = _productService.GetProductById(idProduct);
                var productReview = _productService.GetProductReviewById(idReview);
                if (productReview != null)
                {
                    productReview.IsApproved = false;
                    _productService.UpdateProductReview(productReview);
                    //update product totals
                    _productService.UpdateProductReviewTotals(product);
                }
            }
        }
    }
}
