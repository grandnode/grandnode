using Grand.Core.Caching;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Configuration;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Polls;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Topics;
using Grand.Core.Domain.Vendors;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //currencies
        INotificationHandler<EntityInserted<Currency>>,
        INotificationHandler<EntityUpdated<Currency>>,
        INotificationHandler<EntityDeleted<Currency>>,
        //store
        INotificationHandler<EntityInserted<Store>>,
        INotificationHandler<EntityUpdated<Store>>,
        INotificationHandler<EntityDeleted<Store>>,
        //settings
        INotificationHandler<EntityUpdated<Setting>>,
        //manufacturers
        INotificationHandler<EntityInserted<Manufacturer>>,
        INotificationHandler<EntityUpdated<Manufacturer>>,
        INotificationHandler<EntityDeleted<Manufacturer>>,
        //vendors
        INotificationHandler<EntityInserted<Vendor>>,
        INotificationHandler<EntityUpdated<Vendor>>,
        INotificationHandler<EntityDeleted<Vendor>>,
        //product manufacturers
        INotificationHandler<EntityInserted<ProductManufacturer>>,
        INotificationHandler<EntityUpdated<ProductManufacturer>>,
        INotificationHandler<EntityDeleted<ProductManufacturer>>,
        //categories
        INotificationHandler<EntityInserted<Category>>,
        INotificationHandler<EntityUpdated<Category>>,
        INotificationHandler<EntityDeleted<Category>>,
        //product categories
        INotificationHandler<EntityInserted<ProductCategory>>,
        INotificationHandler<EntityUpdated<ProductCategory>>,
        INotificationHandler<EntityDeleted<ProductCategory>>,
        //products
        INotificationHandler<EntityInserted<Product>>,
        INotificationHandler<EntityUpdated<Product>>,
        INotificationHandler<EntityDeleted<Product>>,
        //related product
        INotificationHandler<EntityInserted<RelatedProduct>>,
        INotificationHandler<EntityUpdated<RelatedProduct>>,
        INotificationHandler<EntityDeleted<RelatedProduct>>,
        //similar product
        INotificationHandler<EntityInserted<SimilarProduct>>,
        INotificationHandler<EntityUpdated<SimilarProduct>>,
        INotificationHandler<EntityDeleted<SimilarProduct>>,
        //product tags
        INotificationHandler<EntityInserted<ProductTag>>,
        INotificationHandler<EntityUpdated<ProductTag>>,
        INotificationHandler<EntityDeleted<ProductTag>>,
        //specification attributes
        INotificationHandler<EntityUpdated<SpecificationAttribute>>,
        INotificationHandler<EntityDeleted<SpecificationAttribute>>,
        //specification attribute options
        INotificationHandler<EntityUpdated<SpecificationAttributeOption>>,
        INotificationHandler<EntityDeleted<SpecificationAttributeOption>>,
        //Product specification attribute
        INotificationHandler<EntityInserted<ProductSpecificationAttribute>>,
        INotificationHandler<EntityUpdated<ProductSpecificationAttribute>>,
        INotificationHandler<EntityDeleted<ProductSpecificationAttribute>>,
        //Topics
        INotificationHandler<EntityInserted<Topic>>,
        INotificationHandler<EntityUpdated<Topic>>,
        INotificationHandler<EntityDeleted<Topic>>,
        //Orders
        INotificationHandler<EntityInserted<Order>>,
        INotificationHandler<EntityUpdated<Order>>,
        INotificationHandler<EntityDeleted<Order>>,
        //Product picture mapping
        INotificationHandler<EntityInserted<ProductPicture>>,
        INotificationHandler<EntityUpdated<ProductPicture>>,
        INotificationHandler<EntityDeleted<ProductPicture>>,
        //polls
        INotificationHandler<EntityInserted<Poll>>,
        INotificationHandler<EntityUpdated<Poll>>,
        INotificationHandler<EntityDeleted<Poll>>,
        //blog posts
        INotificationHandler<EntityInserted<BlogPost>>,
        INotificationHandler<EntityUpdated<BlogPost>>,
        INotificationHandler<EntityDeleted<BlogPost>>,
        //blog post category
        INotificationHandler<EntityInserted<BlogCategory>>,
        INotificationHandler<EntityUpdated<BlogCategory>>,
        INotificationHandler<EntityDeleted<BlogCategory>>,
        //news items
        INotificationHandler<EntityInserted<NewsItem>>,
        INotificationHandler<EntityUpdated<NewsItem>>,
        INotificationHandler<EntityDeleted<NewsItem>>,
        //states/province
        INotificationHandler<EntityInserted<StateProvince>>,
        INotificationHandler<EntityUpdated<StateProvince>>,
        INotificationHandler<EntityDeleted<StateProvince>>,
        //return requests
        INotificationHandler<EntityInserted<ReturnRequestAction>>,
        INotificationHandler<EntityUpdated<ReturnRequestAction>>,
        INotificationHandler<EntityDeleted<ReturnRequestAction>>,
        INotificationHandler<EntityInserted<ReturnRequestReason>>,
        INotificationHandler<EntityUpdated<ReturnRequestReason>>,
        INotificationHandler<EntityDeleted<ReturnRequestReason>>,
        //templates
        INotificationHandler<EntityInserted<CategoryTemplate>>,
        INotificationHandler<EntityUpdated<CategoryTemplate>>,
        INotificationHandler<EntityDeleted<CategoryTemplate>>,
        INotificationHandler<EntityInserted<ManufacturerTemplate>>,
        INotificationHandler<EntityUpdated<ManufacturerTemplate>>,
        INotificationHandler<EntityDeleted<ManufacturerTemplate>>,
        INotificationHandler<EntityInserted<ProductTemplate>>,
        INotificationHandler<EntityUpdated<ProductTemplate>>,
        INotificationHandler<EntityDeleted<ProductTemplate>>,
        INotificationHandler<EntityInserted<TopicTemplate>>,
        INotificationHandler<EntityUpdated<TopicTemplate>>,
        INotificationHandler<EntityDeleted<TopicTemplate>>,
        //checkout attributes
        INotificationHandler<EntityInserted<CheckoutAttribute>>,
        INotificationHandler<EntityUpdated<CheckoutAttribute>>,
        INotificationHandler<EntityDeleted<CheckoutAttribute>>,
        //shopping cart items
        INotificationHandler<EntityUpdated<ShoppingCartItem>>
    {


        private readonly ICacheManager _cacheManager;

        public ModelCacheEventConsumer(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        //currencies
        public async Task Handle(EntityInserted<Currency> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Currency> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Currency> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.AVAILABLE_CURRENCIES_PATTERN_KEY);
        }

        //stores
        public async Task Handle(EntityInserted<Store> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.AVAILABLE_STORES_MODEL_KEY);
        }
        public async Task Handle(EntityUpdated<Store> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.AVAILABLE_STORES_MODEL_KEY);
        }
        public async Task Handle(EntityDeleted<Store> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.AVAILABLE_STORES_MODEL_KEY);
        }

        //settings
        public async Task Handle(EntityUpdated<Setting> eventMessage, CancellationToken cancellationToken)
        {
            //cache should be cleared
            await _cacheManager.Clear();
        }

        //vendors
        public async Task Handle(EntityInserted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.VENDOR_NAVIGATION_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.VENDOR_NAVIGATION_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.VENDOR_NAVIGATION_PATTERN_KEY);
        }

        //manufacturers
        public async Task Handle(EntityInserted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }

        //product manufacturers
        public async Task Handle(EntityInserted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
        public async Task Handle(EntityUpdated<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
        public async Task Handle(EntityDeleted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }

        //categories
        public async Task Handle(EntityInserted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }

        public async Task Handle(EntityDeleted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }

        //product categories
        public async Task Handle(EntityInserted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CATEGORY_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.CategoryId));
        }
        public async Task Handle(EntityUpdated<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CATEGORY_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.CategoryId));
        }
        public async Task Handle(EntityDeleted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CATEGORY_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.CategoryId));
        }

        //products
        public async Task Handle(EntityInserted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);

            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.Id));
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
        public async Task Handle(EntityDeleted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);

            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.Id));
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }

        //product tags
        public async Task Handle(EntityInserted<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }

        //related products

        public async Task Handle(EntityInserted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));

        }
        public async Task Handle(EntityUpdated<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }
        public async Task Handle(EntityDeleted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_RELATED_IDS_PATTERN_KEY, eventMessage.Entity.Id));
        }

        //similar products
        public async Task Handle(EntityInserted<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }
        public async Task Handle(EntityUpdated<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }
        public async Task Handle(EntityDeleted<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId1));
            await _cacheManager.RemoveAsync(string.Format(ModelCacheEventConst.PRODUCTS_SIMILAR_IDS_PATTERN_KEY, eventMessage.Entity.ProductId2));
        }

        //specification attributes
        public async Task Handle(EntityUpdated<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_SPECS_PATTERN);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_SPECS_PATTERN);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }

        //specification attribute options

        public async Task Handle(EntityUpdated<SpecificationAttributeOption> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_SPECS_PATTERN);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<SpecificationAttributeOption> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_SPECS_PATTERN);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }

        //Product specification attribute
        public async Task Handle(EntityInserted<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_SPECS_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_SPECS_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_SPECS_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SPECS_FILTER_PATTERN_KEY);
        }

        //Topics
        public async Task Handle(EntityInserted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }

        //Orders
        public async Task Handle(EntityInserted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }

        //Product picture mappings
        public async Task Handle(EntityInserted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }
        public async Task Handle(EntityUpdated<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }
        public async Task Handle(EntityDeleted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }

        //Polls
        public async Task Handle(EntityInserted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }

        //Blog posts
        public async Task Handle(EntityInserted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }

        //Blog post category
        public async Task Handle(EntityInserted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }

        //News items
        public async Task Handle(EntityInserted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }

        //State/province
        public async Task Handle(EntityInserted<StateProvince> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.STATEPROVINCES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<StateProvince> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.STATEPROVINCES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<StateProvince> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.STATEPROVINCES_PATTERN_KEY);
        }

        //retunr requests
        public async Task Handle(EntityInserted<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }

        //templates
        public async Task Handle(EntityInserted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }

        //checkout attributes
        public async Task Handle(EntityInserted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }

        //shopping cart items        
        public async Task Handle(EntityUpdated<ShoppingCartItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }

    }
}
