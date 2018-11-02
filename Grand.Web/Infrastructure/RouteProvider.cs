using Grand.Framework.Localization;
using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //areas
            routeBuilder.MapRoute(name: "areaRoute", template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            //home page
            routeBuilder.MapLocalizedRoute("HomePage", "", new { controller = "Home", action = "Index" });

            //widgets
            //we have this route for performance optimization because named routeBuilder are MUCH faster than usual Html.Action(...)
            //and this route is highly used
            routeBuilder.MapRoute("WidgetsByZone",
                            "widgetsbyzone/",
                            new { controller = "Widget", action = "WidgetsByZone" });

            //login
            routeBuilder.MapLocalizedRoute("Login",
                            "login/",
                            new { controller = "Customer", action = "Login" });
            //register
            routeBuilder.MapLocalizedRoute("Register",
                            "register/",
                            new { controller = "Customer", action = "Register" });
            //logout
            routeBuilder.MapLocalizedRoute("Logout",
                            "logout/",
                            new { controller = "Customer", action = "Logout" });

            //shopping cart
            routeBuilder.MapLocalizedRoute("ShoppingCart",
                            "cart/",
                            new { controller = "ShoppingCart", action = "Cart" });

            //Continue shopping
            routeBuilder.MapLocalizedRoute("ContinueShopping",
                "cart/continueshopping/",
                new { controller = "ShoppingCart", action = "ContinueShopping" });

            //clear cart
            routeBuilder.MapLocalizedRoute("ClearCart",
                "cart/clear/",
                new { controller = "ShoppingCart", action = "ClearCart" });

            //start checkout
            routeBuilder.MapLocalizedRoute("StartCheckout",
                "cart/checkout/",
                new { controller = "ShoppingCart", action = "StartCheckout" });

            routeBuilder.MapLocalizedRoute("ApplyDiscountCoupon",
                            "applydiscountcoupon/",
                            new { controller = "ShoppingCart", action = "ApplyDiscountCoupon" });

            routeBuilder.MapLocalizedRoute("RemoveDiscountCoupon",
                            "removediscountcoupon/",
                            new { controller = "ShoppingCart", action = "RemoveDiscountCoupon" });

            routeBuilder.MapLocalizedRoute("ApplyGiftCard",
                            "applygiftcard/",
                            new { controller = "ShoppingCart", action = "ApplyGiftCard" });

            routeBuilder.MapLocalizedRoute("RemoveGiftCardCode",
                "removegiftcardcode/",
                new { controller = "ShoppingCart", action = "RemoveGiftCardCode" });

            routeBuilder.MapLocalizedRoute("UpdateCart",
                "updatecart/",
                new { controller = "ShoppingCart", action = "UpdateCart" });

            //get state list by country ID  (AJAX link)
            routeBuilder.MapRoute("DeleteCartItem",
                            "deletecartitem/{id}",
                            new { controller = "ShoppingCart", action = "DeleteCartItem" });

            //estimate shipping
            routeBuilder.MapLocalizedRoute("EstimateShipping",
                            "cart/estimateshipping",
                            new { controller = "ShoppingCart", action = "GetEstimateShipping" });

            //wishlist
            routeBuilder.MapLocalizedRoute("Wishlist", "wishlist/{customerGuid?}",
                            new { controller = "ShoppingCart", action = "Wishlist" });

            //customer account links
            routeBuilder.MapLocalizedRoute("CustomerInfo",
                            "customer/info",
                            new { controller = "Customer", action = "Info" });
            routeBuilder.MapLocalizedRoute("CustomerAddresses",
                            "customer/addresses",
                            new { controller = "Customer", action = "Addresses" });
            routeBuilder.MapLocalizedRoute("CustomerOrders",
                            "order/history",
                            new { controller = "Order", action = "CustomerOrders" });

            //contact us
            routeBuilder.MapLocalizedRoute("ContactUs",
                            "contactus",
                            new { controller = "Common", action = "ContactUs" });
            //sitemap
            routeBuilder.MapLocalizedRoute("Sitemap",
                            "sitemap",
                            new { controller = "Common", action = "Sitemap" });
            routeBuilder.MapLocalizedRoute("sitemap-indexed.xml", "sitemap-{Id:min(0)}.xml",
                new { controller = "Common", action = "SitemapXml" });

            //interactive form
            routeBuilder.MapLocalizedRoute("PopupInteractiveForm",
                            "popupinteractiveform",
                            new { controller = "Common", action = "PopupInteractiveForm" });

            //product search
            routeBuilder.MapLocalizedRoute("ProductSearch",
                            "search/",
                            new { controller = "Catalog", action = "Search" });
            routeBuilder.MapLocalizedRoute("ProductSearchAutoComplete",
                            "catalog/searchtermautocomplete",
                            new { controller = "Catalog", action = "SearchTermAutoComplete" });

            //change currency (AJAX link)
            routeBuilder.MapLocalizedRoute("ChangeCurrency",
                            "changecurrency/{customercurrency}",
                            new { controller = "Common", action = "SetCurrency" });
            //change language (AJAX link)
            routeBuilder.MapLocalizedRoute("ChangeLanguage",
                            "changelanguage/{langid}",
                            new { controller = "Common", action = "SetLanguage" });
            //change tax (AJAX link)
            routeBuilder.MapLocalizedRoute("ChangeTaxType",
                            "changetaxtype/{customertaxtype}",
                            new { controller = "Common", action = "SetTaxType" });
            //change store (AJAX link)
            routeBuilder.MapLocalizedRoute("ChangeStore",
                            "changestore/{store}",
                            new { controller = "Common", action = "SetStore" });

            //recently viewed products
            routeBuilder.MapLocalizedRoute("RecentlyViewedProducts",
                            "recentlyviewedproducts/",
                            new { controller = "Product", action = "RecentlyViewedProducts" });
            //new products
            routeBuilder.MapLocalizedRoute("NewProducts",
                            "newproducts/",
                            new { controller = "Product", action = "NewProducts" });
            //blog
            routeBuilder.MapLocalizedRoute("Blog",
                            "blog",
                            new { controller = "Blog", action = "List" });

            //knowledgebase
            routeBuilder.MapLocalizedRoute("Knowledgebase",
                            "knowledgebase",
                            new { controller = "Knowledgebase", action = "List" });

            //news
            routeBuilder.MapLocalizedRoute("NewsArchive",
                            "news",
                            new { controller = "News", action = "List" });

            //forum
            routeBuilder.MapLocalizedRoute("Boards",
                            "boards",
                            new { controller = "Boards", action = "Index" });

            //compare products
            routeBuilder.MapLocalizedRoute("CompareProducts",
                            "compareproducts/",
                            new { controller = "Product", action = "CompareProducts" });

            //product tags
            routeBuilder.MapLocalizedRoute("ProductTagsAll",
                            "producttag/all/",
                            new { controller = "Catalog", action = "ProductTagsAll" });

            //manufacturers
            routeBuilder.MapLocalizedRoute("ManufacturerList",
                            "manufacturer/all/",
                            new { controller = "Catalog", action = "ManufacturerAll" });
            //vendors
            routeBuilder.MapLocalizedRoute("VendorList",
                            "vendor/all/",
                            new { controller = "Catalog", action = "VendorAll" });


            //add product to cart (without any attributes and options). used on catalog pages.
            routeBuilder.MapLocalizedRoute("AddProductToCart-Catalog",
                            "addproducttocart/catalog/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddProductToCart_Catalog" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });
            //add product to cart (with attributes and options). used on the product details pages.
            routeBuilder.MapLocalizedRoute("AddProductToCart-Details",
                            "addproducttocart/details/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddProductToCart_Details" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });

            //add product to bid, use on the product details page
            routeBuilder.MapLocalizedRoute("AddBid",
                            "addbid/AddBid/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddBid" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });

            //quick view product.
            routeBuilder.MapLocalizedRoute("QuickView-Product",
                            "quickview/product/{productId}",
                            new { controller = "Product", action = "QuickView" },
                            new { productId = @"\w+" },
                            new[] { "Grand.Web.Controllers" });

            //product tags
            routeBuilder.MapLocalizedRoute("ProductsByTag",
                            "producttag/{productTagId}/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTag" });
            //comparing products
            routeBuilder.MapLocalizedRoute("AddProductToCompare",
                            "compareproducts/add/{productId}",
                            new { controller = "Product", action = "AddProductToCompareList" });
            //product email a friend
            routeBuilder.MapLocalizedRoute("ProductEmailAFriend",
                            "productemailafriend/{productId}",
                            new { controller = "Product", action = "ProductEmailAFriend" });
            //product ask question
            routeBuilder.MapLocalizedRoute("AskQuestion",
                            "askquestion/{productId}",
                            new { controller = "Product", action = "AskQuestion" });

            //product ask question on product page
            routeBuilder.MapLocalizedRoute("AskQuestionOnProduct",
                            "askquestiononproduct",
                            new { controller = "Product", action = "AskQuestionOnProduct" });

            //reviews
            routeBuilder.MapLocalizedRoute("ProductReviews",
                            "productreviews/{productId}",
                            new { controller = "Product", action = "ProductReviews" });
            //back in stock notifications
            routeBuilder.MapLocalizedRoute("BackInStockSubscribePopup",
                            "backinstocksubscribe/{productId}",
                            new { controller = "BackInStockSubscription", action = "SubscribePopup" });
            //downloads
            routeBuilder.MapRoute("GetSampleDownload",
                            "download/sample/{productid}",
                            new { controller = "Download", action = "Sample" });



            //checkout pages
            routeBuilder.MapLocalizedRoute("Checkout",
                            "checkout/",
                            new { controller = "Checkout", action = "Index" });
            routeBuilder.MapLocalizedRoute("CheckoutOnePage",
                            "onepagecheckout/",
                            new { controller = "Checkout", action = "OnePageCheckout" });
            routeBuilder.MapLocalizedRoute("CheckoutShippingAddress",
                            "checkout/shippingaddress",
                            new { controller = "Checkout", action = "ShippingAddress" });
            routeBuilder.MapLocalizedRoute("CheckoutSelectShippingAddress",
                            "checkout/selectshippingaddress",
                            new { controller = "Checkout", action = "SelectShippingAddress" });
            routeBuilder.MapLocalizedRoute("CheckoutBillingAddress",
                            "checkout/billingaddress",
                            new { controller = "Checkout", action = "BillingAddress" });
            routeBuilder.MapLocalizedRoute("CheckoutSelectBillingAddress",
                            "checkout/selectbillingaddress",
                            new { controller = "Checkout", action = "SelectBillingAddress" });
            routeBuilder.MapLocalizedRoute("CheckoutShippingMethod",
                            "checkout/shippingmethod",
                            new { controller = "Checkout", action = "ShippingMethod" });
            routeBuilder.MapLocalizedRoute("CheckoutPaymentMethod",
                            "checkout/paymentmethod",
                            new { controller = "Checkout", action = "PaymentMethod" });
            routeBuilder.MapLocalizedRoute("CheckoutPaymentInfo",
                            "checkout/paymentinfo",
                            new { controller = "Checkout", action = "PaymentInfo" });
            routeBuilder.MapLocalizedRoute("CheckoutConfirm",
                            "checkout/confirm",
                            new { controller = "Checkout", action = "Confirm" });
            routeBuilder.MapLocalizedRoute("CheckoutCompleted",
                            "checkout/completed/{orderId}",
                            new { controller = "Checkout", action = "Completed" });

            //subscribe newsletters
            routeBuilder.MapLocalizedRoute("SubscribeNewsletter",
                            "subscribenewsletter",
                            new { controller = "Newsletter", action = "SubscribeNewsletter" });

            //assign newsletters to categories
            routeBuilder.MapLocalizedRoute("SubscribeNewsletterCategory",
                "newsletter/savecategories",
                new { controller = "Newsletter", action = "SaveCategories" });

            //email wishlist
            routeBuilder.MapLocalizedRoute("EmailWishlist",
                            "emailwishlist",
                            new { controller = "ShoppingCart", action = "EmailWishlist" });

            //login page for checkout as guest
            routeBuilder.MapLocalizedRoute("LoginCheckoutAsGuest",
                            "login/checkoutasguest",
                            new { controller = "Customer", action = "Login", checkoutAsGuest = true });

            //register result page
            routeBuilder.MapLocalizedRoute("RegisterResult",
                            "registerresult/{resultId}",
                            new { controller = "Customer", action = "RegisterResult" });
            //check username availability
            routeBuilder.MapLocalizedRoute("CheckUsernameAvailability",
                            "customer/checkusernameavailability",
                            new { controller = "Customer", action = "CheckUsernameAvailability" });

            //passwordrecovery
            routeBuilder.MapLocalizedRoute("PasswordRecovery",
                            "passwordrecovery",
                            new { controller = "Customer", action = "PasswordRecovery" });
            //password recovery confirmation
            routeBuilder.MapLocalizedRoute("PasswordRecoveryConfirm",
                            "passwordrecovery/confirm",
                            new { controller = "Customer", action = "PasswordRecoveryConfirm" });

            //topics
            routeBuilder.MapLocalizedRoute("TopicPopup",
                            "t-popup/{SystemName}",
                            new { controller = "Topic", action = "TopicDetailsPopup" });

            //blog
            routeBuilder.MapLocalizedRoute("BlogByTag",
                            "blog/tag/{tag}",
                            new { controller = "Blog", action = "BlogByTag" });
            routeBuilder.MapLocalizedRoute("BlogByMonth",
                            "blog/month/{month}",
                            new { controller = "Blog", action = "BlogByMonth" });
            //blog RSS
            routeBuilder.MapLocalizedRoute("BlogRSS",
                            "blog/rss/{languageId}",
                            new { controller = "Blog", action = "ListRss" });

            //news RSS
            routeBuilder.MapLocalizedRoute("NewsRSS",
                            "news/rss/{languageId}",
                            new { controller = "News", action = "ListRss" });

            //set review helpfulness (AJAX link)
            routeBuilder.MapRoute("SetProductReviewHelpfulness",
                            "setproductreviewhelpfulness",
                            new { controller = "Product", action = "SetProductReviewHelpfulness" });

            //customer account links
            routeBuilder.MapLocalizedRoute("CustomerReturnRequests",
                            "returnrequest/history",
                            new { controller = "ReturnRequest", action = "CustomerReturnRequests" });
            routeBuilder.MapLocalizedRoute("CustomerDownloadableProducts",
                            "customer/downloadableproducts",
                            new { controller = "Customer", action = "DownloadableProducts" });
            routeBuilder.MapLocalizedRoute("CustomerBackInStockSubscriptions",
                            "backinstocksubscriptions/manage",
                            new { controller = "BackInStockSubscription", action = "CustomerSubscriptions" });
            routeBuilder.MapLocalizedRoute("CustomerBackInStockSubscriptionsPaged",
                            "backinstocksubscriptions/manage/{pageNumber}",
                            new { controller = "BackInStockSubscription", action = "CustomerSubscriptions" });
            routeBuilder.MapLocalizedRoute("CustomerRewardPoints",
                            "rewardpoints/history",
                            new { controller = "Order", action = "CustomerRewardPoints" });
            routeBuilder.MapLocalizedRoute("CustomerChangePassword",
                            "customer/changepassword",
                            new { controller = "Customer", action = "ChangePassword" });
            routeBuilder.MapLocalizedRoute("CustomerDeleteAccount",
                            "customer/deleteaccount",
                            new { controller = "Customer", action = "DeleteAccount" });
            routeBuilder.MapLocalizedRoute("CustomerAvatar",
                            "customer/avatar",
                            new { controller = "Customer", action = "Avatar" });
            routeBuilder.MapLocalizedRoute("CustomerAuctions",
                            "customer/auctions",
                            new { controller = "Customer", action = "Auctions" });
            routeBuilder.MapLocalizedRoute("CustomerNotes",
                            "customer/notes",
                            new { controller = "Customer", action = "Notes" });
            routeBuilder.MapLocalizedRoute("AccountActivation",
                            "customer/activation",
                            new { controller = "Customer", action = "AccountActivation" });
            routeBuilder.MapLocalizedRoute("CustomerForumSubscriptions",
                            "boards/forumsubscriptions",
                            new { controller = "Boards", action = "CustomerForumSubscriptions" });
            routeBuilder.MapLocalizedRoute("CustomerForumSubscriptionsPaged",
                            "boards/forumsubscriptions/{pageNumber}",
                            new { controller = "Boards", action = "CustomerForumSubscriptions" });
            routeBuilder.MapLocalizedRoute("CustomerAddressEdit",
                            "customer/addressedit/{addressId}",
                            new { controller = "Customer", action = "AddressEdit" });
            routeBuilder.MapLocalizedRoute("CustomerAddressAdd",
                            "customer/addressadd",
                            new { controller = "Customer", action = "AddressAdd" });
            //customer profile page
            routeBuilder.MapLocalizedRoute("CustomerProfile",
                            "profile/{id}",
                            new { controller = "Profile", action = "Index" });

            routeBuilder.MapLocalizedRoute("CustomerProfilePaged",
                            "profile/{id}/page/{pageNumber}",
                            new { controller = "Profile", action = "Index" });

            //orders
            routeBuilder.MapLocalizedRoute("OrderDetails",
                            "orderdetails/{orderId}",
                            new { controller = "Order", action = "Details" });
            routeBuilder.MapLocalizedRoute("ShipmentDetails",
                            "orderdetails/shipment/{shipmentId}",
                            new { controller = "Order", action = "ShipmentDetails" });
            routeBuilder.MapLocalizedRoute("ReturnRequest",
                            "returnrequest/{orderId}",
                            new { controller = "ReturnRequest", action = "ReturnRequest" });
            routeBuilder.MapLocalizedRoute("ReturnRequestDetails",
                "returnrequestdetails/{returnRequestId}",
                new { controller = "ReturnRequest", action = "ReturnRequestDetails" });
            routeBuilder.MapLocalizedRoute("ReOrder",
                            "reorder/{orderId}",
                            new { controller = "Order", action = "ReOrder" });
            routeBuilder.MapLocalizedRoute("GetOrderPdfInvoice",
                            "orderdetails/pdf/{orderId}",
                            new { controller = "Order", action = "GetPdfInvoice" });
            routeBuilder.MapLocalizedRoute("PrintOrderDetails",
                            "orderdetails/print/{orderId}",
                            new { controller = "Order", action = "PrintOrderDetails" });
            routeBuilder.MapLocalizedRoute("CancelOrder",
                            "orderdetails/cancel/{orderId}",
                            new { controller = "Order", action = "CancelOrder" });
            routeBuilder.MapLocalizedRoute("AddOrderNote",
                            "orderdetails/ordernote/{orderId}",
                            new { controller = "Order", action = "AddOrderNote" });

            //order downloads
            routeBuilder.MapRoute("GetDownload",
                            "download/getdownload/{orderItemId}/{agree?}",
                            new { controller = "Download", action = "GetDownload" });

            routeBuilder.MapRoute("GetLicense",
                            "download/getlicense/{orderItemId}/",
                            new { controller = "Download", action = "GetLicense" });
            routeBuilder.MapLocalizedRoute("DownloadUserAgreement",
                            "customer/useragreement/{orderItemId}",
                            new { controller = "Customer", action = "UserAgreement" });
            routeBuilder.MapRoute("GetOrderNoteFile",
                            "download/ordernotefile/{ordernoteid}",
                            new { controller = "Download", action = "GetOrderNoteFile" });
            routeBuilder.MapRoute("GetCustomerNoteFile",
                            "download/customernotefile/{customernoteid}",
                            new { controller = "Download", action = "GetCustomerNoteFile" });

            //contact vendor
            routeBuilder.MapLocalizedRoute("ContactVendor",
                            "contactvendor/{vendorId}",
                            new { controller = "Common", action = "ContactVendor" });

            //apply for vendor account
            routeBuilder.MapLocalizedRoute("ApplyVendorAccount",
                            "vendor/apply",
                            new { controller = "Vendor", action = "ApplyVendor" });

            //vendor info
            routeBuilder.MapLocalizedRoute("CustomerVendorInfo", "customer/vendorinfo",
                new { controller = "Vendor", action = "Info" });


            //vendor reviews
            routeBuilder.MapLocalizedRoute("VendorReviews", "vendoreviews/{vendorId}",
                new { controller = "Catalog", action = "VendorReviews" });

            //set review helpfulness (AJAX link)
            routeBuilder.MapRoute("SetVendorReviewHelpfulness", "setvendorreviewhelpfulness",
                new { controller = "Catalog", action = "SetVendorReviewHelpfulness" });

            //poll vote AJAX link
            routeBuilder.MapLocalizedRoute("PollVote",
                            "poll/vote",
                            new { controller = "Poll", action = "Vote" });

            //comparing products
            routeBuilder.MapLocalizedRoute("RemoveProductFromCompareList",
                            "compareproducts/remove/{productId}",
                            new { controller = "Product", action = "RemoveProductFromCompareList" });
            routeBuilder.MapLocalizedRoute("ClearCompareList",
                            "clearcomparelist/",
                            new { controller = "Product", action = "ClearCompareList" });

            //new RSS
            routeBuilder.MapLocalizedRoute("NewProductsRSS",
                            "newproducts/rss",
                            new { controller = "Product", action = "NewProductsRss" });

            //get state list by country ID  (AJAX link)
            routeBuilder.MapRoute("GetStatesByCountryId",
                            "country/getstatesbycountryid/",
                            new { controller = "Country", action = "GetStatesByCountryId" });

            //EU Cookie law accept button handler (AJAX link)
            routeBuilder.MapRoute("EuCookieLawAccept",
                            "eucookielawaccept",
                            new { controller = "Common", action = "EuCookieLawAccept" });

            //authenticate topic AJAX link
            routeBuilder.MapLocalizedRoute("TopicAuthenticate",
                            "topic/authenticate",
                            new { controller = "Topic", action = "Authenticate" });

            //product attributes with "upload file" type
            routeBuilder.MapLocalizedRoute("UploadFileProductAttribute",
                            "uploadfileproductattribute/{attributeId}",
                            new { controller = "Product", action = "UploadFileProductAttribute" });
            //checkout attributes with "upload file" type
            routeBuilder.MapLocalizedRoute("UploadFileCheckoutAttribute",
                            "uploadfilecheckoutattribute/{attributeId}",
                            new { controller = "ShoppingCart", action = "UploadFileCheckoutAttribute" });

            // contact attributes with "upload file" type
            routeBuilder.MapLocalizedRoute("UploadFileContactAttribute",
                            "uploadfilecontactattribute/{attributeId}",
                            new { controller = "Common", action = "UploadFileContactAttribute" });

            //forums
            routeBuilder.MapLocalizedRoute("ActiveDiscussions",
                            "boards/activediscussions",
                            new { controller = "Boards", action = "ActiveDiscussions" });
            routeBuilder.MapLocalizedRoute("ActiveDiscussionsPaged",
                            "boards/activediscussions/page/{pageNumber}",
                            new { controller = "Boards", action = "ActiveDiscussions" });
            routeBuilder.MapLocalizedRoute("ActiveDiscussionsRSS",
                            "boards/activediscussionsrss",
                            new { controller = "Boards", action = "ActiveDiscussionsRSS" });
            routeBuilder.MapLocalizedRoute("PostEdit",
                            "boards/postedit/{id}",
                            new { controller = "Boards", action = "PostEdit" });
            routeBuilder.MapLocalizedRoute("PostDelete",
                            "boards/postdelete/{id}",
                            new { controller = "Boards", action = "PostDelete" });
            routeBuilder.MapLocalizedRoute("PostCreate",
                            "boards/postcreate/{id}",
                            new { controller = "Boards", action = "PostCreate" });
            routeBuilder.MapLocalizedRoute("PostCreateQuote",
                            "boards/postcreate/{id}/{quote}",
                            new { controller = "Boards", action = "PostCreate" });
            routeBuilder.MapLocalizedRoute("TopicEdit",
                            "boards/topicedit/{id}",
                            new { controller = "Boards", action = "TopicEdit" });
            routeBuilder.MapLocalizedRoute("TopicDelete",
                            "boards/topicdelete/{id}",
                            new { controller = "Boards", action = "TopicDelete" });
            routeBuilder.MapLocalizedRoute("TopicCreate",
                            "boards/topiccreate/{id}",
                            new { controller = "Boards", action = "TopicCreate" });
            routeBuilder.MapLocalizedRoute("TopicMove",
                            "boards/topicmove/{id}",
                            new { controller = "Boards", action = "TopicMove" });
            routeBuilder.MapLocalizedRoute("TopicWatch",
                            "boards/topicwatch/{id}",
                            new { controller = "Boards", action = "TopicWatch" });
            routeBuilder.MapLocalizedRoute("TopicSlug",
                            "boards/topic/{id}/{slug}",
                            new { controller = "Boards", action = "Topic" });
            routeBuilder.MapLocalizedRoute("TopicSlugPaged",
                            "boards/topic/{id}/{slug}/page/{pageNumber}",
                            new { controller = "Boards", action = "Topic" });
            routeBuilder.MapLocalizedRoute("ForumWatch",
                            "boards/forumwatch/{id}",
                            new { controller = "Boards", action = "ForumWatch" });
            routeBuilder.MapLocalizedRoute("ForumRSS",
                            "boards/forumrss/{id}",
                            new { controller = "Boards", action = "ForumRSS" });
            routeBuilder.MapLocalizedRoute("ForumSlug",
                            "boards/forum/{id}/{slug}",
                            new { controller = "Boards", action = "Forum" });
            routeBuilder.MapLocalizedRoute("ForumSlugPaged",
                            "boards/forum/{id}/{slug}/page/{pageNumber}",
                            new { controller = "Boards", action = "Forum" });
            routeBuilder.MapLocalizedRoute("ForumGroupSlug",
                            "boards/forumgroup/{id}/{slug}",
                            new { controller = "Boards", action = "ForumGroup" });
            routeBuilder.MapLocalizedRoute("Search",
                            "boards/search",
                            new { controller = "Boards", action = "Search" });

            //private messages
            routeBuilder.MapLocalizedRoute("PrivateMessages",
                            "privatemessages/{tab?}",
                            new { controller = "PrivateMessages", action = "Index" });
            routeBuilder.MapLocalizedRoute("PrivateMessagesPaged",
                            "privatemessages/{tab}/page/{pageNumber}",
                            new { controller = "PrivateMessages", action = "Index" });
            routeBuilder.MapLocalizedRoute("PrivateMessagesInbox",
                            "inboxupdate",
                            new { controller = "PrivateMessages", action = "InboxUpdate" });
            routeBuilder.MapLocalizedRoute("PrivateMessagesSent",
                            "sentupdate",
                            new { controller = "PrivateMessages", action = "SentUpdate" });
            routeBuilder.MapLocalizedRoute("SendPM",
                            "sendpm/{toCustomerId}",
                            new { controller = "PrivateMessages", action = "SendPM" });
            routeBuilder.MapLocalizedRoute("SendPMReply",
                            "sendpm/{toCustomerId}/{replyToMessageId}",
                            new { controller = "PrivateMessages", action = "SendPM" });
            routeBuilder.MapLocalizedRoute("ViewPM",
                            "viewpm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "ViewPM" });
            routeBuilder.MapLocalizedRoute("DeletePM",
                            "deletepm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "DeletePM" });

            //activate newsletters
            routeBuilder.MapLocalizedRoute("NewsletterActivation",
                            "newsletter/subscriptionactivation/{token:guid}/{active}",
                            new { controller = "Newsletter", action = "SubscriptionActivation" });

            //robots.txt
            routeBuilder.MapRoute("robots.txt", "robots.txt",
                            new { controller = "Common", action = "RobotsTextFile" });

            //sitemap (XML)
            routeBuilder.MapLocalizedRoute("sitemap.xml", "sitemap.xml",
                            new { controller = "Common", action = "SitemapXml" });

            //store closed
            routeBuilder.MapLocalizedRoute("StoreClosed", "storeclosed",
                            new { controller = "Common", action = "StoreClosed" });

            //install
            routeBuilder.MapRoute("Installation", "install",
                            new { controller = "Install", action = "Index" });
            //upgrade
            routeBuilder.MapRoute("Upgrade", "upgrade",
                            new { controller = "Upgrade", action = "Index" });

            //page not found
            routeBuilder.MapLocalizedRoute("PageNotFound", "page-not-found",
                            new { controller = "Common", action = "PageNotFound" });

            //push notifications
            routeBuilder.MapRoute(
               "PushNotifications.Send",
               "Admin/PushNotifications/Send",
            new { controller = "PushNotifications", action = "Send" });

            routeBuilder.MapRoute(
                "PushNotifications.Messages",
                "Admin/PushNotifications/Messages",
            new { controller = "PushNotifications", action = "Messages" });

            routeBuilder.MapRoute(
               "PushNotifications.Receivers",
               "Admin/PushNotifications/Receivers",
            new { controller = "PushNotifications", action = "Receivers" });

            routeBuilder.MapRoute(
                "PushNotifications.DeleteReceiver",
                "Admin/PushNotifications/DeleteReceiver",
                new { controller = "PushNotifications", action = "DeleteReceiver" });

            routeBuilder.MapRoute(
                "PushNotifications.Configure",
                "Admin/PushNotifications/Configure",
                new { controller = "PushNotifications", action = "Configure" });

            routeBuilder.MapRoute(
                "PushNotifications.PushMessagesList",
                "Admin/PushNotifications/PushMessagesList",
            new { controller = "PushNotifications", action = "PushMessagesList" });

            routeBuilder.MapRoute(
                "PushNotifications.PushReceiversList",
                "Admin/PushNotifications/PushReceiversList",
            new { controller = "PushNotifications", action = "PushReceiversList" });
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
