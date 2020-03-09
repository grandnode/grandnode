﻿using Grand.Core.Configuration;
using System.Collections.Generic;

namespace Grand.Core.Domain.Catalog
{
    public class CatalogSettings : ISettings
    {
        public CatalogSettings()
        {
            ProductSortingEnumDisabled = new List<int>();
            ProductSortingEnumDisplayOrder= new Dictionary<int, int>();
        }

        /// <summary>
        /// Gets or sets a value indicating details pages of unpublished product details pages could be open (for SEO optimization)
        /// </summary>
        public bool AllowViewUnpublishedProductPage { get; set; }
        /// <summary>
        /// Gets or sets a value indicating customers should see "discontinued" message when visibting details pages of unpublished products (if "AllowViewUnpublishedProductPage" is "true)
        /// </summary>
        public bool DisplayDiscontinuedMessageForUnpublishedProducts { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether "Published" or "Disable buy/wishlist buttons" flags should be updated after order cancellation (deletion).
        /// Of course, when qty > configured minimum stock level
        /// </summary>
        public bool PublishBackProductWhenCancellingOrders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display product SKU on product details page
        /// </summary>
        public bool ShowSkuOnProductDetailsPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display product SKU on catalog pages
        /// </summary>
        public bool ShowSkuOnCatalogPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display specification attribute on catalog pages
        /// </summary>
        public bool ShowSpecAttributeOnCatalogPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate second picture on catalog pages
        /// </summary>
        public bool SecondPictureOnCatalogPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display manufacturer part number of a product
        /// </summary>
        public bool ShowManufacturerPartNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display GTIN of a product
        /// </summary>
        public bool ShowGtin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Free shipping" icon should be displayed for products
        /// </summary>
        public bool ShowFreeShippingNotification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product sorting is enabled
        /// </summary>
        public bool AllowProductSorting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change product view mode
        /// </summary>
        public bool AllowProductViewModeChanging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change product view mode
        /// </summary>
        public string DefaultViewMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a category details page should include products from subcategories
        /// </summary>
        public bool ShowProductsFromSubcategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a search box  should include products from subcategories
        /// </summary>
        public bool ShowProductsFromSubcategoriesInSearchBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether number of products should be displayed beside each category
        /// </summary>
        public bool ShowCategoryProductNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we include subcategories (when 'ShowCategoryProductNumber' is 'true')
        /// </summary>
        public bool ShowCategoryProductNumberIncludingSubcategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether category breadcrumb is enabled
        /// </summary>
        public bool CategoryBreadcrumbEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether a 'Share button' is enabled
        /// </summary>
        public bool ShowShareButton { get; set; }

        /// <summary>
        /// Gets or sets a share code (e.g. AddThis button code)
        /// </summary>
        public string PageShareCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating product reviews must be approved
        /// </summary>
        public bool ProductReviewsMustBeApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default rating value of the product reviews
        /// </summary>
        public int DefaultProductRatingValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow anonymous users write product reviews.
        /// </summary>
        public bool AllowAnonymousUsersToReviewProduct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notification of a store owner about new product reviews is enabled
        /// </summary>
        public bool NotifyStoreOwnerAboutNewProductReviews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product reviews will be filtered per store
        /// </summary>
        public bool ShowProductReviewsPerStore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product 'Email a friend' feature is enabled
        /// </summary>
        public bool EmailAFriendEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'ask product question' feature is enabled
        /// </summary>
        public bool AskQuestionEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'contact us on the product page' feature is enabled
        /// </summary>
        public bool AskQuestionOnProduct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow anonymous users to email a friend.
        /// </summary>
        public bool AllowAnonymousUsersToEmailAFriend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product can be reviewed only by customer who have already ordered it
        /// </summary>
        public bool ProductReviewPossibleOnlyAfterPurchasing { get; set; }
        /// <summary>
        /// Gets or sets a number of "Recently viewed products"
        /// </summary>
        public int RecentlyViewedProductsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Recently viewed products" feature is enabled
        /// </summary>
        public bool RecentlyViewedProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Recommended products" feature is enabled
        /// </summary>
        public bool RecommendedProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Suggested products" feature is enabled
        /// </summary>
        public bool SuggestedProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a number of "Suggested products"
        /// </summary>
        public int SuggestedProductsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Personalized products" feature is enabled
        /// </summary>
        public bool PersonalizedProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a number of "Personalized products"
        /// </summary>
        public int PersonalizedProductsNumber { get; set; }

        /// <summary>
        /// Gets or sets a number of products on the "New products" page
        /// </summary>
        public int NewProductsNumber { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether "New products" page is enabled
        /// </summary>
        public bool NewProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "New products" is show on home page
        /// </summary>
        public bool NewProductsOnHomePage { get; set; }
        
        /// <summary>
        /// Gets or sets a number of products on the "New products" on home page
        /// </summary>
        public int NewProductsNumberOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Compare products" feature is enabled
        /// </summary>
        public bool CompareProductsEnabled { get; set; }

        /// <summary>
        /// Gets or sets an allowed number of products to be compared
        /// </summary>
        public int CompareProductsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether autocomplete is enabled
        /// </summary>
        public bool ProductSearchAutoCompleteEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether search by sku is enabled
        /// </summary>
        public bool SearchBySku { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether search by description is enabled
        /// </summary>
        public bool SearchByDescription { get; set; }

        /// <summary>
        /// Gets or sets a number of products to return when using "autocomplete" feature
        /// </summary>
        public int ProductSearchAutoCompleteNumberOfProducts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show product images in the auto complete search
        /// </summary>
        public bool ShowProductImagesInSearchAutoComplete { get; set; }

       
        /// <summary>
        /// Gets or sets a minimum search term length
        /// </summary>
        public int ProductSearchTermMinimumLength { get; set; }

        /// <summary>
        /// Gets or sets save search autocomplete
        /// </summary>
        public bool SaveSearchAutoComplete { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to show bestsellers on home page
        /// </summary>
        public bool ShowBestsellersOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets a number of bestsellers on home page
        /// </summary>
        public int NumberOfBestsellersOnHomepage { get; set; }

        /// <summary>
        /// Gets or sets a number of time period for bestsellers on home page
        /// </summary>
        public int PeriodBestsellers { get; set; }

        /// <summary>
        /// Gets or sets a number of review on product page
        /// </summary>
        public int NumberOfReview { get; set; }

        /// <summary>
        /// Gets or sets a number of products per page on the search products page
        /// </summary>
        public int SearchPageProductsPerPage { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to select page size on the search products page
        /// </summary>
        public bool SearchPageAllowCustomersToSelectPageSize { get; set; }
        /// <summary>
        /// Gets or sets the available customer selectable page size options on the search products page
        /// </summary>
        public string SearchPagePageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets "List of products purchased by other customers who purchased the above" option is enable
        /// </summary>
        public bool ProductsAlsoPurchasedEnabled { get; set; }

        /// <summary>
        /// Gets or sets a number of products also purchased by other customers to display
        /// </summary>
        public int ProductsAlsoPurchasedNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should process attribute change using AJAX. It's used for dynamical attribute change, SKU/GTIN update of combinations, conditional attributes
        /// </summary>
        public bool AjaxProcessAttributeChange { get; set; }

        /// <summary>
        /// Gets or sets a number of product tags that appear in the tag cloud
        /// </summary>
        public int NumberOfProductTags { get; set; }

        /// <summary>
        /// Gets or sets a number of products per page on 'products by tag' page
        /// </summary>
        public int ProductsByTagPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size for 'products by tag'
        /// </summary>
        public bool ProductsByTagAllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options for 'products by tag'
        /// </summary>
        public string ProductsByTagPageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include "Short description" in compare products
        /// </summary>
        public bool IncludeShortDescriptionInCompareProducts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include "Full description" in compare products
        /// </summary>
        public bool IncludeFullDescriptionInCompareProducts { get; set; }
        
        /// <summary>
        /// An option indicating whether products on category and manufacturer pages should include featured products as well
        /// </summary>
        public bool IncludeFeaturedProductsInNormalLists { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether tier prices should be displayed with applied discounts (if available)
        /// </summary>
        public bool DisplayTierPricesWithDiscounts { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to ignore discounts (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreDiscounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore featured products (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreFeaturedProducts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore ACL rules (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreAcl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore "limit per store" rules (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreStoreLimitations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use customer product prices. It can significantly improve performance when disable.
        /// </summary>
        public bool CustomerProductPrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore load Filterable Specification Attribute Option (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreFilterableSpecAttributeOption { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to ignore load Filterable available start and end date products (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreFilterableAvailableStartEndDateTime { get; set; }
        /// <summary>
        /// Gets or sets a value indicating maximum number of 'back in stock' subscription
        /// </summary>
        public int MaximumBackInStockSubscriptions { get; set; }

        /// <summary>
        /// Gets or sets the value indicating how many manufacturers to display in manufacturers block
        /// </summary>
        public int ManufacturersBlockItemsToDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display information about shipping and tax in the footer (used in Germany)
        /// </summary>
        public bool DisplayTaxShippingInfoFooter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display information about shipping and tax on product details pages (used in Germany)
        /// </summary>
        public bool DisplayTaxShippingInfoProductDetailsPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display information about shipping and tax in product boxes (used in Germany)
        /// </summary>
        public bool DisplayTaxShippingInfoProductBoxes { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to display information about shipping and tax on shopping cart page (used in Germany)
        /// </summary>
        public bool DisplayTaxShippingInfoShoppingCart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display information about shipping and tax on wishlist page (used in Germany)
        /// </summary>
        public bool DisplayTaxShippingInfoWishlist { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display information about shipping and tax on order details page (used in Germany)
        /// </summary>
        public bool DisplayTaxShippingInfoOrderDetailsPage { get; set; }


        /// <summary>
        /// Gets or sets the default value to use for Category page size options (for new categories)
        /// </summary>
        public string DefaultCategoryPageSizeOptions { get; set; }
        /// <summary>
        /// Gets or sets the default value to use for Category page size (for new categories)
        /// </summary>
        public int DefaultCategoryPageSize { get; set; }
        /// <summary>
        /// Gets or sets the default value to use for Manufacturer page size options (for new manufacturers)
        /// </summary>
        public string DefaultManufacturerPageSizeOptions { get; set; }
        /// <summary>
        /// Gets or sets the default value to use for Manufacturer page size (for new manufacturers)
        /// </summary>
        public int DefaultManufacturerPageSize { get; set; }
        /// <summary>
        /// Gets or sets the default value to use show extra field quantity on catalog pages
        /// </summary>
        public bool DisplayQuantityOnCatalogPages { get; set; }
        /// <summary>
        /// Limit of featured products
        /// </summary>
        public int LimitOfFeaturedProducts { get; set; }
        /// <summary>
        /// Gets or sets a list of disabled values of ProductSortingEnum
        /// </summary>
        public List<int> ProductSortingEnumDisabled { get; set; }

        /// <summary>
        /// Gets or sets a display order of ProductSortingEnum values 
        /// </summary>
        public Dictionary<int, int> ProductSortingEnumDisplayOrder { get; set; }
    }
}