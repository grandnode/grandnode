using Grand.Core;
using Grand.Domain;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Notifications.Catalog;
using Grand.Services.Queries.Models.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService : IProductService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_SHOWONHOMEPAGE = "Grand.product.showonhomepage";

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string PRODUCTS_CUSTOMER_ROLE_PATTERN = "Grand.product.cr";

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string PRODUCTS_CUSTOMER_TAG_PATTERN = "Grand.product.ct";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        private const string PRODUCTS_CUSTOMER_PERSONAL_PATTERN = "Grand.product.personal";

        #endregion

        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<ProductDeleted> _productDeletedRepository;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public ProductService(ICacheManager cacheManager,
            IRepository<Product> productRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductDeleted> productDeletedRepository,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IWorkContext workContext,
            IMediator mediator,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            CatalogSettings catalogSettings
            )
        {
            _cacheManager = cacheManager;
            _productRepository = productRepository;
            _productReviewRepository = productReviewRepository;
            _productDeletedRepository = productDeletedRepository;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _workContext = workContext;
            _mediator = mediator;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        #region Products

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual async Task DeleteProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //deleted product
            await _productRepository.DeleteAsync(product);

            //insert to deleted products
            var productDeleted = JsonConvert.DeserializeObject<ProductDeleted>(JsonConvert.SerializeObject(product));
            productDeleted.DeletedOnUtc = DateTime.UtcNow;
            await _productDeletedRepository.InsertAsync(productDeleted);

            //cache
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(product);
        }

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Products</returns>
        public virtual async Task<IList<Product>> GetAllProductsDisplayedOnHomePage()
        {
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Published, true);
            filter &= builder.Eq(x => x.ShowOnHomePage, true);
            filter &= builder.Eq(x => x.VisibleIndividually, true);
            var query = _productRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder).ThenBy(x => x.Name);

            var products = await query.ToListAsync();

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();
            return products;
        }

        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="fromDB">get data from db (not from cache)</param>
        /// <returns>Product</returns>
        public virtual async Task<Product> GetProductById(string productId, bool fromDB = false)
        {
            if (string.IsNullOrEmpty(productId))
                return null;

            if (fromDB)
                return await _productRepository.GetByIdAsync(productId);

            var key = string.Format(PRODUCTS_BY_ID_KEY, productId);
            return await _cacheManager.GetAsync(key, () => _productRepository.GetByIdAsync(productId));
        }

        /// <summary>
        /// Gets product for order
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        public virtual async Task<Product> GetProductByIdIncludeArch(string productId)
        {
            if (String.IsNullOrEmpty(productId))
                return null;
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                product = await _productDeletedRepository.GetByIdAsync(productId) as Product;
            return product;
        }


        /// <summary>
        /// Get products by identifiers
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <returns>Products</returns>
        public virtual async Task<IList<Product>> GetProductsByIds(string[] productIds, bool showHidden = false)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<Product>();

            var builder = Builders<Product>.Filter;
            var filter = builder.Where(x => productIds.Contains(x.Id));
            var products = await _productRepository.Collection.Find(filter).ToListAsync();

            //sort by passed identifiers
            var sortedProducts = new List<Product>();
            foreach (string id in productIds)
            {
                var product = products.Find(x => x.Id == id);
                if (product != null && (showHidden || (_aclService.Authorize(product) && _storeMappingService.Authorize(product) && (product.IsAvailable()))))
                    sortedProducts.Add(product);
            }

            return sortedProducts;
        }

        /// <summary>
        /// Gets products by discount
        /// </summary>
        /// <param name="discountId">Product identifiers</param>
        /// <returns>Products</returns>
        public virtual async Task<IPagedList<Product>> GetProductsByDiscount(string discountId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from c in _productRepository.Table
                        where c.AppliedDiscounts.Any(x => x == discountId)
                        select c;

            return await PagedList<Product>.Create(query, pageIndex, pageSize);
        }


        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual async Task InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //insert
            await _productRepository.InsertAsync(product);

            //clear cache
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(product);
        }

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual async Task UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            var oldProduct = await _productRepository.GetByIdAsync(product.Id);
            //update
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            var update = Builders<Product>.Update
                .Set(x => x.AdditionalShippingCharge, product.AdditionalShippingCharge)
                .Set(x => x.AdminComment, product.AdminComment)
                .Set(x => x.AllowAddingOnlyExistingAttributeCombinations, product.AllowAddingOnlyExistingAttributeCombinations)
                .Set(x => x.AllowBackInStockSubscriptions, product.AllowBackInStockSubscriptions)
                .Set(x => x.AllowCustomerReviews, product.AllowCustomerReviews)
                .Set(x => x.AllowedQuantities, product.AllowedQuantities)
                .Set(x => x.ApprovedRatingSum, product.ApprovedRatingSum)
                .Set(x => x.ApprovedTotalReviews, product.ApprovedTotalReviews)
                .Set(x => x.AutomaticallyAddRequiredProducts, product.AutomaticallyAddRequiredProducts)
                .Set(x => x.AvailableEndDateTimeUtc, product.AvailableEndDateTimeUtc)
                .Set(x => x.AvailableForPreOrder, product.AvailableForPreOrder)
                .Set(x => x.AvailableStartDateTimeUtc, product.AvailableStartDateTimeUtc)
                .Set(x => x.BackorderModeId, product.BackorderModeId)
                .Set(x => x.BasepriceAmount, product.BasepriceAmount)
                .Set(x => x.BasepriceBaseAmount, product.BasepriceBaseAmount)
                .Set(x => x.BasepriceBaseUnitId, product.BasepriceBaseUnitId)
                .Set(x => x.BasepriceEnabled, product.BasepriceEnabled)
                .Set(x => x.BasepriceUnitId, product.BasepriceUnitId)
                .Set(x => x.CallForPrice, product.CallForPrice)
                .Set(x => x.CatalogPrice, product.CatalogPrice)
                .Set(x => x.CreatedOnUtc, product.CreatedOnUtc)
                .Set(x => x.CustomerEntersPrice, product.CustomerEntersPrice)
                .Set(x => x.CustomerRoles, product.CustomerRoles)
                .Set(x => x.DeliveryDateId, product.DeliveryDateId)
                .Set(x => x.DisableBuyButton, product.DisableBuyButton)
                .Set(x => x.DisableWishlistButton, product.DisableWishlistButton)
                .Set(x => x.DisplayOrder, product.DisplayOrder)
                .Set(x => x.DisplayOrderCategory, product.DisplayOrderCategory)
                .Set(x => x.DisplayOrderManufacturer, product.DisplayOrderManufacturer)
                .Set(x => x.DisplayStockAvailability, product.DisplayStockAvailability)
                .Set(x => x.DisplayStockQuantity, product.DisplayStockQuantity)
                .Set(x => x.DownloadActivationTypeId, product.DownloadActivationTypeId)
                .Set(x => x.DownloadExpirationDays, product.DownloadExpirationDays)
                .Set(x => x.DownloadId, product.DownloadId)
                .Set(x => x.Flag, product.Flag)
                .Set(x => x.FullDescription, product.FullDescription)
                .Set(x => x.GiftCardTypeId, product.GiftCardTypeId)
                .Set(x => x.Gtin, product.Gtin)
                .Set(x => x.HasSampleDownload, product.HasSampleDownload)
                .Set(x => x.HasUserAgreement, product.HasUserAgreement)
                .Set(x => x.Height, product.Height)
                .Set(x => x.IncBothDate, product.IncBothDate)
                .Set(x => x.IsDownload, product.IsDownload)
                .Set(x => x.IsFreeShipping, product.IsFreeShipping)
                .Set(x => x.IsGiftCard, product.IsGiftCard)
                .Set(x => x.IsRecurring, product.IsRecurring)
                .Set(x => x.IsShipEnabled, product.IsShipEnabled)
                .Set(x => x.IsTaxExempt, product.IsTaxExempt)
                .Set(x => x.IsTele, product.IsTele)
                .Set(x => x.Length, product.Length)
                .Set(x => x.LimitedToStores, product.LimitedToStores)
                .Set(x => x.Locales, product.Locales)
                .Set(x => x.LowStockActivityId, product.LowStockActivityId)
                .Set(x => x.ManageInventoryMethodId, product.ManageInventoryMethodId)
                .Set(x => x.ManufacturerPartNumber, product.ManufacturerPartNumber)
                .Set(x => x.MarkAsNew, product.MarkAsNew)
                .Set(x => x.MarkAsNewStartDateTimeUtc, product.MarkAsNewStartDateTimeUtc)
                .Set(x => x.MarkAsNewEndDateTimeUtc, product.MarkAsNewEndDateTimeUtc)
                .Set(x => x.MaximumCustomerEnteredPrice, product.MaximumCustomerEnteredPrice)
                .Set(x => x.MaxNumberOfDownloads, product.MaxNumberOfDownloads)
                .Set(x => x.MetaDescription, product.MetaDescription)
                .Set(x => x.MetaKeywords, product.MetaKeywords)
                .Set(x => x.MetaTitle, product.MetaTitle)
                .Set(x => x.MinimumCustomerEnteredPrice, product.MinimumCustomerEnteredPrice)
                .Set(x => x.MinStockQuantity, product.MinStockQuantity)
                .Set(x => x.LowStock, ((product.MinStockQuantity > 0 && product.MinStockQuantity >= product.StockQuantity) || product.StockQuantity <= 0))
                .Set(x => x.Name, product.Name)
                .Set(x => x.NotApprovedRatingSum, product.NotApprovedRatingSum)
                .Set(x => x.NotApprovedTotalReviews, product.NotApprovedTotalReviews)
                .Set(x => x.NotifyAdminForQuantityBelow, product.NotifyAdminForQuantityBelow)
                .Set(x => x.NotReturnable, product.NotReturnable)
                .Set(x => x.OnSale, product.OnSale)
                .Set(x => x.OldPrice, product.OldPrice)
                .Set(x => x.OrderMaximumQuantity, product.OrderMaximumQuantity)
                .Set(x => x.OrderMinimumQuantity, product.OrderMinimumQuantity)
                .Set(x => x.OverriddenGiftCardAmount, product.OverriddenGiftCardAmount)
                .Set(x => x.ParentGroupedProductId, product.ParentGroupedProductId)
                .Set(x => x.PreOrderAvailabilityStartDateTimeUtc, product.PreOrderAvailabilityStartDateTimeUtc)
                .Set(x => x.Price, product.Price)
                .Set(x => x.ProductCost, product.ProductCost)
                .Set(x => x.ProductTemplateId, product.ProductTemplateId)
                .Set(x => x.ProductTypeId, product.ProductTypeId)
                .Set(x => x.Published, product.Published)
                .Set(x => x.RecurringCycleLength, product.RecurringCycleLength)
                .Set(x => x.RecurringCyclePeriodId, product.RecurringCyclePeriodId)
                .Set(x => x.RecurringTotalCycles, product.RecurringTotalCycles)
                .Set(x => x.RequiredProductIds, product.RequiredProductIds)
                .Set(x => x.RequireOtherProducts, product.RequireOtherProducts)
                .Set(x => x.SampleDownloadId, product.SampleDownloadId)
                .Set(x => x.SeName, product.SeName)
                .Set(x => x.ShipSeparately, product.ShipSeparately)
                .Set(x => x.ShortDescription, product.ShortDescription)
                .Set(x => x.ShowOnHomePage, product.ShowOnHomePage)
                .Set(x => x.Sku, product.Sku)
                .Set(x => x.StartPrice, product.StartPrice)
                .Set(x => x.StockQuantity, product.StockQuantity)
                .Set(x => x.Stores, product.Stores)
                .Set(x => x.SubjectToAcl, product.SubjectToAcl)
                .Set(x => x.TaxCategoryId, product.TaxCategoryId)
                .Set(x => x.UnitId, product.UnitId)
                .Set(x => x.UnlimitedDownloads, product.UnlimitedDownloads)
                .Set(x => x.UseMultipleWarehouses, product.UseMultipleWarehouses)
                .Set(x => x.UserAgreementText, product.UserAgreementText)
                .Set(x => x.VendorId, product.VendorId)
                .Set(x => x.VisibleIndividually, product.VisibleIndividually)
                .Set(x => x.WarehouseId, product.WarehouseId)
                .Set(x => x.Weight, product.Weight)
                .Set(x => x.Width, product.Width)
                .Set(x => x.GenericAttributes, product.GenericAttributes)
                .CurrentDate("UpdatedOnUtc");

            await _productRepository.Collection.UpdateOneAsync(filter, update);

            if (oldProduct.AdditionalShippingCharge != product.AdditionalShippingCharge ||
                oldProduct.IsFreeShipping != product.IsFreeShipping ||
                oldProduct.IsGiftCard != product.IsGiftCard ||
                oldProduct.IsShipEnabled != product.IsShipEnabled ||
                oldProduct.IsTaxExempt != product.IsTaxExempt ||
                oldProduct.IsRecurring != product.IsRecurring
                )
            {
                
                await _mediator.Publish(new UpdateProductOnCartEvent(product));
            }

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
            await _cacheManager.RemoveByPrefix(PRODUCTS_CUSTOMER_PERSONAL_PATTERN);
            await _cacheManager.RemoveByPrefix(PRODUCTS_CUSTOMER_ROLE_PATTERN);
            await _cacheManager.RemoveByPrefix(PRODUCTS_CUSTOMER_TAG_PATTERN);

            //event notification
            await _mediator.EntityUpdated(product);
        }

        public virtual async Task UpdateStockProduct(Product product, bool mediator = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //update
            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.StockQuantity, product.StockQuantity)
                    .Set(x => x.LowStock, ((product.MinStockQuantity > 0 && product.MinStockQuantity >= product.StockQuantity) || product.StockQuantity < 0))
                    .CurrentDate("UpdatedOnUtc");
            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            if (mediator)
                await _mediator.Publish(new UpdateStockEvent(product));
        }

        public virtual async Task UpdateMostView(string productId, int qty)
        {
            var update = new UpdateDefinitionBuilder<Product>().Inc(x => x.Viewed, qty);
            await _productRepository.Collection.UpdateManyAsync(x => x.Id == productId, update);
        }

        public virtual async Task UpdateSold(string productId, int qty)
        {
            var update = new UpdateDefinitionBuilder<Product>().Inc(x => x.Sold, qty);
            await _productRepository.Collection.UpdateManyAsync(x => x.Id == productId, update);
        }

        public virtual async Task UnpublishProduct(string productId)
        {
            var filter = Builders<Product>.Filter.Eq("Id", productId);
            var update = Builders<Product>.Update
                    .Set(x => x.Published, false)
                    .CurrentDate("UpdatedOnUtc");
            await _productRepository.Collection.UpdateOneAsync(filter, update);
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productId));

        }

        /// <summary>
        /// Get (visible) product number in certain category
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <returns>Product number</returns>
        public virtual int GetCategoryProductNumber(Customer customer, IList<string> categoryIds = null, string storeId = "")
        {
            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(""))
                categoryIds.Remove("");

            var builder = Builders<Product>.Filter;
            var filter = builder.Where(p => p.Published && p.VisibleIndividually);
            ////category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                filter = filter & builder.Where(p => p.ProductCategories.Any(x => categoryIds.Contains(x.CategoryId)));
            }

            if (!_catalogSettings.IgnoreAcl)
            {
                //ACL (access control list)
                var allowedCustomerRolesIds = customer.GetCustomerRoleIds();
                filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
            }

            if (!string.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
            {
                //Store mapping
                var currentStoreId = new List<string> { storeId };
                filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
            }

            return Convert.ToInt32(_productRepository.Collection.Find(filter).CountDocuments());

        }

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
        /// <param name="productTag">Product tag name; "" to load all records</param>
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
        public virtual async Task<(IPagedList<Product> products, IList<string> filterableSpecificationAttributeOptionIds)> SearchProducts(
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
            string productTag = "",
            string keywords = null,
            bool searchDescriptions = false,
            bool searchSku = true,
            bool searchProductTags = false,
            string languageId = "",
            IList<string> filteredSpecs = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {

            var model = await _mediator.Send(new GetSearchProductsQuery() {
                Customer = _workContext.CurrentCustomer,
                LoadFilterableSpecificationAttributeOptionIds = loadFilterableSpecificationAttributeOptionIds,
                PageIndex = pageIndex,
                PageSize = pageSize,
                CategoryIds = categoryIds,
                ManufacturerId = manufacturerId,
                StoreId = storeId,
                VendorId = vendorId,
                WarehouseId = warehouseId,
                ProductType = productType,
                VisibleIndividuallyOnly = visibleIndividuallyOnly,
                MarkedAsNewOnly = markedAsNewOnly,
                FeaturedProducts = featuredProducts,
                PriceMin = priceMin,
                PriceMax = priceMax,
                ProductTag = productTag,
                Keywords = keywords,
                SearchDescriptions = searchDescriptions,
                SearchSku = searchSku,
                SearchProductTags = searchProductTags,
                LanguageId = languageId,
                FilteredSpecs = filteredSpecs,
                OrderBy = orderBy,
                ShowHidden = showHidden,
                OverridePublished = overridePublished
            });

            return model;
        }

        /// <summary>
        /// Gets products by product attribute
        /// </summary>
        /// <param name="productAttributeId">Product attribute identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Products</returns>
        public virtual async Task<IPagedList<Product>> GetProductsByProductAtributeId(string productAttributeId,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _productRepository.Table
                        select p;
            query = query.Where(x => x.ProductAttributeMappings.Any(y => y.ProductAttributeId == productAttributeId));
            query = query.OrderBy(x => x.Name);

            return await PagedList<Product>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets associated products
        /// </summary>
        /// <param name="parentGroupedProductId">Parent product identifier (used with grouped products)</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <returns>Products</returns>
        public virtual async Task<IList<Product>> GetAssociatedProducts(string parentGroupedProductId,
            string storeId = "", string vendorId = "", bool showHidden = false)
        {

            var builder = Builders<Product>.Filter;
            var filter = FilterDefinition<Product>.Empty;

            filter = filter & builder.Where(p => p.ParentGroupedProductId == parentGroupedProductId);

            if (!showHidden)
            {
                filter = filter & builder.Where(p => p.Published);
            }
            if (!showHidden)
            {
                var nowUtc = DateTime.UtcNow;
                //available dates
                filter = filter & builder.Where(p =>
                    (p.AvailableStartDateTimeUtc == null || p.AvailableStartDateTimeUtc < nowUtc) &&
                    (p.AvailableEndDateTimeUtc == null || p.AvailableEndDateTimeUtc > nowUtc));

            }
            //vendor filtering
            if (!String.IsNullOrEmpty(vendorId))
            {
                filter = filter & builder.Where(p => p.VendorId == vendorId);
            }

            var products = await _productRepository.Collection.Find(filter).SortBy(x => x.DisplayOrder).ToListAsync();

            //ACL mapping
            if (!showHidden)
            {
                products = products.Where(x => _aclService.Authorize(x)).ToList();
            }
            //Store mapping
            if (!showHidden && !string.IsNullOrEmpty(storeId))
            {
                products = products.Where(x => _storeMappingService.Authorize(x, storeId)).ToList();
            }

            return products;
        }

        /// <summary>
        /// Get low stock products
        /// </summary>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="storeId">Store identifier; "" to load all records</param>
        /// <param name="products">Low stock products</param>
        /// <param name="combinations">Low stock attribute combinations</param>
        public virtual void GetLowStockProducts(string vendorId, string storeId,
            out IList<Product> products,
            out IList<ProductAttributeCombination> combinations)
        {
            //Track inventory for product
            //simple products
            var query_simple_products = from p in _productRepository.Table
                                        where p.LowStock &&
                                        ((p.ProductTypeId == (int)ProductType.SimpleProduct && p.ManageInventoryMethodId != (int)ManageInventoryMethod.DontManageStock)
                                        ||
                                        (p.ProductTypeId == (int)ProductType.BundledProduct && p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock))
                                        select p;

            if (!string.IsNullOrEmpty(vendorId))
                query_simple_products = query_simple_products.Where(x => x.VendorId == vendorId);

            if (!string.IsNullOrEmpty(storeId))
                query_simple_products = query_simple_products.Where(x => x.Stores.Contains(storeId));

            products = query_simple_products.ToList();

            //Track inventory for product by product attributes
            var query2_1 = from p in _productRepository.Table
                           where
                           p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
                           (vendorId == "" || p.VendorId == vendorId) &&
                           (storeId == "" || p.Stores.Contains(storeId))
                           from c in p.ProductAttributeCombinations
                           select new ProductAttributeCombination() {
                               ProductId = p.Id,
                               StockQuantity = c.StockQuantity,
                               AttributesXml = c.AttributesXml,
                               AllowOutOfStockOrders = c.AllowOutOfStockOrders,
                               Id = c.Id,
                               Gtin = c.Gtin,
                               ManufacturerPartNumber = c.ManufacturerPartNumber,
                               NotifyAdminForQuantityBelow = c.NotifyAdminForQuantityBelow,
                               OverriddenPrice = c.OverriddenPrice,
                               Sku = c.Sku
                           };

            var query2_2 = from c in query2_1
                           where c.StockQuantity <= 0
                           select c;

            combinations = query2_2.ToList();
        }


        /// <summary>
        /// Gets a product by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product</returns>
        public virtual async Task<Product> GetProductBySku(string sku)
        {
            if (String.IsNullOrEmpty(sku))
                return null;

            sku = sku.Trim();
            var filter = Builders<Product>.Filter.Eq(x => x.Sku, sku);
            return await _productRepository.Collection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task UpdateAssociatedProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("productreview");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            var update = Builders<Product>.Update
                .Set(x => x.DisplayOrder, product.DisplayOrder)
                .Set(x => x.ParentGroupedProductId, product.ParentGroupedProductId);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);

        }

        /// <summary>
        /// Update Interval properties
        /// </summary>
        /// <param name="Interval">Interval</param>
        /// <param name="IntervalUnit">Interval unit</param>
        /// <param name="includeBothDates">Include both dates</param>
        public virtual async Task UpdateIntervalProperties(string productId, int interval, IntervalUnit intervalUnit, bool includeBothDates)
        {
            var product = await GetProductById(productId);
            if (product == null)
                throw new ArgumentNullException("product");

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.Interval, interval)
                    .Set(x => x.IntervalUnitId, (int)intervalUnit)
                    .Set(x => x.IncBothDate, includeBothDates);

            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //event notification
            await _mediator.EntityUpdated(product);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

        }

        #endregion

        #region Inventory management methods

        /// <summary>
        /// Adjust inventory
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantityToChange">Quantity to increase or descrease</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        public virtual async Task AdjustInventory(Product product, int quantityToChange, string attributesXml = "", string warehouseId = "")
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantityToChange == 0)
                return;

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                var prevStockQuantity = product.GetTotalStockQuantity(warehouseId: warehouseId);

                //update stock quantity
                if (product.UseMultipleWarehouses)
                {
                    //use multiple warehouses
                    if (quantityToChange < 0)
                        await ReserveInventory(product, quantityToChange, warehouseId);
                    else
                        await UnblockReservedInventory(product, quantityToChange, warehouseId);

                    product.StockQuantity = product.ProductWarehouseInventory.Sum(x => x.StockQuantity);
                    await UpdateStockProduct(product);
                }
                else
                {
                    //do not use multiple warehouses
                    //simple inventory management
                    product.StockQuantity += quantityToChange;
                    await UpdateStockProduct(product);
                }

                //check if minimum quantity is reached
                if (quantityToChange < 0 && product.MinStockQuantity >= product.GetTotalStockQuantity(warehouseId: ""))
                {
                    switch (product.LowStockActivity)
                    {
                        case LowStockActivity.DisableBuyButton:
                            product.DisableBuyButton = true;
                            product.DisableWishlistButton = true;

                            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
                            var update = Builders<Product>.Update
                                    .Set(x => x.DisableBuyButton, product.DisableBuyButton)
                                    .Set(x => x.DisableWishlistButton, product.DisableWishlistButton)
                                    .Set(x => x.LowStock, true)
                                    .CurrentDate("UpdatedOnUtc");
                            await _productRepository.Collection.UpdateOneAsync(filter, update);
                            //cache
                            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

                            //event notification
                            await _mediator.EntityUpdated(product);

                            break;
                        case LowStockActivity.Unpublish:
                            product.Published = false;
                            var filter2 = Builders<Product>.Filter.Eq("Id", product.Id);
                            var update2 = Builders<Product>.Update
                                    .Set(x => x.Published, product.Published)
                                    .CurrentDate("UpdatedOnUtc");
                            await _productRepository.Collection.UpdateOneAsync(filter2, update2);

                            //cache
                            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
                            if (product.ShowOnHomePage)
                                await _cacheManager.RemoveByPrefix(PRODUCTS_SHOWONHOMEPAGE);

                            //event notification
                            await _mediator.EntityUpdated(product);

                            break;
                        default:
                            break;
                    }
                }
                //qty is increased. product is back in stock (minimum stock quantity is reached again)?
                if (_catalogSettings.PublishBackProductWhenCancellingOrders)
                {
                    if (quantityToChange > 0 && prevStockQuantity <= product.MinStockQuantity && product.MinStockQuantity < product.GetTotalStockQuantity(warehouseId: ""))
                    {
                        switch (product.LowStockActivity)
                        {
                            case LowStockActivity.DisableBuyButton:
                                var filter = Builders<Product>.Filter.Eq("Id", product.Id);
                                var update = Builders<Product>.Update
                                        .Set(x => x.DisableBuyButton, product.DisableBuyButton)
                                        .Set(x => x.DisableWishlistButton, product.DisableWishlistButton)
                                        .Set(x => x.LowStock, true)
                                        .CurrentDate("UpdatedOnUtc");
                                await _productRepository.Collection.UpdateOneAsync(filter, update);
                                //cache
                                await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
                                break;
                            case LowStockActivity.Unpublish:
                                product.Published = false;
                                var filter2 = Builders<Product>.Filter.Eq("Id", product.Id);
                                var update2 = Builders<Product>.Update
                                        .Set(x => x.Published, product.Published)
                                        .CurrentDate("UpdatedOnUtc");
                                await _productRepository.Collection.UpdateOneAsync(filter2, update2);

                                //cache
                                await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
                                if (product.ShowOnHomePage)
                                    await _cacheManager.RemoveByPrefix(PRODUCTS_SHOWONHOMEPAGE);

                                break;
                            default:
                                break;
                        }
                    }
                }

                //send email notification
                if (quantityToChange < 0 && product.GetTotalStockQuantity(warehouseId: warehouseId) < product.NotifyAdminForQuantityBelow)
                {
                    await _mediator.Send(new SendQuantityBelowStoreOwnerNotificationCommand() {
                        Product = product
                    });
                }
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);
                if (combination != null)
                {
                    combination.ProductId = product.Id;
                    if (!product.UseMultipleWarehouses)
                    {
                        combination.StockQuantity += quantityToChange;
                        await _productAttributeService.UpdateProductAttributeCombination(combination);
                    }
                    else
                    {
                        if (quantityToChange < 0)
                            await ReserveInventoryCombination(product, combination, quantityToChange, warehouseId);
                        else
                            await UnblockReservedInventoryCombination(product, combination, quantityToChange, warehouseId);
                    }

                    product.StockQuantity += quantityToChange;
                    await UpdateStockProduct(product);

                    //send email notification
                    if (quantityToChange < 0 && combination.StockQuantity < combination.NotifyAdminForQuantityBelow)
                    {
                        await _mediator.Send(new SendQuantityBelowStoreOwnerNotificationCommand() {
                            Product = product,
                            ProductAttributeCombination = combination
                        });
                    }
                }
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts)
            {
                foreach (var item in product.BundleProducts)
                {
                    var p1 = await GetProductById(item.ProductId);
                    if (p1 != null && (p1.ManageInventoryMethod == ManageInventoryMethod.ManageStock || p1.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes))
                    {
                        await AdjustInventory(p1, quantityToChange * item.Quantity, attributesXml, warehouseId);
                    }
                }
            }

            //bundled products
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                {
                    //associated product (bundle)
                    var associatedProduct = await GetProductById(attributeValue.AssociatedProductId);
                    if (associatedProduct != null)
                    {
                        await AdjustInventory(associatedProduct, quantityToChange * attributeValue.Quantity, warehouseId);
                    }
                }
            }

        }

        /// <summary>
        /// Reserve the given quantity in the warehouses.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual async Task ReserveInventory(Product product, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", "quantity");

            var qty = -quantity;

            var productInventory = product.ProductWarehouseInventory
                .OrderByDescending(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            Action pass = () =>
            {
                foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
                {
                    var selectQty = Math.Min(item.StockQuantity - item.ReservedQuantity, qty);
                    if (qty - selectQty < 0)
                        break;

                    item.ReservedQuantity += selectQty;
                    qty -= selectQty;
                    if (qty <= 0)
                        break;
                }
            };

            // 1st pass: Applying reserved
            pass();

            if (qty > 0)
            {
                // 2rd pass: Booking negative stock!
                var pwi = productInventory[0];
                pwi.ReservedQuantity += qty;
            }

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.ProductWarehouseInventory, productInventory)
                    .CurrentDate("UpdatedOnUtc");
            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }


        /// <summary>
        /// Reserve the given quantity in the warehouses.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="combination">Combination</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual async Task ReserveInventoryCombination(Product product, ProductAttributeCombination combination, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (combination == null)
                throw new ArgumentNullException("combination");

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", "quantity");

            var qty = -quantity;

            var productInventory = combination.WarehouseInventory
                .OrderByDescending(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            Action pass = () =>
            {
                foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
                {
                    var selectQty = Math.Min(item.StockQuantity - item.ReservedQuantity, qty);
                    if (qty - selectQty < 0)
                        break;

                    item.ReservedQuantity += selectQty;
                    qty -= selectQty;
                    if (qty <= 0)
                        break;
                }
            };

            // 1st pass: Applying reserved
            pass();

            if (qty > 0)
            {
                // 2rd pass: Booking negative stock!
                var pwi = productInventory[0];
                pwi.ReservedQuantity += qty;
            }
            combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity);
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, combination.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
            var update = Builders<Product>.Update
                .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                .CurrentDate("UpdatedOnUtc");

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }

        /// <summary>
        /// Unblocks the given quantity reserved items in the warehouses
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be positive</param>
        public virtual async Task UnblockReservedInventory(Product product, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity < 0)
                throw new ArgumentException("Value must be positive.", "quantity");

            var productInventory = product.ProductWarehouseInventory
                .OrderByDescending(pwi => pwi.ReservedQuantity)
                .ThenByDescending(pwi => pwi.StockQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            var qty = quantity;

            foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
            {
                var selectQty = Math.Min(item.ReservedQuantity, qty);
                if (qty - selectQty < 0)
                    break;

                item.ReservedQuantity -= selectQty;
                qty -= selectQty;
                if (qty <= 0)
                    break;
            }

            if (qty > 0)
            {
                var pwi = productInventory[0];
                pwi.StockQuantity += qty;

            }

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.ProductWarehouseInventory, productInventory)
                    .CurrentDate("UpdatedOnUtc");
            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }


        /// <summary>
        /// Unblocks the given quantity reserved items in the warehouses
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be positive</param>
        public virtual async Task UnblockReservedInventoryCombination(Product product, ProductAttributeCombination combination, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity < 0)
                throw new ArgumentException("Value must be positive.", "quantity");

            var productInventory = combination.WarehouseInventory
                .OrderByDescending(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            var qty = quantity;

            foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
            {
                var selectQty = Math.Min(item.ReservedQuantity, qty);
                if (qty - selectQty < 0)
                    break;

                item.ReservedQuantity -= selectQty;
                qty -= selectQty;
                if (qty <= 0)
                    break;
            }

            if (qty > 0)
            {
                var pwi = productInventory[0];
                pwi.StockQuantity += qty;
            }

            combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, combination.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
            var update = Builders<Product>.Update
                .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                .CurrentDate("UpdatedOnUtc");

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }

        /// <summary>
        /// Book the reserved quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributeXML">AttributeXML</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual async Task BookReservedInventory(Product product, string AttributeXML, string warehouseId, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", "quantity");

            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock)
                return;
            if (!product.UseMultipleWarehouses && product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByBundleProducts)
                return;

            //standard manage stock 
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                var pwi = product.ProductWarehouseInventory.FirstOrDefault(pi => pi.WarehouseId == warehouseId);
                if (pwi == null)
                    return;

                pwi.ReservedQuantity = Math.Max(pwi.ReservedQuantity + quantity, 0);
                pwi.StockQuantity += quantity;

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, product.Id);
                filter = filter & builder.Where(x => x.ProductWarehouseInventory.Any(y => y.WarehouseId == pwi.WarehouseId));

                var update = Builders<Product>.Update
                        .Set(x => x.ProductWarehouseInventory.ElementAt(-1), pwi)
                        .CurrentDate("UpdatedOnUtc");
                await _productRepository.Collection.UpdateOneAsync(filter, update);

                product.StockQuantity = product.ProductWarehouseInventory.Sum(x => x.StockQuantity);
                await UpdateStockProduct(product);

            }
            //manage stock by attributes
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.AttributesXml == AttributeXML);
                if (combination == null)
                    return;
                combination.ProductId = product.Id;

                var pwi = combination.WarehouseInventory.FirstOrDefault(pi => pi.WarehouseId == warehouseId);
                if (pwi == null)
                    return;

                pwi.ReservedQuantity = Math.Max(pwi.ReservedQuantity + quantity, 0);
                pwi.StockQuantity += quantity;

                combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, combination.ProductId);
                filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
                var update = Builders<Product>.Update
                    .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                    .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                    .CurrentDate("UpdatedOnUtc");

                await _productRepository.Collection.UpdateManyAsync(filter, update);

                product.StockQuantity = product.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                await UpdateStockProduct(product);

            }
            //manage stock by bundle products
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts)
            {
                foreach (var item in product.BundleProducts)
                {
                    var p1 = await GetProductById(item.ProductId);
                    if (p1 != null && p1.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    {
                        await BookReservedInventory(p1, string.Empty, warehouseId, quantity * item.Quantity);
                    }
                }
            }

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);

        }

        /// <summary>
        /// Reverse booked inventory (if acceptable)
        /// </summary>
        /// <param name="product">product</param>
        /// <param name="shipmentItem">Shipment item</param>
        /// <returns>Quantity reversed</returns>
        public virtual async Task<int> ReverseBookedInventory(Product product, Shipment shipment, ShipmentItem shipmentItem)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");

            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock)
                return 0;
            if (!product.UseMultipleWarehouses && product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByBundleProducts)
                return 0;

            var qty = shipmentItem.Quantity;

            //standard manage stock
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                var pwi = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == shipmentItem.WarehouseId);
                if (pwi == null)
                    return 0;

                //not shipped yet? hence "BookReservedInventory" method was not invoked
                if (!shipment.ShippedDateUtc.HasValue)
                    return 0;

                pwi.StockQuantity += qty;
                pwi.ReservedQuantity += qty;

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, product.Id);
                filter = filter & builder.Where(x => x.ProductWarehouseInventory.Any(y => y.WarehouseId == pwi.WarehouseId));

                var update = Builders<Product>.Update
                        .Set(x => x.ProductWarehouseInventory.ElementAt(-1), pwi)
                        .CurrentDate("UpdatedOnUtc");
                await _productRepository.Collection.UpdateOneAsync(filter, update);
            }

            //manage stock by attributes
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {

                var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.AttributesXml == shipmentItem.AttributeXML);
                if (combination == null)
                    return 0;

                combination.ProductId = product.Id;

                var pwi = combination.WarehouseInventory.FirstOrDefault(x => x.WarehouseId == shipmentItem.WarehouseId);
                if (pwi == null)
                    return 0;

                //not shipped yet? hence "BookReservedInventory" method was not invoked
                if (!shipment.ShippedDateUtc.HasValue)
                    return 0;

                pwi.StockQuantity += qty;
                pwi.ReservedQuantity += qty;

                combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, combination.ProductId);
                filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
                var update = Builders<Product>.Update
                    .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                    .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                    .CurrentDate("UpdatedOnUtc");

                await _productRepository.Collection.UpdateManyAsync(filter, update);
            }
            //manage stock by bundle products
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts)
            {
                foreach (var item in product.BundleProducts)
                {
                    var p1 = await GetProductById(item.ProductId);
                    if (p1 != null && p1.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    {
                        shipmentItem.Quantity = shipmentItem.Quantity * item.Quantity;
                        await ReverseBookedInventory(p1, shipment, shipmentItem);
                    }
                }
            }
            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);

            return qty;
        }

        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual async Task DeleteRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.RelatedProducts, relatedProduct);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", relatedProduct.ProductId1), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, relatedProduct.ProductId1));

            //event notification
            await _mediator.EntityDeleted(relatedProduct);
        }


        public virtual async Task InsertRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.RelatedProducts, relatedProduct);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", relatedProduct.ProductId1), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, relatedProduct.ProductId1));

            //event notification
            await _mediator.EntityInserted(relatedProduct);
        }

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual async Task UpdateRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, relatedProduct.ProductId1);
            filter = filter & builder.ElemMatch(x => x.RelatedProducts, y => y.Id == relatedProduct.Id);
            var update = Builders<Product>.Update
                .Set(x => x.RelatedProducts.ElementAt(-1).DisplayOrder, relatedProduct.DisplayOrder);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, relatedProduct.ProductId1));

            //event notification
            await _mediator.EntityUpdated(relatedProduct);
        }

        #endregion

        #region Similar products

        /// <summary>
        /// Deletes a similar product
        /// </summary>
        /// <param name="similarProduct">Similar product</param>
        public virtual async Task DeleteSimilarProduct(SimilarProduct similarProduct)
        {
            if (similarProduct == null)
                throw new ArgumentNullException("similarProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.SimilarProducts, similarProduct);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", similarProduct.ProductId1), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, similarProduct.ProductId1));

            //event notification
            await _mediator.EntityDeleted(similarProduct);
        }


        public virtual async Task InsertSimilarProduct(SimilarProduct similarProduct)
        {
            if (similarProduct == null)
                throw new ArgumentNullException("similarProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.SimilarProducts, similarProduct);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", similarProduct.ProductId1), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, similarProduct.ProductId1));

            //event notification
            await _mediator.EntityInserted(similarProduct);
        }

        /// <summary>
        /// Updates a similar product
        /// </summary>
        /// <param name="similarProduct">Similar product</param>
        public virtual async Task UpdateSimilarProduct(SimilarProduct similarProduct)
        {
            if (similarProduct == null)
                throw new ArgumentNullException("similarProduct");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, similarProduct.ProductId1);
            filter = filter & builder.ElemMatch(x => x.SimilarProducts, y => y.Id == similarProduct.Id);
            var update = Builders<Product>.Update
                .Set(x => x.SimilarProducts.ElementAt(-1).DisplayOrder, similarProduct.DisplayOrder);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, similarProduct.ProductId1));

            //event notification
            await _mediator.EntityUpdated(similarProduct);
        }

        #endregion

        #region Bundle product

        /// <summary>
        /// Deletes a bundle product
        /// </summary>
        /// <param name="bundleProduct">Bundle product</param>
        public virtual async Task DeleteBundleProduct(BundleProduct bundleProduct)
        {
            if (bundleProduct == null)
                throw new ArgumentNullException("bundleProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.BundleProducts, bundleProduct);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", bundleProduct.ProductBundleId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, bundleProduct.ProductBundleId));

            //event notification
            await _mediator.EntityDeleted(bundleProduct);
        }

        /// <summary>
        /// Inserts a bundle product
        /// </summary>
        /// <param name="bundleProduct">Bundle product</param>
        public virtual async Task InsertBundleProduct(BundleProduct bundleProduct)
        {
            if (bundleProduct == null)
                throw new ArgumentNullException("bundleProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.BundleProducts, bundleProduct);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", bundleProduct.ProductBundleId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, bundleProduct.ProductBundleId));

            //event notification
            await _mediator.EntityInserted(bundleProduct);

        }

        /// <summary>
        /// Updates a bundle product
        /// </summary>
        /// <param name="bundleProduct">Bundle product</param>
        public virtual async Task UpdateBundleProduct(BundleProduct bundleProduct)
        {
            if (bundleProduct == null)
                throw new ArgumentNullException("bundleProduct");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, bundleProduct.ProductBundleId);
            filter = filter & builder.ElemMatch(x => x.BundleProducts, y => y.Id == bundleProduct.Id);
            var update = Builders<Product>.Update
                .Set(x => x.BundleProducts.ElementAt(-1).Quantity, bundleProduct.Quantity)
                .Set(x => x.BundleProducts.ElementAt(-1).DisplayOrder, bundleProduct.DisplayOrder);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, bundleProduct.ProductBundleId));

            //event notification
            await _mediator.EntityUpdated(bundleProduct);

        }

        #endregion

        #region Cross-sell products

        /// <summary>
        /// Deletes a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell identifier</param>
        public virtual async Task DeleteCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.CrossSellProduct, crossSellProduct.ProductId2);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", crossSellProduct.ProductId1), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, crossSellProduct.ProductId1));

            //event notification
            await _mediator.EntityDeleted(crossSellProduct);
        }


        /// <summary>
        /// Inserts a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        public virtual async Task InsertCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.CrossSellProduct, crossSellProduct.ProductId2);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", crossSellProduct.ProductId1), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, crossSellProduct.ProductId1));

            //event notification
            await _mediator.EntityInserted(crossSellProduct);
        }


        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        /// <returns>Cross-sells</returns>
        public virtual async Task<IList<Product>> GetCrosssellProductsByShoppingCart(IList<ShoppingCartItem> cart, int numberOfProducts)
        {
            var result = new List<Product>();

            if (numberOfProducts == 0)
                return result;

            if (cart == null || !cart.Any())
                return result;

            var cartProductIds = new List<string>();
            foreach (var sci in cart)
            {
                string prodId = sci.ProductId;
                if (!cartProductIds.Contains(prodId))
                    cartProductIds.Add(prodId);
            }

            foreach (var sci in cart)
            {
                var product = await GetProductById(sci.ProductId);
                if (product == null)
                    continue;

                var crossSells = product.CrossSellProduct;
                foreach (var crossSell in crossSells)
                {
                    //validate that this product is not added to result yet
                    //validate that this product is not in the cart
                    if (result.Find(p => p.Id == crossSell) == null &&
                        !cartProductIds.Contains(crossSell))
                    {
                        var productToAdd = await GetProductById(crossSell);
                        //validate product
                        if (productToAdd == null || !productToAdd.Published)
                            continue;

                        //add a product to result
                        result.Add(productToAdd);
                        if (result.Count >= numberOfProducts)
                            return result;
                    }
                }
            }
            return result;
        }

        #endregion

        #region Tier prices

        /// <summary>
        /// Deletes a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual async Task DeleteTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.TierPrices, tierPrice);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", tierPrice.ProductId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, tierPrice.ProductId));

            //event notification
            await _mediator.EntityDeleted(tierPrice);
        }

        /// <summary>
        /// Inserts a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual async Task InsertTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.TierPrices, tierPrice);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", tierPrice.ProductId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, tierPrice.ProductId));

            //event notification
            await _mediator.EntityInserted(tierPrice);
        }

        /// <summary>
        /// Updates the tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual async Task UpdateTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");


            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, tierPrice.ProductId);
            filter = filter & builder.Where(x => x.TierPrices.Any(y => y.Id == tierPrice.Id));

            var update = Builders<Product>.Update
                .Set(x => x.TierPrices.ElementAt(-1).Price, tierPrice.Price)
                .Set(x => x.TierPrices.ElementAt(-1).Quantity, tierPrice.Quantity)
                .Set(x => x.TierPrices.ElementAt(-1).StoreId, tierPrice.StoreId)
                .Set(x => x.TierPrices.ElementAt(-1).CustomerRoleId, tierPrice.CustomerRoleId)
                .Set(x => x.TierPrices.ElementAt(-1).StartDateTimeUtc, tierPrice.StartDateTimeUtc)
                .Set(x => x.TierPrices.ElementAt(-1).EndDateTimeUtc, tierPrice.EndDateTimeUtc);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, tierPrice.ProductId));

            //event notification
            await _mediator.EntityUpdated(tierPrice);
        }

        #endregion

        #region Product pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual async Task DeleteProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductPictures, productPicture);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productPicture.ProductId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productPicture.ProductId));

            //event notification
            await _mediator.EntityDeleted(productPicture);
        }

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual async Task InsertProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductPictures, productPicture);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productPicture.ProductId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productPicture.ProductId));

            //event notification
            await _mediator.EntityInserted(productPicture);
        }

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual async Task UpdateProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productPicture.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductPictures, y => y.Id == productPicture.Id);
            var update = Builders<Product>.Update
                .Set(x => x.ProductPictures.ElementAt(-1).DisplayOrder, productPicture.DisplayOrder)
                .Set(x => x.ProductPictures.ElementAt(-1).MimeType, productPicture.MimeType)
                .Set(x => x.ProductPictures.ElementAt(-1).SeoFilename, productPicture.SeoFilename)
                .Set(x => x.ProductPictures.ElementAt(-1).AltAttribute, productPicture.AltAttribute)
                .Set(x => x.ProductPictures.ElementAt(-1).TitleAttribute, productPicture.TitleAttribute);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productPicture.ProductId));

            //event notification
            await _mediator.EntityUpdated(productPicture);
        }

        #endregion

        

        #region Product warehouse inventory

        /// <summary>
        /// Deletes a ProductWarehouseInventory
        /// </summary>
        /// <param name="pwi">ProductWarehouseInventory</param>
        public virtual async Task DeleteProductWarehouseInventory(ProductWarehouseInventory pwi)
        {
            if (pwi == null)
                throw new ArgumentNullException("pwi");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductWarehouseInventory, pwi);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", pwi.ProductId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, pwi.ProductId));

        }

        public virtual async Task InsertProductWarehouseInventory(ProductWarehouseInventory pwi)
        {
            if (pwi == null)
                throw new ArgumentNullException("productWarehouse");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductWarehouseInventory, pwi);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", pwi.ProductId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, pwi.ProductId));

            //event notification
            await _mediator.EntityInserted(pwi);
        }

        public virtual async Task UpdateProductWarehouseInventory(ProductWarehouseInventory pwi)
        {
            if (pwi == null)
                throw new ArgumentNullException("productWarehouseInventory");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, pwi.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductWarehouseInventory, y => y.Id == pwi.Id);
            var update = Builders<Product>.Update
                .Set(x => x.ProductWarehouseInventory.ElementAt(-1).StockQuantity, pwi.StockQuantity)
                .Set(x => x.ProductWarehouseInventory.ElementAt(-1).ReservedQuantity, pwi.ReservedQuantity);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, pwi.ProductId));
            //event notification
            await _mediator.EntityUpdated(pwi);
        }
        #endregion

        #region Discount

        public virtual async Task DeleteDiscount(string discountId, string productId)
        {
            if (string.IsNullOrEmpty(discountId))
                throw new ArgumentNullException("discount");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.AppliedDiscounts, discountId);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productId));
        }

        public virtual async Task InsertDiscount(string discountId, string productId)
        {
            if (string.IsNullOrEmpty(discountId))
                throw new ArgumentNullException("discount");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.AppliedDiscounts, discountId);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productId));
        }

        #endregion

        #endregion
    }
}
