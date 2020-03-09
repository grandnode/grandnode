﻿using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IProductViewModelService
    {
        Task<IEnumerable<ProductOverviewModel>> PrepareProductOverviewModels(
            IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false);
        Task<IList<ProductSpecificationModel>> PrepareProductSpecificationModel(
            Product product);
        Task<ProductReviewOverviewModel> PrepareProductReviewOverviewModel(
           Product product);
        Task<string> PrepareProductTemplateViewPath(string productTemplateId);
        Task<ProductDetailsModel> PrepareProductDetailsPage(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false);

        Task<VendorBriefInfoModel> PrepareVendorBriefInfoModel(Product product);
        Task<ProductDetailsModel.ProductBreadcrumbModel> PrepareProductBreadcrumbModel(Product product);
        Task<List<ProductTagModel>> PrepareProductTagModel(Product product);
        Task<(PictureModel defaultPictureModel, List<PictureModel> pictureModels)> PrepareProductPictureModel(Product product, int defaultPictureSize, bool isAssociatedProduct, string name);
        Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModel(Product product);
        Task<ProductDetailsModel.AddToCartModel> PrepareAddToCartModel(Product product, ShoppingCartItem updatecartitem = null);
        ProductDetailsModel.GiftCardModel PrepareGiftCardModel(Product product, ShoppingCartItem updatecartitem = null);
        Task<IList<ProductDetailsModel.ProductAttributeModel>> PrepareProductAttributeModel(Product product, int defaultPictureSize, ShoppingCartItem updatecartitem = null);
        Task<IList<ProductDetailsModel.TierPriceModel>> PrepareProductTierPriceModel(Product product);
        Task<IList<ProductDetailsModel.ProductBundleModel>> PrepareProductBundleModel(Product product);
        Task PrepareProductReservation(ProductDetailsModel model, Product product);
        Task PrepareProductReviewsModel(ProductReviewsModel model, Product product, int size = 0);
        Task<ProductReview> InsertProductReview(Product product, ProductReviewsModel model);

        Task SendProductEmailAFriendMessage(Product product, ProductEmailAFriendModel model);
        Task SendProductAskQuestionMessage(Product product, ProductAskQuestionModel model);
        Task<ProductDetailsAttributeChangeModel> PrepareProductDetailsAttributeChangeModel(Product product, bool validateAttributeConditions, bool loadPicture, IFormCollection form);
        Task<ProductAskQuestionModel> PrepareProductAskQuestionModel(Product product);
        Task<ProductAskQuestionSimpleModel> PrepareProductAskQuestionSimpleModel(Product product);
        Task<IList<ProductOverviewModel>> PrepareNewProductsDisplayedOnHomePage(int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsDisplayedOnHomePage(int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsHomePageBestSellers(int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsRecommended(int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsPersonalized(int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsSuggested(int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsCrossSell(int? productThumbPictureSize, int count);
        Task<IList<ProductOverviewModel>> PrepareProductsRelated(string productId, int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsSimilar(string productId, int? productThumbPictureSize);
        Task<IList<ProductOverviewModel>> PrepareProductsRecentlyViewed(int? productThumbPictureSize, bool? preparePriceModel);
        Task<IList<ProductOverviewModel>> PrepareIdsProducts(string[] productIds, int? productThumbPictureSize);

    }
}