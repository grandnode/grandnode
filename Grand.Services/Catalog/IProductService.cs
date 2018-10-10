using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using System;
using System.Collections.Generic;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IProductService
    {
        #region Products

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        void DeleteProduct(Product product);

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Products</returns>
        IList<Product> GetAllProductsDisplayedOnHomePage();

        /// <summary>
        /// Gets recommended products for customer roles
        /// </summary>
        /// <returns>Products</returns>
        IList<Product> GetRecommendedProducts(string[] customerRoleIds);

        /// <summary>
        /// Gets recommended products for customer roles
        /// </summary>
        /// <returns>Products</returns>
        IList<Product> GetSuggestedProducts(string[] customerTagIds);

        /// <summary>
        /// Gets personalized products for customer 
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>Products</returns>
        IList<Product> GetPersonalizedProducts(string customerId);

        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        Product GetProductById(string productId);

        /// <summary>
        /// Gets product from product or product deleted
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        Product GetProductByIdIncludeArch(string productId);

        /// <summary>
        /// Gets products by identifier
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <returns>Products</returns>
        IList<Product> GetProductsByIds(string[] productIds);

        /// <summary>
        /// Gets products by discount
        /// </summary>
        /// <param name="discountId">Product identifiers</param>
        /// <returns>Products</returns>
        IPagedList<Product> GetProductsByDiscount(string discountId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        void InsertProduct(Product product);

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateProduct(Product product);

        /// <summary>
        /// Updates stock the product
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateStockProduct(Product product);

        /// <summary>
        /// Updates most view on the product
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <param name="qty">Count</param>
        void UpdateMostView(string productId, int qty);

        /// <summary>
        /// Updates best sellers on the product
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <param name="qty">Count</param>
        void UpdateSold(string productId, int qty);

        /// <summary>
        /// Set product as unpublished
        /// </summary>
        /// <param name="productId"></param>
        void UnpublishProduct(string productId);

        /// <summary>
        /// Get (visible) product number in certain category
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <returns>Product number</returns>
        int GetCategoryProductNumber(IList<string> categoryIds = null, string storeId = "");

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; "" to load all records</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; "" to load all records</param>
        /// <param name="productType">Product type; "" to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; "" to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>Products</returns>
        IPagedList<Product> SearchProducts(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<string> categoryIds = null,
            string manufacturerId = "",
            string storeId = "",
            string vendorId = "",
            string warehouseId = "",
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            string productTagId = "",
            string keywords = null,
            bool searchDescriptions = false,
            bool searchSku = true,
            bool searchProductTags = false,
            string languageId = "",
            IList<string> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null);

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; "" to load all records</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; "" to load all records</param>
        /// <param name="productType">Product type; "" to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; "" to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>Products</returns>
        IPagedList<Product> SearchProducts(
            out IList<string> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<string> categoryIds = null,
            string manufacturerId = "",
            string storeId = "",
            string vendorId = "",
            string warehouseId = "",
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool markedAsNewOnly = false,
            bool? featuredProducts = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            string productTagId = "",
            string keywords = null,
            bool searchDescriptions = false,
            bool searchSku = true,
            bool searchProductTags = false, 
            string languageId = "",
            IList<string> filteredSpecs = null, 
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null);

        /// <summary>
        /// Gets products by product attribute
        /// </summary>
        /// <param name="productAttributeId">Product attribute identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Products</returns>
        IPagedList<Product> GetProductsByProductAtributeId(string productAttributeId,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets associated products
        /// </summary>
        /// <param name="parentGroupedProductId">Parent product identifier (used with grouped products)</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        IList<Product> GetAssociatedProducts(string parentGroupedProductId,
            string storeId = "", string vendorId = "", bool showHidden = false);

        /// <summary>
        /// Update product review totals
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateProductReviewTotals(Product product);

        /// <summary>
        /// Update product associated 
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateAssociatedProduct(Product product);
        /// <summary>
        /// Update product review 
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateProductReview(ProductReview productreview);

        /// <summary>
        /// Insert product review 
        /// </summary>
        /// <param name="product">Product</param>
        void InsertProductReview(ProductReview productreview);

        /// <summary>
        /// Get low stock products
        /// </summary>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="products">Low stock products</param>
        /// <param name="combinations">Low stock attribute combinations</param>
        void GetLowStockProducts(string vendorId,
            out IList<Product> products,
            out IList<ProductAttributeCombination> combinations);

        /// <summary>
        /// Gets a product by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product</returns>
        Product GetProductBySku(string sku);

        /// <summary>
        /// Update HasTierPrices property (used for performance optimization)
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateHasTierPricesProperty(string productId);

        /// <summary>
        /// Update Interval properties
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="Interval">Interval</param>
        /// <param name="IntervalUnit">Interval unit</param>
        /// <param name="includeBothDates">Include both dates</param>
        void UpdateIntervalProperties(string productId, int interval, IntervalUnit intervalUnit, bool  includeBothDates);


        #endregion

        #region Inventory management methods

        /// <summary>
        /// Adjust inventory
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantityToChange">Quantity to increase or descrease</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        void AdjustInventory(Product product, int quantityToChange, string attributesXml = "", string warehouseId = "");

        /// <summary>
        /// Reserve the given quantity in the warehouses.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be negative</param>
        void ReserveInventory(Product product, int quantity, string warehouseId);

        /// <summary>
        /// Unblocks the given quantity reserved items in the warehouses
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be positive</param>
        void UnblockReservedInventory(Product product, int quantity, string warehouseId);

        /// <summary>
        /// Book the reserved quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributeXML">AttributeXML</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="quantity">Quantity, must be negative</param>
        void BookReservedInventory(Product product, string AttributeXML, string warehouseId, int quantity);

        /// <summary>
        /// Reverse booked inventory (if acceptable)
        /// </summary>
        /// <param name="product">product</param>
        /// <param name="shipmentItem">Shipment item</param>
        /// <returns>Quantity reversed</returns>
        int ReverseBookedInventory(Product product, ShipmentItem shipmentItem);

        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void DeleteRelatedProduct(RelatedProduct relatedProduct);

        /// <summary>
        /// Inserts a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void InsertRelatedProduct(RelatedProduct relatedProduct);

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void UpdateRelatedProduct(RelatedProduct relatedProduct);

        #endregion

        #region Cross-sell products

        /// <summary>
        /// Deletes a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell</param>
        void DeleteCrossSellProduct(CrossSellProduct crossSellProduct);

        /// <summary>
        /// Inserts a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        void InsertCrossSellProduct(CrossSellProduct crossSellProduct);

        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        /// <returns>Cross-sells</returns>
        IList<Product> GetCrosssellProductsByShoppingCart(IList<ShoppingCartItem> cart, int numberOfProducts);

        #endregion

        #region Bundle products

        /// <summary>
        /// Deletes a bundle product
        /// </summary>
        /// <param name="bundleProduct">Bundle product</param>
        void DeleteBundleProduct(BundleProduct bundleProduct);

        /// <summary>
        /// Inserts a bundle product
        /// </summary>
        /// <param name="bundleProduct">Bundle product</param>
        void InsertBundleProduct(BundleProduct bundleProduct);

        /// <summary>
        /// Updates a bundle product
        /// </summary>
        /// <param name="bundleProduct">Bundle product</param>
        void UpdateBundleProduct(BundleProduct bundleProduct);

        #endregion

        #region Tier prices

        /// <summary>
        /// Deletes a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void DeleteTierPrice(TierPrice tierPrice);

        /// <summary>
        /// Inserts a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void InsertTierPrice(TierPrice tierPrice);

        /// <summary>
        /// Updates the tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void UpdateTierPrice(TierPrice tierPrice);

        #endregion

        #region Product Tags

        /// <summary>
        /// Inserts a product tags
        /// </summary>
        /// <param name="productTag">Product Tag</param>
        void InsertProductTag(ProductTag productTag);

        /// <summary>
        /// Delete a product tags
        /// </summary>
        /// <param name="productTag">Product Tag</param>
        void DeleteProductTag(ProductTag productTag);

        #endregion


        #region Product pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void DeleteProductPicture(ProductPicture productPicture);

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void InsertProductPicture(ProductPicture productPicture);

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void UpdateProductPicture(ProductPicture productPicture);

        #endregion

        #region Product reviews

        /// <summary>
        /// Gets all product reviews
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="productId">Product identifier; "" to load all records</param>
        /// <returns>Reviews</returns>
        IList<ProductReview> GetAllProductReviews(string customerId, bool? approved,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, string storeId = "", string productId = "", int size = 0);

        /// <summary>
        /// Get rating sum for product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier, "" to load all records</param> 
        /// <returns>Sum</returns>
        int RatingSumProduct(string productId, string storeId);

        /// <summary>
        /// Total reviews for product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier, "" to load all records</param> 
        /// <returns>Sum</returns>
        int TotalReviewsProduct(string productId, string storeId);

        /// <summary>
        /// Gets product review
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <returns>Product review</returns>
        ProductReview GetProductReviewById(string productReviewId);

        /// <summary>
        /// Deletes a product review
        /// </summary>
        /// <param name="productReview">Product review</param>
        void DeleteProductReview(ProductReview productReview);

        #endregion

        #region Product warehouse inventory

        /// <summary>
        /// Deletes a ProductWarehouseInventory
        /// </summary>
        /// <param name="pwi">ProductWarehouseInventory</param>
        void DeleteProductWarehouseInventory(ProductWarehouseInventory pwi);


        /// <summary>
        /// Insert a ProductWarehouseInventory
        /// </summary>
        /// <param name="pwi">ProductWarehouseInventory</param>
        void InsertProductWarehouseInventory(ProductWarehouseInventory pwi);

        /// <summary>
        /// Update a ProductWarehouseInventory
        /// </summary>
        /// <param name="pwi">ProductWarehouseInventory</param>
        void UpdateProductWarehouseInventory(ProductWarehouseInventory pwi);

        void InsertDiscount(string discountId, string productId);
        void DeleteDiscount(string discountId, string productId);
        #endregion
    }
}
