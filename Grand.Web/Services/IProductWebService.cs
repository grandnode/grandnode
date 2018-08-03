using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Catalog;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial interface IProductWebService
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
        void PrepareProductReviewsModel(ProductReviewsModel model, Product product);
        ProductReview InsertProductReview(Product product, ProductReviewsModel model);

        void SendProductEmailAFriendMessage(Product product, ProductEmailAFriendModel model);
        void SendProductAskQuestionMessage(Product product, ProductAskQuestionModel model);
        ProductDetailsAttributeChangeModel PrepareProductDetailsAttributeChangeModel(Product product, bool validateAttributeConditions, bool loadPicture, IFormCollection form);
    }
}