using Grand.Core.Domain.Localization;
using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Web.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            var pattern = "";
            var localizationSettings = routeBuilder.ServiceProvider.GetRequiredService<LocalizationSettings>();
            if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                pattern = "{language:lang=en}/";

            //areas
            routeBuilder.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            //home page
            routeBuilder.MapControllerRoute("HomePage", pattern, new { controller = "Home", action = "Index" });

            //widgets
            //we have this route for performance optimization because named routeBuilder are MUCH faster than usual Html.Action(...)
            //and this route is highly used
            routeBuilder.MapControllerRoute("WidgetsByZone",
                            $"{pattern}widgetsbyzone/",
                            new { controller = "Widget", action = "WidgetsByZone" });

            //login
            routeBuilder.MapControllerRoute("Login",
                            $"{pattern}login/",
                            new { controller = "Customer", action = "Login" });
            //register
            routeBuilder.MapControllerRoute("Register",
                            $"{pattern}register/",
                            new { controller = "Customer", action = "Register" });
            //logout
            routeBuilder.MapControllerRoute("Logout",
                            $"{pattern}logout/",
                            new { controller = "Customer", action = "Logout" });

            //shopping cart
            routeBuilder.MapControllerRoute("ShoppingCart",
                            $"{pattern}cart/",
                            new { controller = "ShoppingCart", action = "Cart" });

            //Continue shopping
            routeBuilder.MapControllerRoute("ContinueShopping",
                $"{pattern}cart/continueshopping/",
                new { controller = "ShoppingCart", action = "ContinueShopping" });

            //clear cart
            routeBuilder.MapControllerRoute("ClearCart",
                $"{pattern}cart/clear/",
                new { controller = "ShoppingCart", action = "ClearCart" });

            //start checkout
            routeBuilder.MapControllerRoute("StartCheckout",
                $"{pattern}cart/checkout/",
                new { controller = "ShoppingCart", action = "StartCheckout" });

            routeBuilder.MapControllerRoute("ApplyDiscountCoupon",
                            $"{pattern}applydiscountcoupon/",
                            new { controller = "ShoppingCart", action = "ApplyDiscountCoupon" });

            routeBuilder.MapControllerRoute("RemoveDiscountCoupon",
                            $"{pattern}removediscountcoupon/",
                            new { controller = "ShoppingCart", action = "RemoveDiscountCoupon" });

            routeBuilder.MapControllerRoute("ApplyGiftCard",
                            $"{pattern}applygiftcard/",
                            new { controller = "ShoppingCart", action = "ApplyGiftCard" });

            routeBuilder.MapControllerRoute("RemoveGiftCardCode",
                $"{pattern}removegiftcardcode/",
                new { controller = "ShoppingCart", action = "RemoveGiftCardCode" });

            routeBuilder.MapControllerRoute("UpdateCart",
                $"{pattern}updatecart/",
                new { controller = "ShoppingCart", action = "UpdateCart" });

            //get state list by country ID  (AJAX link)
            routeBuilder.MapControllerRoute("DeleteCartItem",
                            pattern + "deletecartitem/{id}",
                            new { controller = "ShoppingCart", action = "DeleteCartItem" });

            //estimate shipping
            routeBuilder.MapControllerRoute("EstimateShipping",
                            $"{pattern}cart/estimateshipping",
                            new { controller = "ShoppingCart", action = "GetEstimateShipping" });

            //wishlist
            routeBuilder.MapControllerRoute("Wishlist", pattern + "wishlist/{customerGuid?}",
                            new { controller = "ShoppingCart", action = "Wishlist" });

            //customer account links
            routeBuilder.MapControllerRoute("CustomerInfo",
                            "customer/info",
                            new { controller = "Customer", action = "Info" });
            routeBuilder.MapControllerRoute("CustomerAddresses",
                            "customer/addresses",
                            new { controller = "Customer", action = "Addresses" });
            routeBuilder.MapControllerRoute("CustomerOrders",
                            "order/history",
                            new { controller = "Order", action = "CustomerOrders" });

            //contact us
            routeBuilder.MapControllerRoute("ContactUs",
                            "contactus",
                            new { controller = "Common", action = "ContactUs" });
            //sitemap
            routeBuilder.MapControllerRoute("Sitemap",
                            "sitemap",
                            new { controller = "Common", action = "Sitemap" });
            routeBuilder.MapControllerRoute("sitemap-indexed.xml", "sitemap-{Id:min(0)}.xml",
                new { controller = "Common", action = "SitemapXml" });

            //interactive form
            routeBuilder.MapControllerRoute("PopupInteractiveForm",
                            "popupinteractiveform",
                            new { controller = "Common", action = "PopupInteractiveForm" });

            //product search
            routeBuilder.MapControllerRoute("ProductSearch",
                            "search/",
                            new { controller = "Catalog", action = "Search" });
            routeBuilder.MapControllerRoute("ProductSearchAutoComplete",
                            "catalog/searchtermautocomplete",
                            new { controller = "Catalog", action = "SearchTermAutoComplete" });

            //change currency (AJAX link)
            routeBuilder.MapControllerRoute("ChangeCurrency",
                            "changecurrency/{customercurrency}",
                            new { controller = "Common", action = "SetCurrency" });
            //change language (AJAX link)
            routeBuilder.MapControllerRoute("ChangeLanguage",
                            "changelanguage/{langid}",
                            new { controller = "Common", action = "SetLanguage" });
            //change tax (AJAX link)
            routeBuilder.MapControllerRoute("ChangeTaxType",
                            "changetaxtype/{customertaxtype}",
                            new { controller = "Common", action = "SetTaxType" });
            //change store (AJAX link)
            routeBuilder.MapControllerRoute("ChangeStore",
                            "changestore/{store}",
                            new { controller = "Common", action = "SetStore" });

            //recently viewed products
            routeBuilder.MapControllerRoute("RecentlyViewedProducts",
                            "recentlyviewedproducts/",
                            new { controller = "Product", action = "RecentlyViewedProducts" });
            //new products
            routeBuilder.MapControllerRoute("NewProducts",
                            "newproducts/",
                            new { controller = "Product", action = "NewProducts" });
            //blog
            routeBuilder.MapControllerRoute("Blog",
                            "blog",
                            new { controller = "Blog", action = "List" });

            //knowledgebase
            routeBuilder.MapControllerRoute("Knowledgebase",
                            "knowledgebase",
                            new { controller = "Knowledgebase", action = "List" });

            //news
            routeBuilder.MapControllerRoute("NewsArchive",
                            "news",
                            new { controller = "News", action = "List" });

            //forum
            routeBuilder.MapControllerRoute("Boards",
                            "boards",
                            new { controller = "Boards", action = "Index" });

            //compare products
            routeBuilder.MapControllerRoute("CompareProducts",
                            "compareproducts/",
                            new { controller = "Product", action = "CompareProducts" });

            //product tags
            routeBuilder.MapControllerRoute("ProductTagsAll",
                            "producttag/all/",
                            new { controller = "Catalog", action = "ProductTagsAll" });

            //manufacturers
            routeBuilder.MapControllerRoute("ManufacturerList",
                            "manufacturer/all/",
                            new { controller = "Catalog", action = "ManufacturerAll" });
            //vendors
            routeBuilder.MapControllerRoute("VendorList",
                            "vendor/all/",
                            new { controller = "Catalog", action = "VendorAll" });


            //add product to cart (without any attributes and options). used on catalog pages.
            routeBuilder.MapControllerRoute("AddProductToCart-Catalog",
                            "addproducttocart/catalog/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddProductToCart_Catalog" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });
            //add product to cart (with attributes and options). used on the product details pages.
            routeBuilder.MapControllerRoute("AddProductToCart-Details",
                            "addproducttocart/details/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddProductToCart_Details" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });

            //add product to bid, use on the product details page
            routeBuilder.MapControllerRoute("AddBid",
                            "addbid/AddBid/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddBid" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });

            //quick view product.
            routeBuilder.MapControllerRoute("QuickView-Product",
                            "quickview/product/{productId}",
                            new { controller = "Product", action = "QuickView" },
                            new { productId = @"\w+" },
                            new[] { "Grand.Web.Controllers" });

            //product tags
            routeBuilder.MapControllerRoute("ProductsByTag",
                            "producttag/{productTagId}/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTag" });

            routeBuilder.MapControllerRoute("ProductsByTagName",
                            "producttag/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTagName" });

            //comparing products
            routeBuilder.MapControllerRoute("AddProductToCompare",
                            "compareproducts/add/{productId}",
                            new { controller = "Product", action = "AddProductToCompareList" });
            //product email a friend
            routeBuilder.MapControllerRoute("ProductEmailAFriend",
                            "productemailafriend/{productId}",
                            new { controller = "Product", action = "ProductEmailAFriend" });
            //product ask question
            routeBuilder.MapControllerRoute("AskQuestion",
                            "askquestion/{productId}",
                            new { controller = "Product", action = "AskQuestion" });

            //product ask question on product page
            routeBuilder.MapControllerRoute("AskQuestionOnProduct",
                            "askquestiononproduct",
                            new { controller = "Product", action = "AskQuestionOnProduct" });

            //reviews
            routeBuilder.MapControllerRoute("ProductReviews",
                            "productreviews/{productId}",
                            new { controller = "Product", action = "ProductReviews" });

            //back in stock notifications
            routeBuilder.MapControllerRoute("BackInStockSubscribePopup",
                            "backinstocksubscribe/{productId}",
                            new { controller = "BackInStockSubscription", action = "SubscribePopup" });

            //back in stock notifications button text
            routeBuilder.MapControllerRoute("BackInStockSubscribeButton",
                            "backinstocksubscribebutton/{productId}",
                            new { controller = "BackInStockSubscription", action = "SubscribeButton" });

            //downloads
            routeBuilder.MapControllerRoute("GetSampleDownload",
                            "download/sample/{productid}",
                            new { controller = "Download", action = "Sample" });



            //checkout pages
            routeBuilder.MapControllerRoute("Checkout",
                            "checkout/",
                            new { controller = "Checkout", action = "Index" });
            routeBuilder.MapControllerRoute("CheckoutOnePage",
                            "onepagecheckout/",
                            new { controller = "Checkout", action = "OnePageCheckout" });
            routeBuilder.MapControllerRoute("CheckoutShippingAddress",
                            "checkout/shippingaddress",
                            new { controller = "Checkout", action = "ShippingAddress" });
            routeBuilder.MapControllerRoute("CheckoutSelectShippingAddress",
                            "checkout/selectshippingaddress",
                            new { controller = "Checkout", action = "SelectShippingAddress" });
            routeBuilder.MapControllerRoute("CheckoutBillingAddress",
                            "checkout/billingaddress",
                            new { controller = "Checkout", action = "BillingAddress" });
            routeBuilder.MapControllerRoute("CheckoutSelectBillingAddress",
                            "checkout/selectbillingaddress",
                            new { controller = "Checkout", action = "SelectBillingAddress" });
            routeBuilder.MapControllerRoute("CheckoutShippingMethod",
                            "checkout/shippingmethod",
                            new { controller = "Checkout", action = "ShippingMethod" });
            routeBuilder.MapControllerRoute("CheckoutPaymentMethod",
                            "checkout/paymentmethod",
                            new { controller = "Checkout", action = "PaymentMethod" });
            routeBuilder.MapControllerRoute("CheckoutPaymentInfo",
                            "checkout/paymentinfo",
                            new { controller = "Checkout", action = "PaymentInfo" });
            routeBuilder.MapControllerRoute("CheckoutConfirm",
                            "checkout/confirm",
                            new { controller = "Checkout", action = "Confirm" });
            routeBuilder.MapControllerRoute("CheckoutCompleted",
                            "checkout/completed/{orderId}",
                            new { controller = "Checkout", action = "Completed" });

            //subscribe newsletters
            routeBuilder.MapControllerRoute("SubscribeNewsletter",
                            "subscribenewsletter",
                            new { controller = "Newsletter", action = "SubscribeNewsletter" });

            //assign newsletters to categories
            routeBuilder.MapControllerRoute("SubscribeNewsletterCategory",
                "newsletter/savecategories",
                new { controller = "Newsletter", action = "SaveCategories" });

            //email wishlist
            routeBuilder.MapControllerRoute("EmailWishlist",
                            "emailwishlist",
                            new { controller = "ShoppingCart", action = "EmailWishlist" });

            //login page for checkout as guest
            routeBuilder.MapControllerRoute("LoginCheckoutAsGuest",
                            "login/checkoutasguest",
                            new { controller = "Customer", action = "Login", checkoutAsGuest = true });

            //register result page
            routeBuilder.MapControllerRoute("RegisterResult",
                            "registerresult/{resultId}",
                            new { controller = "Customer", action = "RegisterResult" });
            //check username availability
            routeBuilder.MapControllerRoute("CheckUsernameAvailability",
                            "customer/checkusernameavailability",
                            new { controller = "Customer", action = "CheckUsernameAvailability" });

            //passwordrecovery
            routeBuilder.MapControllerRoute("PasswordRecovery",
                            "passwordrecovery",
                            new { controller = "Customer", action = "PasswordRecovery" });
            //password recovery confirmation
            routeBuilder.MapControllerRoute("PasswordRecoveryConfirm",
                            "passwordrecovery/confirm",
                            new { controller = "Customer", action = "PasswordRecoveryConfirm" });

            //topics
            routeBuilder.MapControllerRoute("TopicPopup",
                            "t-popup/{SystemName}",
                            new { controller = "Topic", action = "TopicDetailsPopup" });

            //blog
            routeBuilder.MapControllerRoute("BlogByTag",
                            "blog/tag/{tag}",
                            new { controller = "Blog", action = "BlogByTag" });
            routeBuilder.MapControllerRoute("BlogByMonth",
                            "blog/month/{month}",
                            new { controller = "Blog", action = "BlogByMonth" });
            routeBuilder.MapControllerRoute("BlogByCategory",
                            "blog/category/{categoryid}",
                            new { controller = "Blog", action = "BlogByCategory" });
            routeBuilder.MapControllerRoute("BlogByKeyword",
                            "blog/keyword/{searchKeyword}",
                            new { controller = "Blog", action = "BlogByKeyword" });
            //blog RSS
            routeBuilder.MapControllerRoute("BlogRSS",
                            "blog/rss/{languageId}",
                            new { controller = "Blog", action = "ListRss" });

            //news RSS
            routeBuilder.MapControllerRoute("NewsRSS",
                            "news/rss/{languageId}",
                            new { controller = "News", action = "ListRss" });

            //set review helpfulness (AJAX link)
            routeBuilder.MapControllerRoute("SetProductReviewHelpfulness",
                            "setproductreviewhelpfulness",
                            new { controller = "Product", action = "SetProductReviewHelpfulness" });

            //customer account links
            routeBuilder.MapControllerRoute("CustomerReturnRequests",
                            "returnrequest/history",
                            new { controller = "ReturnRequest", action = "CustomerReturnRequests" });
            routeBuilder.MapControllerRoute("CustomerDownloadableProducts",
                            "customer/downloadableproducts",
                            new { controller = "Customer", action = "DownloadableProducts" });
            routeBuilder.MapControllerRoute("CustomerBackInStockSubscriptions",
                            "backinstocksubscriptions/manage",
                            new { controller = "BackInStockSubscription", action = "CustomerSubscriptions" });
            routeBuilder.MapControllerRoute("CustomerBackInStockSubscriptionsPaged",
                            "backinstocksubscriptions/manage/{pageNumber}",
                            new { controller = "BackInStockSubscription", action = "CustomerSubscriptions" });
            routeBuilder.MapControllerRoute("CustomerRewardPoints",
                            "rewardpoints/history",
                            new { controller = "Order", action = "CustomerRewardPoints" });
            routeBuilder.MapControllerRoute("CustomerChangePassword",
                            "customer/changepassword",
                            new { controller = "Customer", action = "ChangePassword" });
            routeBuilder.MapControllerRoute("CustomerDeleteAccount",
                            "customer/deleteaccount",
                            new { controller = "Customer", action = "DeleteAccount" });
            routeBuilder.MapControllerRoute("CustomerAvatar",
                            "customer/avatar",
                            new { controller = "Customer", action = "Avatar" });
            routeBuilder.MapControllerRoute("CustomerAuctions",
                            "customer/auctions",
                            new { controller = "Customer", action = "Auctions" });
            routeBuilder.MapControllerRoute("CustomerNotes",
                            "customer/notes",
                            new { controller = "Customer", action = "Notes" });
            routeBuilder.MapControllerRoute("CustomerDocuments",
                            "customer/documents",
                            new { controller = "Customer", action = "Documents" });
            routeBuilder.MapControllerRoute("CustomerCourses",
                            "customer/courses",
                            new { controller = "Customer", action = "Courses" });
            routeBuilder.MapControllerRoute("AccountActivation",
                            "customer/activation",
                            new { controller = "Customer", action = "AccountActivation" });
            routeBuilder.MapControllerRoute("CustomerReviews",
                            "customer/reviews",
                            new { controller = "Customer", action = "Reviews" });
            routeBuilder.MapControllerRoute("CustomerForumSubscriptions",
                            "boards/forumsubscriptions",
                            new { controller = "Boards", action = "CustomerForumSubscriptions" });
            routeBuilder.MapControllerRoute("CustomerForumSubscriptionsPaged",
                            "boards/forumsubscriptions/{pageNumber}",
                            new { controller = "Boards", action = "CustomerForumSubscriptions" });
            routeBuilder.MapControllerRoute("CustomerAddressEdit",
                            "customer/addressedit/{addressId}",
                            new { controller = "Customer", action = "AddressEdit" });
            routeBuilder.MapControllerRoute("CustomerAddressAdd",
                            "customer/addressadd",
                            new { controller = "Customer", action = "AddressAdd" });
            //customer profile page
            routeBuilder.MapControllerRoute("CustomerProfile",
                            "profile/{id}",
                            new { controller = "Profile", action = "Index" });

            routeBuilder.MapControllerRoute("CustomerProfilePaged",
                            "profile/{id}/page/{pageNumber}",
                            new { controller = "Profile", action = "Index" });

            //orders
            routeBuilder.MapControllerRoute("OrderDetails",
                            "orderdetails/{orderId}",
                            new { controller = "Order", action = "Details" });
            routeBuilder.MapControllerRoute("ShipmentDetails",
                            "orderdetails/shipment/{shipmentId}",
                            new { controller = "Order", action = "ShipmentDetails" });
            routeBuilder.MapControllerRoute("ReturnRequest",
                            "returnrequest/{orderId}",
                            new { controller = "ReturnRequest", action = "ReturnRequest" });
            routeBuilder.MapControllerRoute("ReturnRequestDetails",
                "returnrequestdetails/{returnRequestId}",
                new { controller = "ReturnRequest", action = "ReturnRequestDetails" });
            routeBuilder.MapControllerRoute("ReOrder",
                            "reorder/{orderId}",
                            new { controller = "Order", action = "ReOrder" });
            routeBuilder.MapControllerRoute("GetOrderPdfInvoice",
                            "orderdetails/pdf/{orderId}",
                            new { controller = "Order", action = "GetPdfInvoice" });
            routeBuilder.MapControllerRoute("PrintOrderDetails",
                            "orderdetails/print/{orderId}",
                            new { controller = "Order", action = "PrintOrderDetails" });
            routeBuilder.MapControllerRoute("CancelOrder",
                            "orderdetails/cancel/{orderId}",
                            new { controller = "Order", action = "CancelOrder" });
            routeBuilder.MapControllerRoute("AddOrderNote",
                            "orderdetails/ordernote/{orderId}",
                            new { controller = "Order", action = "AddOrderNote" });

            //order downloads
            routeBuilder.MapControllerRoute("GetDownload",
                            "download/getdownload/{orderItemId}/{agree?}",
                            new { controller = "Download", action = "GetDownload" });

            routeBuilder.MapControllerRoute("GetLicense",
                            "download/getlicense/{orderItemId}/",
                            new { controller = "Download", action = "GetLicense" });
            routeBuilder.MapControllerRoute("DownloadUserAgreement",
                            "customer/useragreement/{orderItemId}",
                            new { controller = "Customer", action = "UserAgreement" });
            routeBuilder.MapControllerRoute("GetOrderNoteFile",
                            "download/ordernotefile/{ordernoteid}",
                            new { controller = "Download", action = "GetOrderNoteFile" });
            routeBuilder.MapControllerRoute("GetCustomerNoteFile",
                            "download/customernotefile/{customernoteid}",
                            new { controller = "Download", action = "GetCustomerNoteFile" });
            routeBuilder.MapControllerRoute("GetDocumentFile",
                           "download/documentfile/{documentid}",
                           new { controller = "Download", action = "GetDocumentFile" });
            //contact vendor
            routeBuilder.MapControllerRoute("ContactVendor",
                            "contactvendor/{vendorId}",
                            new { controller = "Common", action = "ContactVendor" });

            //apply for vendor account
            routeBuilder.MapControllerRoute("ApplyVendorAccount",
                            "vendor/apply",
                            new { controller = "Vendor", action = "ApplyVendor" });

            //vendor info
            routeBuilder.MapControllerRoute("CustomerVendorInfo", "customer/vendorinfo",
                new { controller = "Vendor", action = "Info" });


            //vendor reviews
            routeBuilder.MapControllerRoute("VendorReviews", "vendoreviews/{vendorId}",
                new { controller = "Catalog", action = "VendorReviews" });

            //set review helpfulness (AJAX link)
            routeBuilder.MapControllerRoute("SetVendorReviewHelpfulness", "setvendorreviewhelpfulness",
                new { controller = "Catalog", action = "SetVendorReviewHelpfulness" });

            //poll vote AJAX link
            routeBuilder.MapControllerRoute("PollVote",
                            "poll/vote",
                            new { controller = "Poll", action = "Vote" });

            //comparing products
            routeBuilder.MapControllerRoute("RemoveProductFromCompareList",
                            "compareproducts/remove/{productId}",
                            new { controller = "Product", action = "RemoveProductFromCompareList" });
            routeBuilder.MapControllerRoute("ClearCompareList",
                            "clearcomparelist/",
                            new { controller = "Product", action = "ClearCompareList" });

            //new RSS
            routeBuilder.MapControllerRoute("NewProductsRSS",
                            "newproducts/rss",
                            new { controller = "Product", action = "NewProductsRss" });

            //get state list by country ID  (AJAX link)
            routeBuilder.MapControllerRoute("GetStatesByCountryId",
                            "country/getstatesbycountryid/",
                            new { controller = "Country", action = "GetStatesByCountryId" });

            //EU Cookie law accept button handler (AJAX link)
            routeBuilder.MapControllerRoute("EuCookieLawAccept",
                            "eucookielawaccept",
                            new { controller = "Common", action = "EuCookieLawAccept" });

            //authenticate topic AJAX link
            routeBuilder.MapControllerRoute("TopicAuthenticate",
                            "topic/authenticate",
                            new { controller = "Topic", action = "Authenticate" });

            //product attributes with "upload file" type
            routeBuilder.MapControllerRoute("UploadFileProductAttribute",
                            "uploadfileproductattribute/{attributeId}",
                            new { controller = "Product", action = "UploadFileProductAttribute" });
            //checkout attributes with "upload file" type
            routeBuilder.MapControllerRoute("UploadFileCheckoutAttribute",
                            "uploadfilecheckoutattribute/{attributeId}",
                            new { controller = "ShoppingCart", action = "UploadFileCheckoutAttribute" });

            // contact attributes with "upload file" type
            routeBuilder.MapControllerRoute("UploadFileContactAttribute",
                            "uploadfilecontactattribute/{attributeId}",
                            new { controller = "Common", action = "UploadFileContactAttribute" });

            //forums
            routeBuilder.MapControllerRoute("ActiveDiscussions",
                            "boards/activediscussions",
                            new { controller = "Boards", action = "ActiveDiscussions" });
            routeBuilder.MapControllerRoute("ActiveDiscussionsPaged",
                            "boards/activediscussions/page/{pageNumber}",
                            new { controller = "Boards", action = "ActiveDiscussions" });
            routeBuilder.MapControllerRoute("ActiveDiscussionsRSS",
                            "boards/activediscussionsrss",
                            new { controller = "Boards", action = "ActiveDiscussionsRSS" });
            routeBuilder.MapControllerRoute("PostEdit",
                            "boards/postedit/{id}",
                            new { controller = "Boards", action = "PostEdit" });
            routeBuilder.MapControllerRoute("PostDelete",
                            "boards/postdelete/{id}",
                            new { controller = "Boards", action = "PostDelete" });
            routeBuilder.MapControllerRoute("PostCreate",
                            "boards/postcreate/{id}",
                            new { controller = "Boards", action = "PostCreate" });
            routeBuilder.MapControllerRoute("PostCreateQuote",
                            "boards/postcreate/{id}/{quote}",
                            new { controller = "Boards", action = "PostCreate" });
            routeBuilder.MapControllerRoute("TopicEdit",
                            "boards/topicedit/{id}",
                            new { controller = "Boards", action = "TopicEdit" });
            routeBuilder.MapControllerRoute("TopicDelete",
                            "boards/topicdelete/{id}",
                            new { controller = "Boards", action = "TopicDelete" });
            routeBuilder.MapControllerRoute("TopicCreate",
                            "boards/topiccreate/{id}",
                            new { controller = "Boards", action = "TopicCreate" });
            routeBuilder.MapControllerRoute("TopicMove",
                            "boards/topicmove/{id}",
                            new { controller = "Boards", action = "TopicMove" });
            routeBuilder.MapControllerRoute("TopicWatch",
                            "boards/topicwatch/{id}",
                            new { controller = "Boards", action = "TopicWatch" });
            routeBuilder.MapControllerRoute("TopicSlug",
                            "boards/topic/{id}/{slug}",
                            new { controller = "Boards", action = "Topic" });
            routeBuilder.MapControllerRoute("TopicSlugPaged",
                            "boards/topic/{id}/{slug}/page/{pageNumber}",
                            new { controller = "Boards", action = "Topic" });
            routeBuilder.MapControllerRoute("ForumWatch",
                            "boards/forumwatch/{id}",
                            new { controller = "Boards", action = "ForumWatch" });
            routeBuilder.MapControllerRoute("ForumRSS",
                            "boards/forumrss/{id}",
                            new { controller = "Boards", action = "ForumRSS" });
            routeBuilder.MapControllerRoute("ForumSlug",
                            "boards/forum/{id}/{slug}",
                            new { controller = "Boards", action = "Forum" });
            routeBuilder.MapControllerRoute("ForumSlugPaged",
                            "boards/forum/{id}/{slug}/page/{pageNumber}",
                            new { controller = "Boards", action = "Forum" });
            routeBuilder.MapControllerRoute("ForumGroupSlug",
                            "boards/forumgroup/{id}/{slug}",
                            new { controller = "Boards", action = "ForumGroup" });
            routeBuilder.MapControllerRoute("Search",
                            "boards/search",
                            new { controller = "Boards", action = "Search" });

            //private messages
            routeBuilder.MapControllerRoute("PrivateMessages",
                            "privatemessages/{tab?}",
                            new { controller = "PrivateMessages", action = "Index" });
            routeBuilder.MapControllerRoute("PrivateMessagesPaged",
                            "privatemessages/{tab}/page/{pageNumber}",
                            new { controller = "PrivateMessages", action = "Index" });
            routeBuilder.MapControllerRoute("PrivateMessagesInbox",
                            "inboxupdate",
                            new { controller = "PrivateMessages", action = "InboxUpdate" });
            routeBuilder.MapControllerRoute("PrivateMessagesSent",
                            "sentupdate",
                            new { controller = "PrivateMessages", action = "SentUpdate" });
            routeBuilder.MapControllerRoute("SendPM",
                            "sendpm/{toCustomerId}",
                            new { controller = "PrivateMessages", action = "SendPM" });
            routeBuilder.MapControllerRoute("SendPMReply",
                            "sendpm/{toCustomerId}/{replyToMessageId}",
                            new { controller = "PrivateMessages", action = "SendPM" });
            routeBuilder.MapControllerRoute("ViewPM",
                            "viewpm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "ViewPM" });
            routeBuilder.MapControllerRoute("DeletePM",
                            "deletepm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "DeletePM" });

            //activate newsletters
            routeBuilder.MapControllerRoute("NewsletterActivation",
                            "newsletter/subscriptionactivation/{token:guid}/{active}",
                            new { controller = "Newsletter", action = "SubscriptionActivation" });

            //robots.txt
            routeBuilder.MapControllerRoute("robots.txt", "robots.txt",
                            new { controller = "Common", action = "RobotsTextFile" });

            //sitemap (XML)
            routeBuilder.MapControllerRoute("sitemap.xml", "sitemap.xml",
                            new { controller = "Common", action = "SitemapXml" });

            //store closed
            routeBuilder.MapControllerRoute("StoreClosed", "storeclosed",
                            new { controller = "Common", action = "StoreClosed" });

            //install
            routeBuilder.MapControllerRoute("Installation", "install",
                            new { controller = "Install", action = "Index" });
            //upgrade
            routeBuilder.MapControllerRoute("Upgrade", "upgrade",
                            new { controller = "Upgrade", action = "Index" });

            //page not found
            routeBuilder.MapControllerRoute("PageNotFound", "page-not-found",
                            new { controller = "Common", action = "PageNotFound" });

            //lets encrypt
            routeBuilder.MapControllerRoute("well-known", ".well-known/acme-challenge/{fileName}",
                new { controller = "LetsEncrypt", action = "Index" });

        }

        public int Priority {
            get {
                return 0;
            }
        }
    }
}
