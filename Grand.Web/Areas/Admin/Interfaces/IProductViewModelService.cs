using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IProductViewModelService
    {
        Task PrepareProductModel(ProductModel model, Product product, bool setPredefinedValues, bool excludeProperties);
        Task PrepareProductReviewModel(ProductReviewModel model, ProductReview productReview, bool excludeProperties, bool formatReviewText);
        Task BackInStockNotifications(Product product, ProductModel model, int prevStockQuantity, List<ProductWarehouseInventory> prevMultiWarehouseStock);
        Task BackInStockNotifications(ProductAttributeCombination combination);
        Task<(IEnumerable<OrderModel> orderModels, int totalCount)> PrepareOrderModel(string productId, int pageIndex, int pageSize);
        Task PrepareAddProductAttributeCombinationModel(ProductAttributeCombinationModel model, Product product);
        Task SaveProductWarehouseInventory(Product product, IList<ProductModel.ProductWarehouseInventoryModel> model);
        Task PrepareTierPriceModel(ProductModel.TierPriceModel model);
        Task PrepareProductAttributeValueModel(Product product, ProductModel.ProductAttributeValueModel model);
        Task<ProductListModel> PrepareProductListModel();
        Task<(IEnumerable<ProductModel> productModels, int totalCount)> PrepareProductsModel(ProductListModel model, int pageIndex, int pageSize);
        Task<IList<Product>> PrepareProducts(ProductListModel model);
        Task<Product> InsertProductModel(ProductModel model);
        Task<Product> UpdateProductModel(Product product, ProductModel model);
        Task DeleteProduct(Product product);
        Task DeleteSelected(IList<string> selectedIds);
        Task<ProductModel.AddRequiredProductModel> PrepareAddRequiredProductModel();
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddRequiredProductModel model, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddRelatedProductModel model, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddSimilarProductModel model, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddBundleProductModel model, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddCrossSellProductModel model, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddAssociatedProductModel model, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model, int pageIndex, int pageSize);
        Task<IList<ProductModel.ProductCategoryModel>> PrepareProductCategoryModel(Product product);
        Task InsertProductCategoryModel(ProductModel.ProductCategoryModel model);
        Task UpdateProductCategoryModel(ProductModel.ProductCategoryModel model);
        Task DeleteProductCategory(string id, string productId);
        Task<IList<ProductModel.ProductManufacturerModel>> PrepareProductManufacturerModel(Product product);
        Task InsertProductManufacturer(ProductModel.ProductManufacturerModel model);
        Task UpdateProductManufacturer(ProductModel.ProductManufacturerModel model);
        Task DeleteProductManufacturer(string id, string productId);
        Task InsertRelatedProductModel(ProductModel.AddRelatedProductModel model);
        Task UpdateRelatedProductModel(ProductModel.RelatedProductModel model);
        Task DeleteRelatedProductModel(ProductModel.RelatedProductModel model);
        Task InsertSimilarProductModel(ProductModel.AddSimilarProductModel model);
        Task UpdateSimilarProductModel(ProductModel.SimilarProductModel model);
        Task DeleteSimilarProductModel(ProductModel.SimilarProductModel model);
        Task InsertBundleProductModel(ProductModel.AddBundleProductModel model);
        Task UpdateBundleProductModel(ProductModel.BundleProductModel model);
        Task DeleteBundleProductModel(ProductModel.BundleProductModel model);
        Task InsertCrossSellProductModel(ProductModel.AddCrossSellProductModel model);
        Task DeleteCrossSellProduct(string productId, string crossSellProductId);
        Task InsertAssociatedProductModel(ProductModel.AddAssociatedProductModel model);
        Task DeleteAssociatedProduct(Product product);
        Task<ProductModel.AddRelatedProductModel> PrepareRelatedProductModel();
        Task<ProductModel.AddSimilarProductModel> PrepareSimilarProductModel();
        Task<ProductModel.AddBundleProductModel> PrepareBundleProductModel();
        Task<ProductModel.AddCrossSellProductModel> PrepareCrossSellProductModel();
        Task<ProductModel.AddAssociatedProductModel> PrepareAssociatedProductModel();
        Task<BulkEditListModel> PrepareBulkEditListModel();
        Task<(IEnumerable<BulkEditProductModel> bulkEditProductModels, int totalCount)> PrepareBulkEditProductModel(BulkEditListModel model, int pageIndex, int pageSize);
        Task UpdateBulkEdit(IList<BulkEditProductModel> products);
        Task DeleteBulkEdit(IList<BulkEditProductModel> products);
        //tierprices
        Task<IList<ProductModel.TierPriceModel>> PrepareTierPriceModel(Product product);
        Task<(IEnumerable<ProductModel.BidModel> bidModels, int totalCount)> PrepareBidMode(string productId, int pageIndex, int pageSize);
        Task<(IEnumerable<ProductModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareActivityLogModel(string productId, int pageIndex, int pageSize);
        Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(Product product);
        Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model);
        Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(Product product, ProductAttributeMapping productAttributeMapping);
        Task<IList<ProductModel.ProductAttributeMappingModel>> PrepareProductAttributeMappingModels(Product product);
        Task InsertProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model);
        Task UpdateProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model);
        Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(ProductAttributeMapping productAttributeMapping);
        Task UpdateProductAttributeValidationRulesModel(ProductAttributeMapping productAttributeMapping, ProductModel.ProductAttributeMappingModel model);
        Task<ProductAttributeConditionModel> PrepareProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping);
        Task UpdateProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping, ProductAttributeConditionModel model, Dictionary<string, string> form);
        Task<IList<ProductModel.ProductAttributeValueModel>> PrepareProductAttributeValueModels(Product product, ProductAttributeMapping productAttributeMapping);
        Task<ProductModel.ProductAttributeValueModel> PrepareProductAttributeValueModel(ProductAttributeMapping pa, ProductAttributeValue pav);
        Task InsertProductAttributeValueModel(ProductModel.ProductAttributeValueModel model);
        Task UpdateProductAttributeValueModel(ProductAttributeValue pav, ProductModel.ProductAttributeValueModel model);
        Task<ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel> PrepareAssociateProductToAttributeValueModel();
        Task<IList<ProductModel.ProductAttributeCombinationModel>> PrepareProductAttributeCombinationModel(Product product);
        Task<ProductAttributeCombinationModel> PrepareProductAttributeCombinationModel(Product product, string combinationId);
        //tier prices for combination
        Task<IList<string>> InsertOrUpdateProductAttributeCombinationPopup(Product product, ProductAttributeCombinationModel model, Dictionary<string, string> form);
        Task GenerateAllAttributeCombinations(Product product);
        Task<IList<ProductModel.ProductAttributeCombinationTierPricesModel>> PrepareProductAttributeCombinationTierPricesModel(Product product, string productAttributeCombinationId);
        Task InsertProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model);
        Task UpdateProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model);
        Task DeleteProductAttributeCombinationTierPrices(Product product, ProductAttributeCombination productAttributeCombination, ProductCombinationTierPrices tierPrice);

        //Pictures
        Task<IList<ProductModel.ProductPictureModel>> PrepareProductPictureModel(Product product);
        Task InsertProductPicture(Product product, string pictureId, int displayOrder, string overrideAltAttribute, string overrideTitleAttribute);
        Task UpdateProductPicture(ProductModel.ProductPictureModel model);
        Task DeleteProductPicture(ProductModel.ProductPictureModel model);

        //Product specification
        Task<IList<ProductSpecificationAttributeModel>> PrepareProductSpecificationAttributeModel(Product product);
        Task InsertProductSpecificationAttributeModel(ProductModel.AddProductSpecificationAttributeModel model, Product product);
        Task UpdateProductSpecificationAttributeModel(Product product, ProductSpecificationAttribute psa, ProductSpecificationAttributeModel model);
        Task DeleteProductSpecificationAttribute(Product product, ProductSpecificationAttribute psa);
    }
}
