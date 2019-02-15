using Grand.Core.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IProductViewModelService
    {
        void PrepareProductModel(ProductModel model, Product product,
            bool setPredefinedValues, bool excludeProperties);
        void PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText);
        void BackInStockNotifications(Product product, ProductModel model, int prevStockQuantity,
            List<ProductWarehouseInventory> prevMultiWarehouseStock
            );
        (IEnumerable<OrderModel> orderModels, int totalCount) PrepareOrderModel(string productId, int pageIndex, int pageSize);
        void PrepareAddProductAttributeCombinationModel(ProductAttributeCombinationModel model, Product product);
        void SaveProductWarehouseInventory(Product product, IList<ProductModel.ProductWarehouseInventoryModel> model);
        void PrepareTierPriceModel(ProductModel.TierPriceModel model);
        void PrepareProductAttributeValueModel(Product product, ProductModel.ProductAttributeValueModel model);
        ProductListModel PrepareProductListModel();
        (IEnumerable<ProductModel> productModels, int totalCount) PrepareProductsModel(ProductListModel model, int pageIndex, int pageSize);
        IList<Product> PrepareProducts(ProductListModel model);
        Product InsertProductModel(ProductModel model);
        Product UpdateProductModel(Product product, ProductModel model);
        void DeleteProduct(Product product);
        void DeleteSelected(IList<string> selectedIds);
        ProductModel.AddRequiredProductModel PrepareAddRequiredProductModel();
        (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddRequiredProductModel model, int pageIndex, int pageSize);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddRelatedProductModel model, int pageIndex, int pageSize);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddBundleProductModel model, int pageIndex, int pageSize);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddCrossSellProductModel model, int pageIndex, int pageSize);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddAssociatedProductModel model, int pageIndex, int pageSize);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model, int pageIndex, int pageSize);
        IList<ProductModel.ProductCategoryModel> PrepareProductCategoryModel(Product product);
        void InsertProductCategoryModel(ProductModel.ProductCategoryModel model);
        void UpdateProductCategoryModel(ProductModel.ProductCategoryModel model);
        void DeleteProductCategory(string id, string productId);
        IList<ProductModel.ProductManufacturerModel> PrepareProductManufacturerModel(Product product);
        void InsertProductManufacturer(ProductModel.ProductManufacturerModel model);
        void UpdateProductManufacturer(ProductModel.ProductManufacturerModel model);
        void DeleteProductManufacturer(string id, string productId);
        void InsertRelatedProductModel(ProductModel.AddRelatedProductModel model);
        void UpdateRelatedProductModel(ProductModel.RelatedProductModel model);
        void DeleteRelatedProductModel(ProductModel.RelatedProductModel model);
        void InsertBundleProductModel(ProductModel.AddBundleProductModel model);
        void UpdateBundleProductModel(ProductModel.BundleProductModel model);
        void DeleteBundleProductModel(ProductModel.BundleProductModel model);
        void InsertCrossSellProductModel(ProductModel.AddCrossSellProductModel model);
        void DeleteCrossSellProduct(string productId, string crossSellProductId);
        void InsertAssociatedProductModel(ProductModel.AddAssociatedProductModel model);
        void DeleteAssociatedProduct(Product product);
        ProductModel.AddRelatedProductModel PrepareRelatedProductModel();
        ProductModel.AddBundleProductModel PrepareBundleProductModel();
        ProductModel.AddCrossSellProductModel PrepareCrossSellProductModel();
        ProductModel.AddAssociatedProductModel PrepareAssociatedProductModel();
        BulkEditListModel PrepareBulkEditListModel();
        (IEnumerable<BulkEditProductModel> bulkEditProductModels, int totalCount) PrepareBulkEditProductModel(BulkEditListModel model, int pageIndex, int pageSize);
        void UpdateBulkEdit(IList<BulkEditProductModel> products);
        void DeleteBulkEdit(IList<BulkEditProductModel> products);
        //tierprices
        IList<ProductModel.TierPriceModel> PrepareTierPriceModel(Product product);
        (IEnumerable<ProductModel.BidModel> bidModels, int totalCount) PrepareBidMode(string productId, int pageIndex, int pageSize);
        (IEnumerable<ProductModel.ActivityLogModel> activityLogModels, int totalCount) PrepareActivityLogModel(string productId, int pageIndex, int pageSize);
        ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(Product product);
        ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model);
        ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(Product product, ProductAttributeMapping productAttributeMapping);
        IList<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModels(Product product);
        void InsertProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model);
        void UpdateProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model);
        ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(ProductAttributeMapping productAttributeMapping);
        void UpdateProductAttributeValidationRulesModel(ProductAttributeMapping productAttributeMapping, ProductModel.ProductAttributeMappingModel model);
        ProductAttributeConditionModel PrepareProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping);
        void UpdateProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping, ProductAttributeConditionModel model, Dictionary<string, string> form);
        IList<ProductModel.ProductAttributeValueModel> PrepareProductAttributeValueModels(Product product, ProductAttributeMapping productAttributeMapping);
        ProductModel.ProductAttributeValueModel PrepareProductAttributeValueModel(ProductAttributeMapping pa, ProductAttributeValue pav);
        void InsertProductAttributeValueModel(ProductModel.ProductAttributeValueModel model);
        void UpdateProductAttributeValueModel(ProductAttributeValue pav, ProductModel.ProductAttributeValueModel model);
        ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel PrepareAssociateProductToAttributeValueModel();
        IList<ProductModel.ProductAttributeCombinationModel> PrepareProductAttributeCombinationModel(Product product);
        ProductAttributeCombinationModel PrepareProductAttributeCombinationModel(Product product, string combinationId);
        //tier prices for combination
        IList<string> InsertOrUpdateProductAttributeCombinationPopup(Product product, ProductAttributeCombinationModel model, Dictionary<string, string> form);
        void GenerateAllAttributeCombinations(Product product);
        IList<ProductModel.ProductAttributeCombinationTierPricesModel> PrepareProductAttributeCombinationTierPricesModel(Product product, string productAttributeCombinationId);
        void InsertProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model);
        void UpdateProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model);
        void DeleteProductAttributeCombinationTierPrices(Product product, ProductAttributeCombination productAttributeCombination, ProductCombinationTierPrices tierPrice);

        //Pictures
        IList<ProductModel.ProductPictureModel> PrepareProductPictureModel(Product product);
        void InsertProductPicture(Product product, string pictureId, int displayOrder,
           string overrideAltAttribute, string overrideTitleAttribute);
        void UpdateProductPicture(ProductModel.ProductPictureModel model);
        void DeleteProductPicture(ProductModel.ProductPictureModel model);

        //Product specification
        IList<ProductSpecificationAttributeModel> PrepareProductSpecificationAttributeModel(Product product);
        void InsertProductSpecificationAttributeModel(ProductModel.AddProductSpecificationAttributeModel model, Product product);
        void UpdateProductSpecificationAttributeModel(Product product, ProductSpecificationAttribute psa, ProductSpecificationAttributeModel model);
        void DeleteProductSpecificationAttribute(Product product, ProductSpecificationAttribute psa);
    }
}
