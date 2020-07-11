
namespace Grand.Web.Infrastructure.Cache
{
    public static class ModelCacheEventConst
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

        
        /// <summary>
        /// Key for caching of manufacturer displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : customer role
        /// {1} : store ID
        /// {2} : language ID
        /// </remarks>
        public const string MANUFACTURER_FEATURED_PRODUCT_HOMEPAGE_KEY = "Grand.pres.manufacturer.navigation.homepage-fp-{0}-{1}-{2}";

        /// <summary>
        /// Key for List of ManufacturerModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : comma separated list of customer roles
        /// {2} : current store ID
        /// </remarks>
        public const string MANUFACTURER_ALL_MODEL_KEY = "Grand.pres.manufacturer.navigation.all-{0}-{1}-{2}";

        /// <summary>
        /// Key for VendorNavigationModel caching
        /// </summary>
        public const string VENDOR_NAVIGATION_MODEL_KEY = "Grand.pres.vendor.navigation";
        public const string VENDOR_NAVIGATION_PATTERN_KEY = "Grand.pres.vendor.navigation";

        /// <summary>
        /// Key for List of VendorModel caching
        /// </summary>
        public const string VENDOR_ALL_MODEL_KEY = "Grand.pres.vendor.navigation.all";

        /// <summary>
        /// Key for caching of a value indicating whether a manufacturer has featured products
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY = "Grand.pres.manufacturer.hasfeaturedproducts-{0}-{1}-{2}";
        public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY = "Grand.pres.manufacturer.hasfeaturedproducts-{0}";
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
        /// Key for caching of a value indicating whether a category has featured products
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string CATEGORY_HAS_FEATURED_PRODUCTS_KEY = "Grand.pres.category.hasfeaturedproducts-{0}-{1}-{2}";
        public const string CATEGORY_HAS_FEATURED_PRODUCTS_MODEL_KEY = "Grand.pres.category.hasfeaturedproducts-{0}";

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

        public const string CATEGORY_SUBCATEGORIES_PATTERN_KEY = "Grand.pres.category.subcategories";

        /// <summary>
        /// Key for caching of categories displayed on home page
        /// </summary>
        /// <remarks>
        /// {0} : roles of the current user
        /// {1} : current store ID
        /// {2} : language ID
        /// </remarks>
        public const string CATEGORY_HOMEPAGE_KEY = "Grand.pres.category.homepage-{0}-{1}-{2}";
        public const string CATEGORY_HOMEPAGE_PATTERN_KEY = "Grand.pres.category.homepage";
        public const string CATEGORY_FEATURED_PRODUCTS_HOMEPAGE_KEY = "Grand.pres.category.homepage-fp-{0}-{1}-{2}";

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
        public const string PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY = "Grand.pres.product.manufacturers-{0}";
        public const string PRODUCT_MANUFACTURERS_PATTERN_KEY = "Grand.pres.product.manufacturers";

        /// <summary>
        /// Key for ProductSpecificationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// </remarks>
        public const string PRODUCT_SPECS_MODEL_KEY = "Grand.pres.product.specs-{0}-{1}";
        public const string PRODUCT_SPECS_PATTERN_KEY = "Grand.pres.product.specs-{0}";
        public const string PRODUCT_SPECS_PATTERN = "Grand.pres.product.specs";

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
        public const string PRODUCTS_ALSO_PURCHASED_IDS_KEY = "Grand.pres.alsopurchased-{0}-{1}";
        public const string PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY = "Grand.pres.alsopurchased";

        /// <summary>
        /// Key for "related" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTS_RELATED_IDS_KEY = "Grand.pres.related-{0}-{1}";
        public const string PRODUCTS_RELATED_IDS_PATTERN_KEY = "Grand.pres.related-{0}";
        public const string PRODUCTS_RELATED_IDS_PATTERN = "Grand.pres.related";

        /// <summary>
        /// Key for "similar" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCTS_SIMILAR_IDS_KEY = "Grand.pres.similar-{0}-{1}";
        public const string PRODUCTS_SIMILAR_IDS_PATTERN_KEY = "Grand.pres.similar-{0}";
        public const string PRODUCTS_SIMILAR_IDS_PATTERN = "Grand.pres.similar";


        /// <summary>
        /// Key for product picture caching on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : current store ID
        /// </remarks>
        public const string PRODUCT_DETAILS_PICTURES_MODEL_KEY = "Grand.pres.product.picture-{0}-{1}-{2}-{3}-{4}";
        public const string PRODUCT_DETAILS_PICTURES_PATTERN_KEY = "Grand.pres.product.picture-{0}";


        /// Key for product reviews caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : current store ID
        /// </remarks>
        public const string PRODUCT_REVIEWS_MODEL_KEY = "Grand.pres.product.reviews-{0}-{1}";


        /// <summary>
        /// Key for cart picture caching
        /// </summary>
        /// <remarks>
        /// {0} : product Id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : current store ID
        /// </remarks>
        public const string CART_PICTURE_MODEL_KEY = "Grand.pres.cart.picture-{0}-{1}-{2}-{3}-{4}";
        public const string CART_PICTURE_PATTERN_KEY = "Grand.pres.cart.picture-{0}";


        /// <summary>
        /// Key for picture caching
        /// </summary>
        /// <remarks>
        /// {0} : picture Id
        /// </remarks>
        public const string PICTURE_PATTERN_KEY = "Grand.picture-{0}";

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
        /// {4} : current store ID
        /// </remarks>
        public const string NEWS_PICTURE_MODEL_KEY = "Grand.pres.news.picture-{0}-{1}-{2}-{3}-{4}";

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
        /// </summary>
        public const string STORE_LOGO_PATH = "Grand.pres.logo-{0}-{1}";
        public const string STORE_LOGO_PATH_PATTERN_KEY = "Grand.pres.logo";
        
        /// <summary>
        /// Key for available stores
        /// </summary>
        public const string AVAILABLE_STORES_MODEL_KEY = "Grand.pres.stores.all";

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
    }
}
