using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Catalog;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Interfaces
{
    public partial interface IProductViewModelService
    {
        IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(
            IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false);
        IList<ProductSpecificationModel> PrepareProductSpecificationModel(
            Product product);
        ProductReviewOverviewModel PrepareProductReviewOverviewModel(
           Product product);
        string PrepareProductTemplateViewPath(string productTemplateId);
        ProductDetailsModel PrepareProductDetailsPage(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false);
        void PrepareProductReviewsModel(ProductReviewsModel model, Product product, int size = 0);
        ProductReview InsertProductReview(Product product, ProductReviewsModel model);

        void SendProductEmailAFriendMessage(Product product, ProductEmailAFriendModel model);
        void SendProductAskQuestionMessage(Product product, ProductAskQuestionModel model);
        ProductDetailsAttributeChangeModel PrepareProductDetailsAttributeChangeModel(Product product, bool validateAttributeConditions, bool loadPicture, IFormCollection form);
        ProductAskQuestionModel PrepareProductAskQuestionModel(Product product);
        ProductAskQuestionSimpleModel PrepareProductAskQuestionSimpleModel(Product product);
        IList<ProductOverviewModel> PrepareProductsDisplayedOnHomePage(int? productThumbPictureSize);
        IList<ProductOverviewModel> PrepareProductsHomePageBestSellers(int? productThumbPictureSize);
        IList<ProductOverviewModel> PrepareProductsRecommended(int? productThumbPictureSize);
        IList<ProductOverviewModel> PrepareProductsPersonalized(int? productThumbPictureSize);
        IList<ProductOverviewModel> PrepareProductsSuggested(int? productThumbPictureSize);
        IList<ProductOverviewModel> PrepareProductsCrossSell(int? productThumbPictureSize, int count);
        IList<ProductOverviewModel> PrepareProductsRelated(string productId, int? productThumbPictureSize);
        IList<ProductOverviewModel> PrepareProductsRecentlyViewed(int? productThumbPictureSize, bool? preparePriceModel);
    }
}