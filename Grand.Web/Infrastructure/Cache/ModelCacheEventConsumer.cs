using Grand.Core.Caching;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Configuration;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Polls;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Topics;
using Grand.Core.Domain.Vendors;
using Grand.Core.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //languages
        INotificationHandler<EntityInserted<Language>>,
        INotificationHandler<EntityUpdated<Language>>,
        INotificationHandler<EntityDeleted<Language>>,
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
        //bundle product
        INotificationHandler<EntityInserted<BundleProduct>>,
        INotificationHandler<EntityUpdated<BundleProduct>>,
        INotificationHandler<EntityDeleted<BundleProduct>>,
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
        //Product attributes
        INotificationHandler<EntityDeleted<ProductAttribute>>,
        //Product attributes
        INotificationHandler<EntityInserted<ProductAttributeMapping>>,
        INotificationHandler<EntityDeleted<ProductAttributeMapping>>,
        //Product attribute values
        INotificationHandler<EntityUpdated<ProductAttributeValue>>,
        //Topics
        INotificationHandler<EntityInserted<Topic>>,
        INotificationHandler<EntityUpdated<Topic>>,
        INotificationHandler<EntityDeleted<Topic>>,
        //Orders
        INotificationHandler<EntityInserted<Order>>,
        INotificationHandler<EntityUpdated<Order>>,
        INotificationHandler<EntityDeleted<Order>>,
        //Picture
        INotificationHandler<EntityInserted<Picture>>,
        INotificationHandler<EntityUpdated<Picture>>,
        INotificationHandler<EntityDeleted<Picture>>,
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
        /// <summary>
        /// Key for categories on the search page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SEARCH_CATEGORIES_MODEL_KEY = "Grand.pres.search.categories-{0}-{1}-{2}";
        public const string SEARCH_CATEGORIES_PATTERN_KEY = "Grand.pres.search.categories";

        /// <summary>
        /// Key for ManufacturerNavigationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : current manufacturer id
        /// {1} : language id
        /// {2} : roles of the current user
        /// {3} : current store ID
        /// </remarks>
        public const string MANUFACTURER_NAVIGATION_MODEL_KEY = "Grand.pres.manufacturer.navigation-{0}-{1}-{2}-{3}";
        public const string MANUFACTURER_NAVIGATION_PATTERN_KEY = "Grand.pres.manufacturer.navigation";

        /// <summary>
        /// Key for ManufacturerNavigationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// </remarks>
        public const string MANUFACTURER_NAVIGATION_MENU = "Grand.pres.manufacturer.navigation.menu-{0}-{1}";

        /// <summary>
        /// Key for caching of manufacturer displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : language ID
        /// </remarks>
        public const string MANUFACTURER_HOMEPAGE_KEY = "Grand.pres.manufacturer.navigation.homepage-{0}-{1}";
        public const string MANUFACTURER_FEATURED_PRODUCT_HOMEPAGE_KEY = "Grand.pres.manufacturer.navigation.homepage-fp-{0}-{1}";


        /// <summary>
        /// Key for VendorNavigationModel caching
        /// </summary>
        public const string VENDOR_NAVIGATION_MODEL_KEY = "Grand.pres.vendor.navigation";
        public const string VENDOR_NAVIGATION_PATTERN_KEY = "Grand.pres.vendor.navigation";

        /// <summary>
        /// Key for caching of a value indicating whether a manufacturer has featured products
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY = "Grand.pres.manufacturer.hasfeaturedproducts-{0}-{1}-{2}";
        public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "Grand.pres.manufacturer.hasfeaturedproducts";

        /// <summary>
        /// Key for CategoryNavigationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_ALL_MODEL_KEY = "Grand.pres.category.all-{0}-{1}-{2}";

        /// <summary>
        /// Key for CategorySearchBoxModel caching
        /// </summary>
        /// <remarks>
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_ALL_SEARCHBOX = "Grand.pres.category.all.searchbox-{0}-{1}";

        public const string CATEGORY_ALL_PATTERN_KEY = "Grand.pres.category.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : comma separated list of customer roles
        /// {1} : current store ID
        /// {2} : category ID
        /// </remarks>
        public const string CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY = "Grand.pres.category.numberofproducts-{0}-{1}-{2}";

        /// <summary>
        /// Key for caching of a value indicating whether a category has featured products
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_HAS_FEATURED_PRODUCTS_KEY = "Grand.pres.category.hasfeaturedproducts-{0}-{1}-{2}";
        public const string CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "Grand.pres.category.hasfeaturedproducts";

        /// <summary>
        /// Key for caching of category breadcrumb
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// </remarks>
        public const string CATEGORY_BREADCRUMB_KEY = "Grand.pres.category.breadcrumb-{0}-{1}-{2}-{3}";
        public const string CATEGORY_BREADCRUMB_PATTERN_KEY = "Grand.pres.category.breadcrumb";

        /// <summary>
        /// Key for caching of knowledgebase category breadcrumb
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// </remarks>
        public const string KNOWLEDGEBASE_CATEGORY_BREADCRUMB_KEY = "Grand.knowledgebase.category.breadcrumb-{0}-{1}-{2}-{3}";
        public const string KNOWLEDGEBASE_CATEGORY_BREADCRUMB_PATTERN_KEY = "Grand.knowledgebase.category.breadcrumb";

        /// <summary>
        /// Key for caching of subcategories of certain category
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// {3} : language ID
        /// {4} : is connection SSL secured (included in a category picture URL)
        /// </remarks>
        public const string CATEGORY_SUBCATEGORIES_KEY = "Grand.pres.category.subcategories-{0}-{1}-{2}-{3}-{4}";
        public const string CATEGORY_SUBCATEGORIES_PATTERN_KEY = "Grand.pres.category.subcategories";

        /// <summary>
        /// Key for caching of categories displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : roles of the current user
        /// {1} : current store ID
        /// {2} : language ID
        /// {3} : is connection SSL secured (included in a category picture URL)
        /// </remarks>
        public const string CATEGORY_HOMEPAGE_KEY = "Grand.pres.category.homepage-{0}-{1}-{2}-{3}";
        public const string CATEGORY_HOMEPAGE_PATTERN_KEY = "Grand.pres.category.homepage";
        public const string CATEGORY_FEATURED_PRODUCTS_HOMEPAGE_KEY = "Grand.pres.category.homepage-fp-{0}-{1}-{2}-{3}";

        /// <summary>
        /// Key for GetChildCategoryIds method results caching
        /// </summary>
        /// <remarks>
        /// {0} : parent category id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY = "Grand.pres.category.childidentifiers-{0}-{1}-{2}";
        public const string CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY = "Grand.pres.category.childidentifiers";

        /// <summary>
        /// Key for SpecificationAttributeOptionFilter caching
        /// </summary>
        /// <remarks>
        /// {0} : comma separated list of specification attribute option IDs
        /// {1} : language id
        /// </remarks>
        public const string SPECS_FILTER_MODEL_KEY = "Grand.pres.filter.specs-{0}-{1}";
        public const string SPECS_FILTER_PATTERN_KEY = "Grand.pres.filter.specs";

        /// <summary>
        /// Key for ProductBreadcrumbModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// {2} : comma separated list of customer roles
        /// {3} : current store ID
        /// </remarks>
        public const string PRODUCT_BREADCRUMB_MODEL_KEY = "Grand.pres.product.breadcrumb-{0}-{1}-{2}-{3}";
        public const string PRODUCT_BREADCRUMB_PATTERN_KEY = "Grand.pres.product.breadcrumb";

        /// <summary>
        /// Key for ProductTagModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// {2} : current store ID
        /// </remarks>
        public const string PRODUCTTAG_BY_PRODUCT_MODEL_KEY = "Grand.pres.producttag.byproduct-{0}-{1}-{2}";
        public const string PRODUCTTAG_BY_PRODUCT_PATTERN_KEY = "Grand.pres.producttag.byproduct";

        /// <summary>
        /// Key for PopularProductTagsModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTTAG_POPULAR_MODEL_KEY = "Grand.pres.producttag.popular-{0}-{1}";
        public const string PRODUCTTAG_POPULAR_PATTERN_KEY = "Grand.pres.producttag.popular";

        /// <summary>
        /// Key for ProductManufacturers model caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// {2} : roles of the current user
        /// {3} : current store ID
        /// </remarks>
        public const string PRODUCT_MANUFACTURERS_MODEL_KEY = "Grand.pres.product.manufacturers-{0}-{1}-{2}-{3}";
        public const string PRODUCT_MANUFACTURERS_PATTERN_KEY = "Grand.pres.product.manufacturers";

        /// <summary>
        /// Key for ProductSpecificationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// </remarks>
        public const string PRODUCT_SPECS_MODEL_KEY = "Grand.pres.product.specs-{0}-{1}";
        public const string PRODUCT_SPECS_PATTERN_KEY = "Grand.pres.product.specs";

        /// <summary>
        /// Key for caching of a value indicating whether a product has product attributes
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// </remarks>
        public const string PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY = "Grand.pres.product.hasproductattributes-{0}";
        public const string PRODUCT_HAS_PRODUCT_ATTRIBUTES_PATTERN_KEY = "Grand.pres.product.hasproductattributes";

        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic system name
        /// {1} : language id
        /// {2} : store id
        /// {3} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_MODEL_BY_SYSTEMNAME_KEY = "Grand.pres.topic.details.bysystemname-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic id
        /// {1} : language id
        /// {2} : store id
        /// {3} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_MODEL_BY_ID_KEY = "Grand.pres.topic.details.byid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic system name
        /// {1} : language id
        /// {2} : store id
        /// </remarks>
        public const string TOPIC_SENAME_BY_SYSTEMNAME = "Grand.pres.topic.sename.bysystemname-{0}-{1}-{2}";
        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// {2} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_TOP_MENU_MODEL_KEY = "Grand.pres.topic.topmenu-{0}-{1}-{2}";
        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// {2} : comma separated list of customer roles
        /// </remarks>
        public const string TOPIC_FOOTER_MODEL_KEY = "Grand.pres.topic.footer-{0}-{1}-{2}";
        public const string TOPIC_PATTERN_KEY = "Grand.pres.topic";

        /// <summary>
        /// Key for CategoryTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : category template id
        /// </remarks>
        public const string CATEGORY_TEMPLATE_MODEL_KEY = "Grand.pres.categorytemplate-{0}";
        public const string CATEGORY_TEMPLATE_PATTERN_KEY = "Grand.pres.categorytemplate";

        /// <summary>
        /// Key for ManufacturerTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer template id
        /// </remarks>
        public const string MANUFACTURER_TEMPLATE_MODEL_KEY = "Grand.pres.manufacturertemplate-{0}";
        public const string MANUFACTURER_TEMPLATE_PATTERN_KEY = "Grand.pres.manufacturertemplate";

        /// <summary>
        /// Key for ProductTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : product template id
        /// </remarks>
        public const string PRODUCT_TEMPLATE_MODEL_KEY = "Grand.pres.producttemplate-{0}";
        public const string PRODUCT_TEMPLATE_PATTERN_KEY = "Grand.pres.producttemplate";

        /// <summary>
        /// Key for TopicTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : topic template id
        /// </remarks>
        public const string TOPIC_TEMPLATE_MODEL_KEY = "Grand.pres.topictemplate-{0}";
        public const string TOPIC_TEMPLATE_PATTERN_KEY = "Grand.pres.topictemplate";

        /// <summary>
        /// Key for bestsellers identifiers displayed on the home page
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// </remarks>
        public const string HOMEPAGE_BESTSELLERS_IDS_KEY = "Grand.pres.bestsellers.homepage-{0}";
        public const string HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY = "Grand.pres.bestsellers.homepage";

        /// <summary>
        /// Key for "also purchased" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTS_ALSO_PURCHASED_IDS_KEY = "Grand.pres.alsopuchased-{0}-{1}";
        public const string PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY = "Grand.pres.alsopuchased";

        /// <summary>
        /// Key for "related" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTS_RELATED_IDS_KEY = "Grand.pres.related-{0}-{1}";
        public const string PRODUCTS_RELATED_IDS_PATTERN_KEY = "Grand.pres.related";

        /// <summary>
        /// Key for "similar" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTS_SIMILAR_IDS_KEY = "Grand.pres.similar-{0}-{1}";
        public const string PRODUCTS_SIMILAR_IDS_PATTERN_KEY = "Grand.pres.similar";

        /// <summary>
        /// Key for default product picture caching (all pictures)
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : picture size
        /// {2} : isAssociatedProduct?
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string PRODUCT_DEFAULTPICTURE_MODEL_KEY = "Grand.pres.product.detailspictures-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string PRODUCT_SECOND_DEFAULTPICTURE_MODEL_KEY = "Grand.pres.product.detailspictures-second-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string PRODUCT_DEFAULTPICTURE_PATTERN_KEY = "Grand.pres.product.detailspictures";

        /// <summary>
        /// Key for product picture caching on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string PRODUCT_DETAILS_PICTURES_MODEL_KEY = "Grand.pres.product.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string PRODUCT_DETAILS_TPICTURES_PATTERN_KEY = "Grand.pres.product.picture";


        /// Key for product reviews caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCT_REVIEWS_MODEL_KEY = "Grand.pres.product.reviews-{0}-{1}";
        /// <summary>
        /// Key for product attribute picture caching on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : picture id
        /// {1} : is connection SSL secured?
        /// {2} : current store ID
        /// </remarks>
        public const string PRODUCTATTRIBUTE_PICTURE_MODEL_KEY = "Grand.pres.productattribute.picture-{0}-{1}-{2}";
        public const string PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY = "Grand.pres.productattribute.picture";

        /// <summary>
        /// Key for product attribute picture caching on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : picture id
        /// {1} : is connection SSL secured?
        /// {2} : current store ID
        /// </remarks>
        public const string PRODUCTATTRIBUTE_IMAGESQUARE_PICTURE_MODEL_KEY = "Grand.pres.productattribute.imagesquare.picture-{0}-{1}-{2}";
        public const string PRODUCTATTRIBUTE_IMAGESQUARE_PICTURE_PATTERN_KEY = "Grand.pres.productattribute.imagesquare.picture";

        /// <summary>
        /// Key for category picture caching
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string CATEGORY_PICTURE_MODEL_KEY = "Grand.pres.category.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string CATEGORY_PICTURE_PATTERN_KEY = "Grand.pres.category.picture";

        /// <summary>
        /// Key for manufacturer picture caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized manufacturer name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string MANUFACTURER_PICTURE_MODEL_KEY = "Grand.pres.manufacturer.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string MANUFACTURER_PICTURE_PATTERN_KEY = "Grand.pres.manufacturer.picture";

        /// <summary>
        /// Key for vendor picture caching
        /// </summary>
        /// <remarks>
        /// {0} : vendor id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string VENDOR_PICTURE_MODEL_KEY = "Grand.pres.vendor.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string VENDOR_PICTURE_PATTERN_KEY = "Grand.pres.vendor.picture";

        /// <summary>
        /// Key for cart picture caching
        /// </summary>
        /// <remarks>
        /// {0} : product Id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string CART_PICTURE_MODEL_KEY = "Grand.pres.cart.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string CART_PICTURE_PATTERN_KEY = "Grand.pres.cart.picture";

        /// <summary>
        /// Key for home page polls
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string HOMEPAGE_POLLS_MODEL_KEY = "Grand.pres.poll.homepage-{0}-{1}";
        /// <summary>
        /// Key for polls by system name
        /// </summary>
        /// <remarks>
        /// {0} : poll system name
        /// {1} : store ID
        /// </remarks>
        public const string POLL_BY_SYSTEMNAME__MODEL_KEY = "Grand.pres.poll.systemname-{0}-{1}";
        public const string POLLS_PATTERN_KEY = "Grand.pres.poll";

        /// <summary>
        /// Key for blog tag list model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string BLOG_TAGS_MODEL_KEY = "Grand.pres.blog.tags-{0}-{1}";
        /// <summary>
        /// Key for blog archive (years, months) block model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string BLOG_MONTHS_MODEL_KEY = "Grand.pres.blog.months-{0}-{1}";
        public const string BLOG_HOMEPAGE_MODEL_KEY = "Grand.pres.blog.homepage-{0}-{1}";
        public const string BLOG_CATEGORY_MODEL_KEY = "Grand.pres.blog.category-{0}-{1}";
        public const string BLOG_PATTERN_KEY = "Grand.pres.blog";

        /// <summary>
        /// Key for blog picture caching
        /// </summary>
        /// <remarks>
        /// {0} : blog id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string BLOG_PICTURE_MODEL_KEY = "Grand.pres.blog.picture-{0}-{1}-{2}-{3}-{4}-{5}";


        /// <summary>
        /// Key for home page news
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string HOMEPAGE_NEWSMODEL_KEY = "Grand.pres.news.homepage-{0}-{1}";
        public const string NEWS_PATTERN_KEY = "Grand.pres.news";

        /// <summary>
        /// Key for news picture caching
        /// </summary>
        /// <remarks>
        /// {0} : blog id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
        /// {5} : current store ID
        /// </remarks>
        public const string NEWS_PICTURE_MODEL_KEY = "Grand.pres.news.picture-{0}-{1}-{2}-{3}-{4}-{5}";


        /// <summary>
        /// Key for states by country id
        /// </summary>
        /// <remarks>
        /// {0} : country ID
        /// {1} : "empty" or "select" item
        /// {2} : language ID
        /// </remarks>
        public const string STATEPROVINCES_BY_COUNTRY_MODEL_KEY = "Grand.pres.stateprovinces.bycountry-{0}-{1}-{2}";
        public const string STATEPROVINCES_PATTERN_KEY = "Grand.pres.stateprovinces";

        /// <summary>
        /// Key for return request reasons
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        public const string RETURNREQUESTREASONS_MODEL_KEY = "Grand.pres.returnrequesreasons-{0}";
        public const string RETURNREQUESTREASONS_PATTERN_KEY = "Grand.pres.returnrequesreasons";

        /// <summary>
        /// Key for return request actions
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        public const string RETURNREQUESTACTIONS_MODEL_KEY = "Grand.pres.returnrequestactions-{0}";
        public const string RETURNREQUESTACTIONS_PATTERN_KEY = "Grand.pres.returnrequestactions";


        /// <summary>
        /// {0} : current store ID
        /// {1} : current theme
        /// {2} : is connection SSL secured (included in a picture URL)
        /// </summary>
        public const string STORE_LOGO_PATH = "Grand.pres.logo-{0}-{1}-{2}";
        public const string STORE_LOGO_PATH_PATTERN_KEY = "Grand.pres.logo";
        /// <summary>
        /// Key for available languages
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// </remarks>
        public const string AVAILABLE_LANGUAGES_MODEL_KEY = "Grand.pres.languages.all-{0}";
        public const string AVAILABLE_LANGUAGES_PATTERN_KEY = "Grand.pres.languages";

        /// <summary>
        /// Key for available stores
        /// </summary>
        public const string AVAILABLE_STORES_MODEL_KEY = "Grand.pres.stores.all";

        /// <summary>
        /// Key for available currencies
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {0} : current store ID
        /// </remarks>
        public const string AVAILABLE_CURRENCIES_MODEL_KEY = "Grand.pres.currencies.all-{0}-{1}";
        public const string AVAILABLE_CURRENCIES_PATTERN_KEY = "Grand.pres.currencies";

        /// <summary>
        /// Key for caching of a value indicating whether we have checkout attributes
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : true - all attributes, false - only shippable attributes
        /// </remarks>
        public const string CHECKOUTATTRIBUTES_EXIST_KEY = "Grand.pres.checkoutattributes.exist-{0}-{1}";
        public const string CHECKOUTATTRIBUTES_PATTERN_KEY = "Grand.pres.checkoutattributes";

        /// <summary>
        /// Key for sitemap on the sitemap page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SITEMAP_PAGE_MODEL_KEY = "Grand.pres.sitemap.page-{0}-{1}-{2}";
        /// <summary>
        /// Key for sitemap on the sitemap SEO page
        /// </summary>
        /// <remarks>
        /// {0} : sitemap identifier
        /// {1} : language id
        /// {2} : roles of the current user
        /// {3} : current store ID
        /// </remarks>
        public const string SITEMAP_SEO_MODEL_KEY = "Grand.pres.sitemap.seo-{0}-{1}-{2}-{3}";
        public const string SITEMAP_PATTERN_KEY = "Grand.pres.sitemap";

        /// <summary>
        /// Key for widget info
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : widget zone
        /// {2} : current theme name
        /// </remarks>
        public const string WIDGET_MODEL_KEY = "Grand.pres.widget-{0}-{1}-{2}";
        public const string WIDGET_PATTERN_KEY = "Grand.pres.widget";

        private readonly ICacheManager _cacheManager;

        public ModelCacheEventConsumer(IServiceProvider serviceProvider)
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        }

        //languages
        public async Task Handle(EntityInserted<Language> eventMessage, CancellationToken cancellationToken)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Language> eventMessage, CancellationToken cancellationToken)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Language> eventMessage, CancellationToken cancellationToken)
        {
            //clear all localizable models
            await _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }

        //currencies
        public async Task Handle(EntityInserted<Currency> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Currency> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Currency> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }

        //stores
        public async Task Handle(EntityInserted<Store> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(AVAILABLE_STORES_MODEL_KEY);
        }
        public async Task Handle(EntityUpdated<Store> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(AVAILABLE_STORES_MODEL_KEY);
        }
        public async Task Handle(EntityDeleted<Store> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(AVAILABLE_STORES_MODEL_KEY);
        }


        //settings
        public async Task Handle(EntityUpdated<Setting> eventMessage, CancellationToken cancellationToken)
        {
            //clear models which depend on settings
            await _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY); //depends on CatalogSettings.NumberOfProductTags
            await _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY); //depends on CatalogSettings.ManufacturersBlockItemsToDisplay
            await _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY); //depends on VendorSettings.VendorBlockItemsToDisplay
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY); //depends on CatalogSettings.ShowCategoryProductNumber and CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            await _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY); //depends on CatalogSettings.NumberOfBestsellersOnHomepage
            await _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY); //depends on CatalogSettings.ProductsAlsoPurchasedNumber
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_SIMILAR_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY); //depends on BlogSettings.NumberOfTags
            await _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY); //depends on NewsSettings.MainPageNewsCount
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY); //depends on distinct sitemap settings
            await _cacheManager.RemoveByPattern(WIDGET_PATTERN_KEY); //depends on WidgetSettings and certain settings of widgets
            await _cacheManager.RemoveByPattern(STORE_LOGO_PATH_PATTERN_KEY); //depends on StoreInformationSettings.LogoPictureId
        }

        //vendors
        public async Task Handle(EntityInserted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Vendor> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(VENDOR_NAVIGATION_PATTERN_KEY);
        }

        //manufacturers
        public async Task Handle(EntityInserted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_MODEL_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_MODEL_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Manufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_MODEL_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product manufacturers
        public async Task Handle(EntityInserted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }

        //categories
        public async Task Handle(EntityInserted<Category> eventMessage, CancellationToken cancellationToken)
        {

            await _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<KnowledgebaseCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(KNOWLEDGEBASE_CATEGORY_BREADCRUMB_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<KnowledgebaseCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(KNOWLEDGEBASE_CATEGORY_BREADCRUMB_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<KnowledgebaseCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(KNOWLEDGEBASE_CATEGORY_BREADCRUMB_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Category> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(SEARCH_CATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product categories

        public async Task Handle(EntityInserted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_BREADCRUMB_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_ALL_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }

        //products
        public async Task Handle(EntityInserted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_SIMILAR_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Product> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_SIMILAR_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product tags
        public async Task Handle(EntityInserted<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductTag> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }

        //related products

        public async Task Handle(EntityInserted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<RelatedProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }

        //similar products
        public async Task Handle(EntityInserted<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_SIMILAR_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_SIMILAR_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<SimilarProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_SIMILAR_IDS_PATTERN_KEY);
        }

        //bundle products

        public async Task Handle(EntityInserted<BundleProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BundleProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BundleProduct> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTS_RELATED_IDS_PATTERN_KEY);
        }

        //specification attributes
        public async Task Handle(EntityUpdated<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<SpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
        }

        //specification attribute options

        public async Task Handle(EntityUpdated<SpecificationAttributeOption> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<SpecificationAttributeOption> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
        }

        //Product specification attribute
        public async Task Handle(EntityInserted<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductSpecificationAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SPECS_FILTER_PATTERN_KEY);
        }

        //Product attributes
        public async Task Handle(EntityDeleted<ProductAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_HAS_PRODUCT_ATTRIBUTES_PATTERN_KEY);
        }

        //Product attributes
        public async Task Handle(EntityInserted<ProductAttributeMapping> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_HAS_PRODUCT_ATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductAttributeMapping> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_HAS_PRODUCT_ATTRIBUTES_PATTERN_KEY);
        }
        //Product attributes
        public async Task Handle(EntityUpdated<ProductAttributeValue> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_IMAGESQUARE_PICTURE_PATTERN_KEY);
        }

        //Topics
        public async Task Handle(EntityInserted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //Orders
        public async Task Handle(EntityInserted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }

        //Pictures
        public async Task Handle(EntityInserted<Picture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(VENDOR_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Picture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(VENDOR_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Picture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_HOMEPAGE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CATEGORY_SUBCATEGORIES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(VENDOR_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }

        //Product picture mappings
        public async Task Handle(EntityInserted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCT_DETAILS_TPICTURES_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(PRODUCTATTRIBUTE_PICTURE_PATTERN_KEY);
            await _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }

        //Polls
        public async Task Handle(EntityInserted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }

        //Blog posts
        public async Task Handle(EntityInserted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }

        //Blog post category
        public async Task Handle(EntityInserted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }

        //News items
        public async Task Handle(EntityInserted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }

        //State/province
        public async Task Handle(EntityInserted<StateProvince> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<StateProvince> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<StateProvince> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }

        //retunr requests
        public async Task Handle(EntityInserted<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(RETURNREQUESTREASONS_PATTERN_KEY);
        }

        //templates
        public async Task Handle(EntityInserted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(TOPIC_TEMPLATE_PATTERN_KEY);
        }

        //checkout attributes
        public async Task Handle(EntityInserted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CHECKOUTATTRIBUTES_PATTERN_KEY);
        }

        //shopping cart items        
        public async Task Handle(EntityUpdated<ShoppingCartItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }

    }
}
