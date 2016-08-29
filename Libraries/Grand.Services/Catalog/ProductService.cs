using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Shipping;
using Grand.Services.Customers;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Core.Infrastructure;
using Grand.Services.Shipping;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Seo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        private const string PRODUCTS_BY_ID_KEY = "Nop.product.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Nop.product.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_SHOWONHOMEPAGE = "Nop.product.showonhomepage";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_CUSTOMER_ROLE = "Nop.product.cr-{0}";

        #endregion

        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;
        private readonly IRepository<ProductDeleted> _productDeletedRepository;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILanguageService _languageService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDataProvider _dataProvider;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CommonSettings _commonSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="productPictureRepository">Product picture repository</param>
        /// <param name="productAttributeService">Product attribute service</param>
        /// <param name="productAttributeParser">Product attribute parser service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="dbContext">Database Context</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="productTagRepository">Product Tag repository</param>
        public ProductService(ICacheManager cacheManager,
            IRepository<Product> productRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<AclRecord> aclRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerRoleProduct> customerRoleProductRepository,
            IRepository<ProductDeleted> productDeletedRepository,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ILanguageService languageService,
            IWorkflowMessageService workflowMessageService,
            IDataProvider dataProvider, 
            IWorkContext workContext,
            IStoreContext storeContext,
            LocalizationSettings localizationSettings, 
            CommonSettings commonSettings,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IRepository<ProductTag> productTagRepository
            )
        {
            this._cacheManager = cacheManager;
            this._productRepository = productRepository;
            this._productReviewRepository = productReviewRepository;
            this._aclRepository = aclRepository;
            this._urlRecordRepository = urlRecordRepository;
            this._customerRepository = customerRepository;
            this._customerRoleProductRepository = customerRoleProductRepository;
            this._productDeletedRepository = productDeletedRepository;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._languageService = languageService;
            this._workflowMessageService = workflowMessageService;
            this._dataProvider = dataProvider;
            this._workContext = workContext;
            this._storeContext= storeContext;
            this._localizationSettings = localizationSettings;
            this._commonSettings = commonSettings;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._productTagRepository = productTagRepository;
        }

        #endregion
        
        #region Methods

        #region Products

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void DeleteProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //delete from shopping cart
            var builder = Builders<Customer>.Update;
            var updatefilter = builder.PullFilter(x => x.ShoppingCartItems, y => y.ProductId == product.Id);
            var result = _customerRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            //delete related product
            var builderRelated = Builders<Product>.Update;
            var updatefilterRelated = builderRelated.PullFilter(x => x.RelatedProducts, y => y.ProductId2 == product.Id);
            var resultRelated = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilterRelated).Result;
            
            //delete cross sales product
            var builderCross = Builders<Product>.Update;
            var updatefilterCross = builderCross.Pull(x => x.CrossSellProduct, product.Id);
            var resultCross = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilterCross).Result;

            //delete customer role product
            var filtersCrp = Builders<CustomerRoleProduct>.Filter;
            var filterCrp = filtersCrp.Eq(x => x.ProductId, product.Id);            
            _customerRoleProductRepository.Collection.DeleteManyAsync(filterCrp);

            //delete review
            var filtersProductReview = Builders<ProductReview>.Filter;
            var filterProdReview = filtersProductReview.Eq(x => x.ProductId, product.Id);
            _productReviewRepository.Collection.DeleteManyAsync(filterProdReview);

            //delete url
            var filters = Builders<UrlRecord>.Filter;
            var filter = filters.Eq(x => x.EntityId, product.Id);
            filter = filter & filters.Eq(x=>x.EntityName, "Product");
            _urlRecordRepository.Collection.DeleteManyAsync(filter);

            //insert to deleted products
            var productDeleted = JsonConvert.DeserializeObject<ProductDeleted>(JsonConvert.SerializeObject(product));
            productDeleted.DeletedOnUtc = DateTime.UtcNow;
            _productDeletedRepository.Insert(productDeleted);

            //deleted product
            _productRepository.Delete(product);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

        }

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Products</returns>
        public virtual IList<Product> GetAllProductsDisplayedOnHomePage()
        {
            return _cacheManager.Get(PRODUCTS_SHOWONHOMEPAGE, () =>
                {
                    var query = from p in _productRepository.Table
                                orderby p.DisplayOrder, p.Name
                                where p.Published &&
                                p.ShowOnHomePage
                                select p;
                    var products = query.ToList();
                    return products;
                }
            );
        }

        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        public virtual Product GetProductById(string productId)
        {
            if (String.IsNullOrEmpty(productId))
                return null;            
            string key = string.Format(PRODUCTS_BY_ID_KEY, productId);
            return _cacheManager.Get(key, () => _productRepository.GetById(productId));
        }

        /// <summary>
        /// Gets product for order
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        public virtual Product GetProductByIdIncludeArch(string productId)
        {
            if (String.IsNullOrEmpty(productId))
                return null;
            var product = _productRepository.GetById(productId);
            if (product == null)
                product = _productDeletedRepository.GetById(productId) as Product;
            return product;
        }


        /// <summary>
        /// Get products by identifiers
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <returns>Products</returns>
        public virtual IList<Product> GetProductsByIds(string[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<Product>();

            var builder = Builders<Product>.Filter;
            var filter = builder.Where(x => productIds.Contains(x.Id));
            var products = _productRepository.Collection.Find(filter).ToListAsync().Result;

            //sort by passed identifiers
            var sortedProducts = new List<Product>();
            foreach (string id in productIds)
            {
                var product = products.Find(x => x.Id == id);
                if (product != null)
                    sortedProducts.Add(product);
            }
            return sortedProducts;
        }

        /// <summary>
        /// Gets products by discount
        /// </summary>
        /// <param name="discountId">Product identifiers</param>
        /// <returns>Products</returns>
        public virtual IList<Product> GetProductsByDiscount(string discountId)
        {
            var query = from c in _productRepository.Table
                        where c.AppliedDiscounts.Any(x=>x.Id == discountId)
                        select c;

            var products = query.ToList();
            return products;

        }


        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //insert
            _productRepository.Insert(product);

            //clear cache
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            
            //event notification
            _eventPublisher.EntityInserted(product);
        }

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            var oldProduct = _productRepository.GetById(product.Id);
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
                .Set(x => x.FullDescription, product.FullDescription)
                .Set(x => x.GiftCardTypeId, product.GiftCardTypeId)
                .Set(x => x.Gtin, product.Gtin)
                .Set(x => x.HasDiscountsApplied, product.HasDiscountsApplied)
                .Set(x => x.HasSampleDownload, product.HasSampleDownload)
                .Set(x => x.HasTierPrices, product.HasTierPrices)
                .Set(x => x.HasUserAgreement, product.HasUserAgreement)
                .Set(x => x.Height, product.Height)
                .Set(x => x.IsDownload, product.IsDownload)
                .Set(x => x.IsFreeShipping, product.IsFreeShipping)
                .Set(x => x.IsGiftCard, product.IsGiftCard)
                .Set(x => x.IsRecurring, product.IsRecurring)
                .Set(x => x.IsRental, product.IsRental)
                .Set(x => x.IsShipEnabled, product.IsShipEnabled)
                .Set(x => x.IsTaxExempt, product.IsTaxExempt)
                .Set(x => x.IsTelecommunicationsOrBroadcastingOrElectronicServices, product.IsTelecommunicationsOrBroadcastingOrElectronicServices)
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
                .Set(x => x.RentalPriceLength, product.RentalPriceLength)
                .Set(x => x.RentalPricePeriodId, product.RentalPricePeriodId)
                .Set(x => x.RequiredProductIds, product.RequiredProductIds)
                .Set(x => x.RequireOtherProducts, product.RequireOtherProducts)
                .Set(x => x.SampleDownloadId, product.SampleDownloadId)
                .Set(x => x.SeName, product.SeName)
                .Set(x => x.ShipSeparately, product.ShipSeparately)
                .Set(x => x.ShortDescription, product.ShortDescription)
                .Set(x => x.ShowOnHomePage, product.ShowOnHomePage)
                .Set(x => x.Sku, product.Sku)
                .Set(x => x.SpecialPrice, product.SpecialPrice)
                .Set(x => x.SpecialPriceEndDateTimeUtc, product.SpecialPriceEndDateTimeUtc)
                .Set(x => x.SpecialPriceStartDateTimeUtc, product.SpecialPriceStartDateTimeUtc)
                .Set(x => x.StockQuantity, product.StockQuantity)
                .Set(x => x.Stores, product.Stores)
                .Set(x => x.SubjectToAcl, product.SubjectToAcl)
                .Set(x => x.TaxCategoryId, product.TaxCategoryId)
                .Set(x => x.UnitId, product.UnitId)
                .Set(x => x.UnlimitedDownloads, product.UnlimitedDownloads)
                .Set(x => x.UpdatedOnUtc, product.UpdatedOnUtc)
                .Set(x => x.UseMultipleWarehouses, product.UseMultipleWarehouses)
                .Set(x => x.UserAgreementText, product.UserAgreementText)
                .Set(x => x.VendorId, product.VendorId)
                .Set(x => x.VisibleIndividually, product.VisibleIndividually)
                .Set(x => x.WarehouseId, product.WarehouseId)
                .Set(x => x.Weight, product.Weight)
                .Set(x => x.Width, product.Width);

            var result = _productRepository.Collection.UpdateOneAsync(filter, update).Result;

            if(oldProduct.AdditionalShippingCharge!=product.AdditionalShippingCharge ||
                oldProduct.IsFreeShipping!=product.IsFreeShipping ||
                oldProduct.IsGiftCard != product.IsGiftCard ||
                oldProduct.IsShipEnabled != product.IsShipEnabled ||
                oldProduct.IsTaxExempt != product.IsTaxExempt
                )
            {

                var builderCustomer = Builders<Customer>.Filter;
                var filterCustomer = builderCustomer.ElemMatch(x => x.ShoppingCartItems, y => y.ProductId == product.Id);
                _customerRepository.Collection.Find(filterCustomer).ForEachAsync((cs) =>
                {
                    foreach (var item in cs.ShoppingCartItems.Where(x => x.ProductId == product.Id))
                    {
                        var updateCustomer = Builders<Customer>.Update
                            .Set(x => x.ShoppingCartItems.ElementAt(-1).AdditionalShippingChargeProduct, product.AdditionalShippingCharge)
                            .Set(x => x.ShoppingCartItems.ElementAt(-1).IsFreeShipping, product.IsFreeShipping)
                            .Set(x => x.ShoppingCartItems.ElementAt(-1).IsGiftCard, product.IsGiftCard)
                            .Set(x => x.ShoppingCartItems.ElementAt(-1).IsShipEnabled, product.IsShipEnabled)
                            .Set(x => x.ShoppingCartItems.ElementAt(-1).IsTaxExempt, product.IsTaxExempt);

                        var _builderCustomer = Builders<Customer>.Filter;
                        var _filterCustomer = _builderCustomer.ElemMatch(x => x.ShoppingCartItems, y => y.Id == item.Id);
                        var resultcustomer = _customerRepository.Collection.UpdateManyAsync(_filterCustomer, updateCustomer).Result;
                    }
                }
                );

            }

            //cache
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
                        
            //event notification
            _eventPublisher.EntityUpdated(product);
        }

        public virtual void UpdateStockProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //update
            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x=>x.StockQuantity, product.StockQuantity)
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            _eventPublisher.EntityUpdated(product);
        }

        public virtual void UpdateMostView(string productId, int qty)
        {
            Task.Run(() =>
            {
                var update = new UpdateDefinitionBuilder<Product>().Inc(x => x.Viewed, qty);
                var result = _productRepository.Collection.UpdateManyAsync(x => x.Id == productId, update).Result;
            });
        }

        public virtual void UpdateSold(string productId, int qty)
        {
            Task.Run(() =>
            {
                var update = new UpdateDefinitionBuilder<Product>().Inc(x => x.Sold, qty);
                var result = _productRepository.Collection.UpdateManyAsync(x => x.Id == productId, update).Result;
            });
        }

        /// <summary>
        /// Get (visible) product number in certain category
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>Product number</returns>
        public virtual int GetCategoryProductNumber(IList<string> categoryIds = null, string storeId = "")
        {
            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(""))
                categoryIds.Remove("");


            var builder = Builders<Product>.Filter;
            var filter = builder.Where(p => p.Published && p.VisibleIndividually);
            ////category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                filter = filter & builder.Where(p => p.ProductCategories.Any(x=> categoryIds.Contains(x.CategoryId)));
            }

            if (!_catalogSettings.IgnoreAcl)
            {
                //ACL (access control list)
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
            }

            if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
            {
                //Store mapping
                var currentStoreId = new List<string> { _storeContext.CurrentStore.Id };
                filter = filter & (builder.AnyIn(x => x.Stores, currentStoreId) | builder.Where(x => !x.LimitedToStores));
            }

            return Convert.ToInt32(_productRepository.Collection.Find(filter).CountAsync().Result);

        }

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
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
        public virtual IPagedList<Product> SearchProducts(
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
            bool? overridePublished = null)
        {
            IList<string> filterableSpecificationAttributeOptionIds;
            return SearchProducts(out filterableSpecificationAttributeOptionIds, false,
                pageIndex, pageSize, categoryIds, manufacturerId,
                storeId, vendorId, warehouseId,
                productType, visibleIndividuallyOnly, markedAsNewOnly, featuredProducts,
                priceMin, priceMax, productTagId, keywords, searchDescriptions, searchSku,
                searchProductTags, languageId, filteredSpecs,
                orderBy, showHidden, overridePublished);
        }

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
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
        public virtual IPagedList<Product> SearchProducts(
            out IList<string> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,  //Int32.MaxValue
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
            bool? overridePublished = null)
        {
            filterableSpecificationAttributeOptionIds = new List<string>();
            featuredProducts = featuredProducts.HasValue ? featuredProducts : false;
            //search by keyword
            bool searchLocalizedValue = false;
            if (!String.IsNullOrEmpty(languageId))
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages().Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

            //validate "categoryIds" parameter
            if (categoryIds !=null && categoryIds.Contains(""))
                categoryIds.Remove("");

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
            //stored procedures aren't supported. Use LINQ

            #region Search products

            //products
            var builder = Builders<Product>.Filter;
            var filter = FilterDefinition<Product>.Empty;

            //category filtering
            if (categoryIds != null && categoryIds.Any())
            {

                if (featuredProducts.Value)
                {
                    string categoryId = categoryIds[0];
                    filter = filter & builder.Where(x => x.ProductCategories.Any(y => y.CategoryId == categoryId && y.IsFeaturedProduct));
                }
                else
                {
                    filter = filter & builder.Where(x => x.ProductCategories.Any(y => categoryIds.Contains(y.CategoryId)));
                }
            }
            //manufacturer filtering
            if (!String.IsNullOrEmpty(manufacturerId))
            {
                if (featuredProducts.Value)
                {
                    filter = filter & builder.Where(x => x.ProductManufacturers.Any(y => y.ManufacturerId == manufacturerId && y.IsFeaturedProduct));
                }
                else
                {
                    filter = filter & builder.Where(x => x.ProductManufacturers.Any(y => y.ManufacturerId == manufacturerId));
                }

            }

            if (!overridePublished.HasValue)
            {
                //process according to "showHidden"
                if (!showHidden)
                {
                    filter = filter & builder.Where(p => p.Published);
                }
            }
            else if (overridePublished.Value)
            {
                //published only
                filter = filter & builder.Where(p => p.Published);
            }
            else if (!overridePublished.Value)
            {
                //unpublished only
                filter = filter & builder.Where(p => !p.Published);
            }
            if (visibleIndividuallyOnly)
            {
                filter = filter & builder.Where(p => p.VisibleIndividually);
            }
            if (productType.HasValue)
            {
                var productTypeId = (int)productType.Value;
                filter = filter & builder.Where(p => p.ProductTypeId == productTypeId);
            }

            //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
            //That's why we pass the date value
            var nowUtc = DateTime.UtcNow;
            if (priceMin.HasValue)
            {

                filter = filter & builder.Where(p =>
                    //special price (specified price and valid date range)
                    ((p.SpecialPrice != null &&
                      ((p.SpecialPriceStartDateTimeUtc == null ||
                        p.SpecialPriceStartDateTimeUtc < nowUtc) &&
                       (p.SpecialPriceEndDateTimeUtc == null ||
                        p.SpecialPriceEndDateTimeUtc > nowUtc))) &&
                     (p.SpecialPrice >= priceMin.Value))
                    ||
                    //regular price (price isn't specified or date range isn't valid)
                    ((p.SpecialPrice == null ||
                      ((p.SpecialPriceStartDateTimeUtc != null &&
                        p.SpecialPriceStartDateTimeUtc > nowUtc) ||
                       (p.SpecialPriceEndDateTimeUtc != null &&
                        p.SpecialPriceEndDateTimeUtc < nowUtc))) &&
                     (p.Price >= priceMin.Value)));
                    
            }
            if (priceMax.HasValue)
            {

                    //max price
                    
                    filter = filter & builder.Where(p =>
                                    //special price (specified price and valid date range)
                                    ((p.SpecialPrice != null &&
                                      ((p.SpecialPriceStartDateTimeUtc == null ||
                                        p.SpecialPriceStartDateTimeUtc < nowUtc) &&
                                       (p.SpecialPriceEndDateTimeUtc == null ||
                                        p.SpecialPriceEndDateTimeUtc > nowUtc))) &&
                                     (p.SpecialPrice <= priceMax.Value))
                                    ||
                                    //regular price (price isn't specified or date range isn't valid)
                                    ((p.SpecialPrice == null ||
                                      ((p.SpecialPriceStartDateTimeUtc != null &&
                                        p.SpecialPriceStartDateTimeUtc > nowUtc) ||
                                       (p.SpecialPriceEndDateTimeUtc != null &&
                                        p.SpecialPriceEndDateTimeUtc < nowUtc))) &&
                                     (p.Price <= priceMax.Value)));
                    
            }
            if (!showHidden && !_catalogSettings.IgnoreFilterableAvailableStartEndDateTime)
            {
                filter = filter & builder.Where(p =>
                    (p.AvailableStartDateTimeUtc == null || p.AvailableStartDateTimeUtc < nowUtc) &&
                    (p.AvailableEndDateTimeUtc == null || p.AvailableEndDateTimeUtc > nowUtc));


            }

            if (markedAsNewOnly)
            {
                filter = filter & builder.Where(p => p.MarkAsNew);
                filter = filter & builder.Where(p =>
                    (!p.MarkAsNewStartDateTimeUtc.HasValue || p.MarkAsNewStartDateTimeUtc.Value < nowUtc) &&
                    (!p.MarkAsNewEndDateTimeUtc.HasValue || p.MarkAsNewEndDateTimeUtc.Value > nowUtc));
            }

            //searching by keyword
            if (!String.IsNullOrWhiteSpace(keywords))
            {
                if(_commonSettings.UseFullTextSearch)
                {
                    keywords = "\"" + keywords + "\"";
                    keywords = keywords.Replace("+", "\" \"");
                    keywords = keywords.Replace(" ", "\" \"");
                    filter = filter & builder.Text(keywords);
                }
                else
                {
                    if (!searchDescriptions)
                        filter = filter & builder.Where(p =>
                            p.Name.ToLower().Contains(keywords.ToLower())
                            ||
                            p.Locales.Any(x => x.LocaleKey == "Name" && x.LocaleValue != null && x.LocaleValue.ToLower().Contains(keywords.ToLower()))
                            );
                    else
                    {
                        filter = filter & builder.Where(p =>
                                (p.Name != null && p.Name.ToLower().Contains(keywords.ToLower()))
                                ||
                                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(keywords.ToLower()))
                                ||
                                (p.FullDescription != null && p.FullDescription.ToLower().Contains(keywords.ToLower()))
                                ||
                                (p.Locales.Any(x => x.LocaleValue != null && x.LocaleValue.ToLower().Contains(keywords.ToLower()))));
                    }
                }
                
            }

            if (!showHidden && !_catalogSettings.IgnoreAcl)
            {
                filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
            }

            if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
            {
                filter = filter & builder.Where(x => x.Stores.Any(y=>y == storeId) || !x.LimitedToStores);

            }

            //search by specs
            if (filteredSpecs != null && filteredSpecs.Any())
            {
                    foreach(var spec in filteredSpecs)
                    {
                        string _specId = spec.Split(':').FirstOrDefault();
                        string _specOptionId = spec.Split(':').Last();

                        filter = filter & builder.Where(x => x.ProductSpecificationAttributes.Any(y=>y.SpecificationAttributeId == _specId && y.SpecificationAttributeOptionId == _specOptionId));
                    }
            }

            //vendor filtering
            if (!String.IsNullOrEmpty(vendorId))
            {
                filter = filter & builder.Where(x => x.VendorId == vendorId);
            }

            //warehouse filtering
            if (!String.IsNullOrEmpty(warehouseId))
            {
                filter = filter & (builder.Where(x => x.UseMultipleWarehouses && x.ProductWarehouseInventory.Any(y=>y.WarehouseId == warehouseId)) |
                    builder.Where(x => !x.UseMultipleWarehouses && x.WarehouseId == warehouseId));

            }

            //tag filtering
            if (!String.IsNullOrEmpty(productTagId))
            {
                filter = filter & builder.Where(x => x.ProductTags.Any(y => y.Id == productTagId));
            }


            var builderSort = Builders<Product>.Sort.Descending(x=>x.CreatedOnUtc);

            if (orderBy == ProductSortingEnum.Position && categoryIds != null && categoryIds.Any())
            {
                //category position
                builderSort = Builders<Product>.Sort.Ascending(x => x.DisplayOrderCategory);
            }
            else if (orderBy == ProductSortingEnum.Position && !String.IsNullOrEmpty(manufacturerId))
            {
                //manufacturer position
                builderSort = Builders<Product>.Sort.Ascending(x => x.DisplayOrderManufacturer);
            }
            else if (orderBy == ProductSortingEnum.Position)
            {
                //otherwise sort by name
                builderSort = Builders<Product>.Sort.Ascending(x => x.Name);
            }
            else if (orderBy == ProductSortingEnum.NameAsc)
            {
                //Name: A to Z
                builderSort = Builders<Product>.Sort.Ascending(x => x.Name);
            }
            else if (orderBy == ProductSortingEnum.NameDesc)
            {
                //Name: Z to A
                builderSort = Builders<Product>.Sort.Descending(x => x.Name);
            }
            else if (orderBy == ProductSortingEnum.PriceAsc)
            {
                //Price: Low to High
                builderSort = Builders<Product>.Sort.Ascending(x => x.Price);
            }
            else if (orderBy == ProductSortingEnum.PriceDesc)
            {
                //Price: High to Low
                builderSort = Builders<Product>.Sort.Descending(x => x.Price);
            }
            else if (orderBy == ProductSortingEnum.CreatedOn)
            {
                //creation date
                builderSort = Builders<Product>.Sort.Ascending(x => x.CreatedOnUtc);
            }
            else if (orderBy == ProductSortingEnum.OnSale)
            {
                //on sale
                builderSort = Builders<Product>.Sort.Descending(x => x.OnSale);
            }
            else if (orderBy == ProductSortingEnum.MostViewed)
            {
                //most viewed
                builderSort = Builders<Product>.Sort.Descending(x => x.Viewed);
            }
            else if (orderBy == ProductSortingEnum.BestSellers)
            {
                //best seller
                builderSort = Builders<Product>.Sort.Descending(x => x.Sold);
            }
            else
            {
                //actually this code is not reachable
                builderSort = Builders<Product>.Sort.Ascending(x => x.Id);
            }

           var products = new PagedList<Product>();
           Task taskProduct = Task.Run(() =>
           {
               products = new PagedList<Product>(_productRepository.Collection, filter, builderSort, pageIndex, pageSize);

           });

            if (loadFilterableSpecificationAttributeOptionIds && !_catalogSettings.IgnoreFilterableSpecAttributeOption)
            {
                IList<string> specyfication = new List<string>();
                Task t = Task.Run(() =>
                {
                    var filterSpec = filter &
                        builder.Where(x => x.ProductSpecificationAttributes.Any());

                    var taskCount = _productRepository.Collection.CountAsync(filterSpec);
                    taskCount.Wait();
                    if ((int)taskCount.Result > 0)
                    {
                        var qspec = _productRepository.Collection
                        .Aggregate()
                        .Match(filterSpec)
                        .Unwind(x => x.ProductSpecificationAttributes)
                        .Project(new BsonDocument
                         {
                         {"AllowFiltering", "$ProductSpecificationAttributes.AllowFiltering"},
                         {"SpecificationAttributeId", "$ProductSpecificationAttributes.SpecificationAttributeId"},
                         {"SpecificationAttributeOptionId", "$ProductSpecificationAttributes.SpecificationAttributeOptionId"}
                         })
                        .Match(new BsonDocument("AllowFiltering", true))
                        .Group(new BsonDocument
                                {
                                            {"_id",
                                                new BsonDocument {
                                                    { "SpecificationAttributeId", "$SpecificationAttributeId" },
                                                    { "SpecificationAttributeOptionId", "$SpecificationAttributeOptionId" },
                                                }
                                            },
                                            {"count", new BsonDocument
                                                {
                                                    { "$sum" , 1}
                                                }
                                            }
                                })
                        .ToListAsync().Result;
                        foreach (var item in qspec)
                        {
                            var so = item["_id"]["SpecificationAttributeId"].ToString() + ":" +
                                item["_id"]["SpecificationAttributeOptionId"].ToString();
                            specyfication.Add(so);
                        }


                    }
                    
                });
                t.Wait();
                filterableSpecificationAttributeOptionIds = specyfication;
            }

            taskProduct.Wait();

            //return products
            return products;

            #endregion
            
        }

        /// <summary>
        /// Gets products by product attribute
        /// </summary>
        /// <param name="productAttributeId">Product attribute identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Products</returns>
        public virtual IPagedList<Product> GetProductsByProductAtributeId(string productAttributeId,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _productRepository.Table
                        select p;
            query = query.Where(x => x.ProductAttributeMappings.Any(y => y.ProductAttributeId == productAttributeId));
            query = query.OrderBy(x => x.Name);

            var products = new PagedList<Product>(query, pageIndex, pageSize);
            return products;
        }

        /// <summary>
        /// Gets associated products
        /// </summary>
        /// <param name="parentGroupedProductId">Parent product identifier (used with grouped products)</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        public virtual IList<Product> GetAssociatedProducts(string parentGroupedProductId,
            string storeId = "", string vendorId = "", bool showHidden = false)
        {
            var query = from p in _productRepository.Table
                        select p;

            query = query.Where(x => x.ParentGroupedProductId == parentGroupedProductId);
            if (!showHidden)
            {
                query = query.Where(x => x.Published);
            }
            if (!showHidden)
            {
                var nowUtc = DateTime.UtcNow;
                //available dates
                query = query.Where(p =>
                    (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < nowUtc) &&
                    (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > nowUtc));
            }
            //vendor filtering
            if (!String.IsNullOrEmpty(vendorId))
            {
                query = query.Where(p => p.VendorId == vendorId);
            }
            query = query.OrderBy(x => x.DisplayOrder);

            var products = query.ToList();

            //ACL mapping
            if (!showHidden)
            {
                products = products.Where(x => _aclService.Authorize(x)).ToList();
            }
            //Store mapping
            if (!showHidden && !String.IsNullOrEmpty(storeId))
            {
                products = products.Where(x => _storeMappingService.Authorize(x, storeId)).ToList();
            }

            return products;
        }

        /// <summary>
        /// Get low stock products
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="products">Low stock products</param>
        /// <param name="combinations">Low stock attribute combinations</param>
        public virtual void GetLowStockProducts(string vendorId,
            out IList<Product> products,
            out IList<ProductAttributeCombination> combinations)
        {
            //Track inventory for product
            string vendors = "";
            if (!String.IsNullOrEmpty(vendorId))
                vendors = " && this.VendorId=='" + vendorId.ToString()+"' ";
            if (String.IsNullOrEmpty(vendorId))
                vendorId = "";

            var doc = MongoDB.Bson.Serialization.BsonSerializer
                        .Deserialize<BsonDocument>
                        ("{$where: \" this.MinStockQuantity >= this.StockQuantity && this.ProductTypeId == 5 && this.ManageInventoryMethodId != 0 " + vendors + " \" }");

            products = _productRepository.Collection.Find(new CommandDocument(doc)).ToListAsync().Result;


            //Track inventory for product by product attributes
            var query2_1 = from p in _productRepository.Table
                           where
                           p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
                           (vendorId == "" || p.VendorId == vendorId)
                           from c in p.ProductAttributeCombinations
                           select c;

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
        public virtual Product GetProductBySku(string sku)
        {
            if (String.IsNullOrEmpty(sku))
                return null;

            sku = sku.Trim();

            var query = from p in _productRepository.Table
                        orderby p.Id
                        where p.Sku == sku
                        select p;
            var product = query.FirstOrDefault();
            return product;
        }

        /// <summary>
        /// Update product review totals
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateProductReviewTotals(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            int approvedRatingSum = 0;
            int notApprovedRatingSum = 0;
            int approvedTotalReviews = 0;
            int notApprovedTotalReviews = 0;
            var reviews = _productReviewRepository.Table.Where(x => x.ProductId == product.Id); //product.ProductReviews;
            foreach (var pr in reviews)
            {
                if (pr.IsApproved)
                {
                    approvedRatingSum += pr.Rating;
                    approvedTotalReviews++;
                }
                else
                {
                    notApprovedRatingSum += pr.Rating;
                    notApprovedTotalReviews++;
                }
            }

            product.ApprovedRatingSum = approvedRatingSum;
            product.NotApprovedRatingSum = notApprovedRatingSum;
            product.ApprovedTotalReviews = approvedTotalReviews;
            product.NotApprovedTotalReviews = notApprovedTotalReviews;

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.ApprovedRatingSum, product.ApprovedRatingSum)
                    .Set(x => x.NotApprovedRatingSum, product.NotApprovedRatingSum)
                    .Set(x => x.ApprovedTotalReviews, product.ApprovedTotalReviews)
                    .Set(x => x.NotApprovedTotalReviews, product.NotApprovedTotalReviews)
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            _eventPublisher.EntityUpdated(product);

            //UpdateProduct(product);
        }

        public virtual void UpdateProductReview(ProductReview productreview)
        {
            if (productreview == null)
                throw new ArgumentNullException("productreview");

            //_productPictureRepository.Update(productPicture);
            var builder = Builders<ProductReview>.Filter;
            var filter = builder.Eq(x => x.Id, productreview.Id);
            var update = Builders<ProductReview>.Update
                .Set(x => x.Title, productreview.Title)
                .Set(x => x.ReviewText, productreview.ReviewText)
                .Set(x => x.IsApproved, productreview.IsApproved);

            var result = _productReviewRepository.Collection.UpdateManyAsync(filter, update).Result;

            //event notification
            _eventPublisher.EntityUpdated(productreview);
        }

        public virtual void UpdateAssociatedProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("productreview");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            var update = Builders<Product>.Update
                .Set(x => x.DisplayOrder, product.DisplayOrder)
                .Set(x => x.ParentGroupedProductId, product.ParentGroupedProductId);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            _eventPublisher.EntityUpdated(product);

        }


        /// <summary>
        /// Update HasTierPrices property (used for performance optimization)
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateHasTierPricesProperty(string productId)
        {
            var product = GetProductById(productId);
            if (product == null)
                throw new ArgumentNullException("product");

            product.HasTierPrices = product.TierPrices.Any();

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.HasTierPrices, product.HasTierPrices)
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //event notification
            _eventPublisher.EntityUpdated(product);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
        }

        /// <summary>
        /// Update HasDiscountsApplied property (used for performance optimization)
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateHasDiscountsApplied(string productId)
        {
            var product = GetProductById(productId);
            if (product == null)
                throw new ArgumentNullException("product");

            product.HasDiscountsApplied = product.AppliedDiscounts.Any();

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.HasDiscountsApplied, product.HasDiscountsApplied)
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));


            //event notification
            _eventPublisher.EntityUpdated(product);

            //UpdateProduct(product);
        }

        #endregion

        #region Inventory management methods

        /// <summary>
        /// Adjust inventory
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantityToChange">Quantity to increase or descrease</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        public virtual void AdjustInventory(Product product, int quantityToChange, string attributesXml = "")
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantityToChange == 0)
                return;
            //var prevStockQuantity = product.GetTotalStockQuantity();

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                var prevStockQuantity = product.GetTotalStockQuantity();

                //update stock quantity
                if (product.UseMultipleWarehouses)
                {
                    //use multiple warehouses
                    if (quantityToChange < 0)
                        ReserveInventory(product, quantityToChange);
                    else
                        UnblockReservedInventory(product, quantityToChange);
                }
                else
                {
                    //do not use multiple warehouses
                    //simple inventory management
                    product.StockQuantity += quantityToChange;
                    UpdateStockProduct(product);
                }

                //check if minimum quantity is reached
                if (quantityToChange < 0 && product.MinStockQuantity >= product.GetTotalStockQuantity())
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
                                    .CurrentDate("UpdateDate");
                            _productRepository.Collection.UpdateOneAsync(filter, update);
                            //cache
                            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

                            //event notification
                            _eventPublisher.EntityUpdated(product);

                            //UpdateProduct(product);
                            break;
                        case LowStockActivity.Unpublish:
                            product.Published = false;
                            var filter2 = Builders<Product>.Filter.Eq("Id", product.Id);
                            var update2 = Builders<Product>.Update
                                    .Set(x => x.Published, product.Published)
                                    .CurrentDate("UpdateDate");
                            _productRepository.Collection.UpdateOneAsync(filter2, update2);
                            
                            //cache
                            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
                            if(product.ShowOnHomePage)
                                _cacheManager.RemoveByPattern(PRODUCTS_SHOWONHOMEPAGE);

                            //event notification
                            _eventPublisher.EntityUpdated(product);

                            //UpdateProduct(product);
                            break;
                        default:
                            break;
                    }
                }
                //qty is increased. product is back in stock (minimum stock quantity is reached again)?
                if (_catalogSettings.PublishBackProductWhenCancellingOrders)
                {
                    if (quantityToChange > 0 && prevStockQuantity <= product.MinStockQuantity && product.MinStockQuantity < product.GetTotalStockQuantity())
                    {
                        switch (product.LowStockActivity)
                        {
                            case LowStockActivity.DisableBuyButton:
                                //product.DisableBuyButton = false;
                                //product.DisableWishlistButton = false;
                                //UpdateProduct(product);
                                var filter = Builders<Product>.Filter.Eq("Id", product.Id);
                                var update = Builders<Product>.Update
                                        .Set(x => x.DisableBuyButton, product.DisableBuyButton)
                                        .Set(x => x.DisableWishlistButton, product.DisableWishlistButton)
                                        .CurrentDate("UpdateDate");
                                _productRepository.Collection.UpdateOneAsync(filter, update);
                                //cache
                                _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
                                break;
                            case LowStockActivity.Unpublish:
                                //product.Published = true;
                                //UpdateProduct(product);
                                product.Published = false;
                                var filter2 = Builders<Product>.Filter.Eq("Id", product.Id);
                                var update2 = Builders<Product>.Update
                                        .Set(x => x.Published, product.Published)
                                        .CurrentDate("UpdateDate");
                                _productRepository.Collection.UpdateOneAsync(filter2, update2);

                                //cache
                                _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
                                if (product.ShowOnHomePage)
                                    _cacheManager.RemoveByPattern(PRODUCTS_SHOWONHOMEPAGE);

                                break;
                            default:
                                break;
                        }
                    }
                }

                //send email notification
                if (quantityToChange < 0 && product.GetTotalStockQuantity() < product.NotifyAdminForQuantityBelow)
                {
                    _workflowMessageService.SendQuantityBelowStoreOwnerNotification(product, _localizationSettings.DefaultAdminLanguageId);
                }
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);
                if (combination != null)
                {
                    combination.StockQuantity += quantityToChange;
                    _productAttributeService.UpdateProductAttributeCombination(combination);
                    product.StockQuantity += quantityToChange;
                    UpdateStockProduct(product);
                    //send email notification
                    if (quantityToChange < 0 && combination.StockQuantity < combination.NotifyAdminForQuantityBelow)
                    {
                        _workflowMessageService.SendQuantityBelowStoreOwnerNotification(combination, _localizationSettings.DefaultAdminLanguageId);
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
                    var associatedProduct = GetProductById(attributeValue.AssociatedProductId);
                    if (associatedProduct != null)
                    {
                        AdjustInventory(associatedProduct, quantityToChange * attributeValue.Quantity);
                    }
                }
            }
            
        }

        /// <summary>
        /// Reserve the given quantity in the warehouses.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual void ReserveInventory(Product product, int quantity)
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
                foreach (var item in productInventory)
                {
                    var selectQty = Math.Min(item.StockQuantity - item.ReservedQuantity, qty);
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
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            _eventPublisher.EntityUpdated(product);
            //this.UpdateProduct(product);
        }

        /// <summary>
        /// Unblocks the given quantity reserved items in the warehouses
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be positive</param>
        public virtual void UnblockReservedInventory(Product product, int quantity)
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

            foreach (var item in productInventory)
            {
                var selectQty = Math.Min(item.ReservedQuantity, qty);
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
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            _eventPublisher.EntityUpdated(product);
        }

        /// <summary>
        /// Book the reserved quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual void BookReservedInventory(Product product, string warehouseId, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", "quantity");

            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                return;
            if (!product.UseMultipleWarehouses)
                return;

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
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
            //event notification
            _eventPublisher.EntityUpdated(product);

            //TODO add support for bundled products (AttributesXml)
        }

        /// <summary>
        /// Reverse booked inventory (if acceptable)
        /// </summary>
        /// <param name="product">product</param>
        /// <param name="shipmentItem">Shipment item</param>
        /// <returns>Quantity reversed</returns>
        public virtual int ReverseBookedInventory(Product product, ShipmentItem shipmentItem)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");
            
            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                return 0;
            if (!product.UseMultipleWarehouses)
                return 0;

            var pwi = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == shipmentItem.WarehouseId);
            if (pwi == null)
                return 0;

            var shipment = EngineContext.Current.Resolve<IShipmentService>().GetShipmentById(shipmentItem.ShipmentId);

            //not shipped yet? hence "BookReservedInventory" method was not invoked
            if (!shipment.ShippedDateUtc.HasValue)
                return 0;

            var qty = shipmentItem.Quantity;

            pwi.StockQuantity += qty;
            pwi.ReservedQuantity += qty;

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, product.Id);
            filter = filter & builder.Where(x => x.ProductWarehouseInventory.Any(y => y.WarehouseId == pwi.WarehouseId));

            var update = Builders<Product>.Update
                    .Set(x => x.ProductWarehouseInventory.ElementAt(-1), pwi)
                    .CurrentDate("UpdateDate");
            _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, product.Id));
            //_cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(product);

            return qty;
        }

        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void DeleteRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            //_relatedProductRepository.Delete(relatedProduct);

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.RelatedProducts, relatedProduct);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", relatedProduct.ProductId1), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, relatedProduct.ProductId1));

            //event notification
            _eventPublisher.EntityDeleted(relatedProduct);
        }

        
        public virtual void InsertRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            //_relatedProductRepository.Insert(relatedProduct);

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.RelatedProducts, relatedProduct);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", relatedProduct.ProductId1), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, relatedProduct.ProductId1));

            //event notification
            _eventPublisher.EntityInserted(relatedProduct);
        }

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void UpdateRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            //_relatedProductRepository.Update(relatedProduct);
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, relatedProduct.ProductId1);
            filter = filter & builder.ElemMatch(x => x.RelatedProducts, y => y.Id == relatedProduct.Id);
            var update = Builders<Product>.Update
                .Set(x => x.RelatedProducts.ElementAt(-1).DisplayOrder, relatedProduct.DisplayOrder);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, relatedProduct.ProductId1));

            //event notification
            _eventPublisher.EntityUpdated(relatedProduct);
        }

        #endregion

        #region Cross-sell products

        /// <summary>
        /// Deletes a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell identifier</param>
        public virtual void DeleteCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.CrossSellProduct, crossSellProduct.ProductId2);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", crossSellProduct.ProductId1), update);


            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, crossSellProduct.ProductId1));
            //_crossSellProductRepository.Delete(crossSellProduct);

            //event notification
            _eventPublisher.EntityDeleted(crossSellProduct);
        }


        /// <summary>
        /// Inserts a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        public virtual void InsertCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            //_crossSellProductRepository.Insert(crossSellProduct);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.CrossSellProduct, crossSellProduct.ProductId2);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", crossSellProduct.ProductId1), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, crossSellProduct.ProductId1));

            //event notification
            _eventPublisher.EntityInserted(crossSellProduct);
        }

       
        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        /// <returns>Cross-sells</returns>
        public virtual IList<Product> GetCrosssellProductsByShoppingCart(IList<ShoppingCartItem> cart, int numberOfProducts)
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
                var product = GetProductById(sci.ProductId);
                var crossSells = product.CrossSellProduct; //GetCrossSellProductsByProductId1(sci.ProductId);
                foreach (var crossSell in crossSells)
                {
                    //validate that this product is not added to result yet
                    //validate that this product is not in the cart
                    if (result.Find(p => p.Id == crossSell) == null &&
                        !cartProductIds.Contains(crossSell))
                    {
                        var productToAdd = GetProductById(crossSell);
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
        public virtual void DeleteTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            //_tierPriceRepository.Delete(tierPrice);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.TierPrices, tierPrice);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", tierPrice.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, tierPrice.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(tierPrice);
        }

        /// <summary>
        /// Inserts a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual void InsertTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            //_tierPriceRepository.Insert(tierPrice);
            
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.TierPrices, tierPrice);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", tierPrice.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, tierPrice.ProductId));

            //event notification
            _eventPublisher.EntityInserted(tierPrice);
        }

        /// <summary>
        /// Updates the tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual void UpdateTierPrice(TierPrice tierPrice)
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
                .Set(x => x.TierPrices.ElementAt(-1).CustomerRoleId, tierPrice.CustomerRoleId);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, tierPrice.ProductId));


            //event notification
            _eventPublisher.EntityUpdated(tierPrice);
        }

        #endregion

        #region Product pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void DeleteProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            //_productPictureRepository.Delete(productPicture);

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductPictures, productPicture);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productPicture.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productPicture.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(productPicture);
        }

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void InsertProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            //_productPictureRepository.Insert(productPicture);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductPictures, productPicture);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productPicture.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productPicture.ProductId));


            //event notification
            _eventPublisher.EntityInserted(productPicture);
        }

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void InsertProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");
           
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductTags, productTag);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productTag.ProductId), update);

            var builder = Builders<ProductTag>.Filter;
            var filter = builder.Eq(x => x.Id, productTag.Id);
            var updateTag = Builders<ProductTag>.Update
                .Inc(x => x.Count, 1);
            var result = _productTagRepository.Collection.UpdateManyAsync(filter, updateTag).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productTag.ProductId));


            //event notification
            _eventPublisher.EntityInserted(productTag);
        }

        public virtual void DeleteProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductTags, productTag);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productTag.ProductId), update);

            var builder = Builders<ProductTag>.Filter;
            var filter = builder.Eq(x => x.Id, productTag.Id);
            var updateTag = Builders<ProductTag>.Update
                .Inc(x => x.Count, -1);
            var result = _productTagRepository.Collection.UpdateManyAsync(filter, updateTag).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productTag.ProductId));

            //event notification
            _eventPublisher.EntityDeleted(productTag);
        }

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void UpdateProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            //_productPictureRepository.Update(productPicture);
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productPicture.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductPictures, y => y.Id == productPicture.Id);
            var update = Builders<Product>.Update
                .Set(x => x.ProductPictures.ElementAt(-1).DisplayOrder, productPicture.DisplayOrder)
                .Set(x => x.ProductPictures.ElementAt(-1).MimeType, productPicture.MimeType)
                .Set(x => x.ProductPictures.ElementAt(-1).SeoFilename, productPicture.SeoFilename)
                .Set(x => x.ProductPictures.ElementAt(-1).AltAttribute, productPicture.AltAttribute)
                .Set(x => x.ProductPictures.ElementAt(-1).TitleAttribute, productPicture.TitleAttribute);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productPicture.ProductId));

            //event notification
            _eventPublisher.EntityUpdated(productPicture);
        }

        #endregion

        #region Recommended products

        /// <summary>
        /// Gets recommended products for customer roles
        /// </summary>
        /// <param name="customerRoleIds">Customer role ids</param>
        /// <returns>Products</returns>
        public virtual IList<Product> GetRecommendedProducts(string[] customerRoleIds)
        {

            return _cacheManager.Get(string.Format(PRODUCTS_CUSTOMER_ROLE, string.Join(",", customerRoleIds)), 
                () =>
                {
                    var query = from cr in _customerRoleProductRepository.Table
                                orderby cr.DisplayOrder
                                where customerRoleIds.Contains(cr.CustomerRoleId)
                                select cr.ProductId;

                    var productIds = query.ToList().Distinct();

                    var products = new List<Product>();

                    foreach (var product in GetProductsByIds(productIds.ToArray()))
                        if (product.Published)
                            products.Add(product);

                    return products;
                });
        }

        #endregion

        #region Product reviews

        /// <summary>
        /// Gets all product reviews
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <returns>Reviews</returns>
        public virtual IList<ProductReview> GetAllProductReviews(string customerId, bool? approved,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, string storeId = "", string productId = "")
        {
            var query = from p in _productReviewRepository.Table
                        select p;

            if (approved.HasValue)
                query = query.Where(c => c.IsApproved == approved);
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(c => c.CustomerId == customerId);
            if (fromUtc.HasValue)
                query = query.Where(c => fromUtc.Value <= c.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(c => toUtc.Value >= c.CreatedOnUtc);
            if (!String.IsNullOrEmpty(message))
                query = query.Where(c => c.Title.Contains(message) || c.ReviewText.Contains(message));
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(c => c.StoreId == storeId || c.StoreId == "");
            if (!String.IsNullOrEmpty(productId))
                query = query.Where(c => c.ProductId == productId);

            query = query.OrderBy(c => c.CreatedOnUtc);
            var content = query.ToList();
            return content;
        }

        

        public virtual int RatingSumProduct(string productId, string storeId)
        {
            var query = from p in _productReviewRepository.Table
                        where p.ProductId == productId && p.IsApproved && (p.StoreId == storeId || p.StoreId == "")
                        group p by true into g
                        select new { Sum = g.Sum(x=>x.Rating) };
            var content = query.ToListAsync().Result;
            return content.Count > 0 ? content.FirstOrDefault().Sum : 0;
        }

        public virtual int TotalReviewsProduct(string productId, string storeId)
        {
            var query = from p in _productReviewRepository.Table
                        where p.ProductId == productId && p.IsApproved && (p.StoreId == storeId || p.StoreId == "")
                        group p by true into g
                        select new { Count = g.Count() };
            var content = query.ToListAsync().Result;
            return content.Count > 0 ? content.FirstOrDefault().Count : 0;
        }


        /// <summary>
        /// Inserts a product review
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void InsertProductReview(ProductReview productReview)
        {
            if (productReview == null)
                throw new ArgumentNullException("productPicture");

            _productReviewRepository.Insert(productReview);

            //event notification
            _eventPublisher.EntityInserted(productReview);
        }


        /// <summary>
        /// Deletes a product review
        /// </summary>
        /// <param name="productReview">Product review</param>
        public virtual void DeleteProductReview(ProductReview productReview)
        {
            if (productReview == null)
                throw new ArgumentNullException("productReview");

            _productReviewRepository.Delete(productReview);
            
            //event notification
            _eventPublisher.EntityDeleted(productReview);
        }

        /// <summary>
        /// Gets product review
        /// </summary>
        /// <param name="productReviewId">Product review identifier</param>
        /// <returns>Product review</returns>
        public virtual ProductReview GetProductReviewById(string productReviewId)
        {
            return _productReviewRepository.GetById(productReviewId);
        }

        #endregion

        #region Product warehouse inventory

        /// <summary>
        /// Deletes a ProductWarehouseInventory
        /// </summary>
        /// <param name="pwi">ProductWarehouseInventory</param>
        public virtual void DeleteProductWarehouseInventory(ProductWarehouseInventory pwi)
        {
            if (pwi == null)
                throw new ArgumentNullException("pwi");

            //_productWarehouseInventoryRepository.Delete(pwi);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductWarehouseInventory, pwi);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", pwi.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, pwi.ProductId));

        }

        public virtual void InsertProductWarehouseInventory(ProductWarehouseInventory pwi)
        {
            if (pwi == null)
                throw new ArgumentNullException("productWarehouse");

            //_productPictureRepository.Insert(productPicture);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductWarehouseInventory, pwi);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", pwi.ProductId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, pwi.ProductId));

            //event notification
            _eventPublisher.EntityInserted(pwi);
        }

        public virtual void UpdateProductWarehouseInventory(ProductWarehouseInventory pwi)
        {
            if (pwi == null)
                throw new ArgumentNullException("productWarehouseInventory");

            //_productPictureRepository.Update(productPicture);
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, pwi.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductWarehouseInventory, y => y.Id == pwi.Id);
            var update = Builders<Product>.Update
                .Set(x => x.ProductWarehouseInventory.ElementAt(-1).StockQuantity, pwi.StockQuantity)
                .Set(x => x.ProductWarehouseInventory.ElementAt(-1).ReservedQuantity, pwi.ReservedQuantity);

            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, pwi.ProductId));
            //event notification
            _eventPublisher.EntityUpdated(pwi);
        }


        public virtual void DeleteDiscount(Discount discount, string productId)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            //_productWarehouseInventoryRepository.Delete(pwi);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.AppliedDiscounts, discount);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productId));


            _eventPublisher.EntityDeleted(discount);
        }

        public virtual void InsertDiscount(Discount discount, string productId)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            //_productPictureRepository.Insert(productPicture);
            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.AppliedDiscounts, discount);
            _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productId), update);

            //cache
            _cacheManager.RemoveByPattern(string.Format(PRODUCTS_BY_ID_KEY, productId));

            //event notification
            _eventPublisher.EntityInserted(discount);
        }


        #endregion

        #endregion
    }
}
