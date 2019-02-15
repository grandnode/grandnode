using Grand.Core.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IProductReviewViewModelService
    {
        void PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText);
        (IEnumerable<ProductReviewModel> productReviewModels, int totalCount) PrepareProductReviewsModel(ProductReviewListModel model, int pageIndex, int pageSize);
        ProductReview UpdateProductReview(ProductReview productReview, ProductReviewModel model);
        ProductReviewListModel PrepareProductReviewListModel();
        void DeleteProductReview(ProductReview productReview);
        void ApproveSelected(IList<string> selectedIds);
        void DisapproveSelected(IList<string> selectedIds);

    }
}
