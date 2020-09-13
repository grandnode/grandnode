using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Domain.Localization;
using Grand.Framework.Mvc.Routing;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Grand.Web.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            var pattern = "";
            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                var config = routeBuilder.ServiceProvider.GetRequiredService<GrandConfig>();
                if (config.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    pattern = $"{{language:lang={config.SeoFriendlyUrlsDefaultCode}}}/";
                }
            }

            //areas
            routeBuilder.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            //home page
            routeBuilder.MapControllerRoute("HomePage", pattern, new { controller = "Home", action = "Index" });


            RegisterCustomerRoute(routeBuilder, pattern);

            RegisterVendorRoute(routeBuilder, pattern);

            RegisterCartRoute(routeBuilder, pattern);

            RegisterOrderRoute(routeBuilder, pattern);

            RegisterReturnRequestRoute(routeBuilder, pattern);

            RegisterCommonRoute(routeBuilder, pattern);

            RegisterCatalogRoute(routeBuilder, pattern);

            RegisterProductRoute(routeBuilder, pattern);

            RegisterCmsRoute(routeBuilder, pattern);

            RegisterBoardsRoute(routeBuilder, pattern);

            RegisterPrivateMessagesRoute(routeBuilder, pattern);

            RegisterBlogRoute(routeBuilder, pattern);

            RegisterNewsletterRoute(routeBuilder, pattern);

            RegisterAddToCartRoute(routeBuilder, pattern);

            RegisterBackInStockSubscriptionRoute(routeBuilder, pattern);

            RegisterCheckoutRoute(routeBuilder, pattern);

            RegisterDownloadRoute(routeBuilder, pattern);

            RegisterTopicRoute(routeBuilder, pattern);

            RegisterInstallRoute(routeBuilder, pattern);

        }

        public int Priority {
            get {
                return 0;
            }
        }

        private void RegisterCustomerRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //login
            routeBuilder.MapControllerRoute("Login",
                            $"{pattern}login/",
                            new { controller = "Customer", action = "Login" });

            // two factor authorization digit code page
            routeBuilder.MapControllerRoute("TwoFactorAuthorization",
                            "two-factor-authorization",
                            new { controller = "Customer", action = "TwoFactorAuthorization" });

            //register
            routeBuilder.MapControllerRoute("Register",
                            $"{pattern}register/",
                            new { controller = "Customer", action = "Register" });
            //logout
            routeBuilder.MapControllerRoute("Logout",
                            $"{pattern}logout/",
                            new { controller = "Customer", action = "Logout" });

            //customer account links
            routeBuilder.MapControllerRoute("CustomerInfo",
                            pattern + "customer/info",
                            new { controller = "Customer", action = "Info" });

            // enable two factor authorization digit code page
            routeBuilder.MapControllerRoute("EnableTwoFactorAuthorization",
                           "customer/enable-two-factor-authorization",
                           new { controller = "Customer", action = "EnableTwoFactorAuthenticator" });

            routeBuilder.MapControllerRoute("CustomerAddresses",
                            pattern + "customer/addresses",
                            new { controller = "Customer", action = "Addresses" });

            //login page for checkout as guest
            routeBuilder.MapControllerRoute("LoginCheckoutAsGuest",
                            pattern + "login/checkoutasguest",
                            new { controller = "Customer", action = "Login", checkoutAsGuest = true });

            //register result page
            routeBuilder.MapControllerRoute("RegisterResult",
                            pattern + "registerresult/{resultId}",
                            new { controller = "Customer", action = "RegisterResult" });

            //check username availability
            routeBuilder.MapControllerRoute("CheckUsernameAvailability",
                            pattern + "customer/checkusernameavailability",
                            new { controller = "Customer", action = "CheckUsernameAvailability" });

            //passwordrecovery
            routeBuilder.MapControllerRoute("PasswordRecovery",
                            pattern + "passwordrecovery",
                            new { controller = "Customer", action = "PasswordRecovery" });

            //password recovery confirmation
            routeBuilder.MapControllerRoute("PasswordRecoveryConfirm",
                            pattern + "passwordrecovery/confirm",
                            new { controller = "Customer", action = "PasswordRecoveryConfirm" });

            routeBuilder.MapControllerRoute("CustomerAuctions",
                            pattern + "customer/auctions",
                            new { controller = "Customer", action = "Auctions" });

            routeBuilder.MapControllerRoute("CustomerNotes",
                            pattern + "customer/notes",
                            new { controller = "Customer", action = "Notes" });

            routeBuilder.MapControllerRoute("CustomerDocuments",
                            pattern + "customer/documents",
                            new { controller = "Customer", action = "Documents" });

            routeBuilder.MapControllerRoute("CustomerCourses",
                            pattern + "customer/courses",
                            new { controller = "Customer", action = "Courses" });

            routeBuilder.MapControllerRoute("AccountActivation",
                            pattern + "customer/activation",
                            new { controller = "Customer", action = "AccountActivation" });

            routeBuilder.MapControllerRoute("CustomerReviews",
                            pattern + "customer/reviews",
                            new { controller = "Customer", action = "Reviews" });

            routeBuilder.MapControllerRoute("CustomerSubAccounts",
                            pattern + "customer/subaccounts",
                            new { controller = "Customer", action = "SubAccounts" });

            routeBuilder.MapControllerRoute("CustomerSubAccountAdd",
                            pattern + "customer/subaccountadd",
                            new { controller = "Customer", action = "SubAccountAdd" });

            routeBuilder.MapControllerRoute("CustomerSubAccountEdit",
                            pattern + "customer/subaccountedit",
                            new { controller = "Customer", action = "SubAccountEdit" });

            routeBuilder.MapControllerRoute("CustomerSubAccountDelete",
                            pattern + "customer/subaccountdelete",
                            new { controller = "Customer", action = "SubAccountDelete" });

            routeBuilder.MapControllerRoute("CustomerDownloadableProducts",
                            pattern + "customer/downloadableproducts",
                            new { controller = "Customer", action = "DownloadableProducts" });

            routeBuilder.MapControllerRoute("CustomerChangePassword",
                            pattern + "customer/changepassword",
                            new { controller = "Customer", action = "ChangePassword" });

            routeBuilder.MapControllerRoute("CustomerDeleteAccount",
                            pattern + "customer/deleteaccount",
                            new { controller = "Customer", action = "DeleteAccount" });

            routeBuilder.MapControllerRoute("CustomerAvatar",
                            pattern + "customer/avatar",
                            new { controller = "Customer", action = "Avatar" });

            routeBuilder.MapControllerRoute("CustomerAddressEdit",
                            pattern + "customer/addressedit/{addressId}",
                            new { controller = "Customer", action = "AddressEdit" });

            routeBuilder.MapControllerRoute("CustomerAddressAdd",
                            pattern + "customer/addressadd",
                            new { controller = "Customer", action = "AddressAdd" });

            routeBuilder.MapControllerRoute("DownloadUserAgreement",
                            pattern + "customer/useragreement/{orderItemId}",
                            new { controller = "Customer", action = "UserAgreement" });

            
        }
        private void RegisterVendorRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //vendor info
            routeBuilder.MapControllerRoute("CustomerVendorInfo",
                            pattern + "customer/vendorinfo",
                            new { controller = "Vendor", action = "Info" });

            //apply for vendor account
            routeBuilder.MapControllerRoute("ApplyVendorAccount",
                            pattern + "vendor/apply",
                            new { controller = "Vendor", action = "ApplyVendor" });

            //contact vendor
            routeBuilder.MapControllerRoute("ContactVendor",
                            pattern + "vendor/contact/{vendorId}",
                            new { controller = "Vendor", action = "ContactVendor" });

        }

        private void RegisterCatalogRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //product search
            routeBuilder.MapControllerRoute("ProductSearch",
                            pattern + "search/",
                            new { controller = "Catalog", action = "Search" });

            routeBuilder.MapControllerRoute("ProductSearchAutoComplete",
                            pattern + "catalog/searchtermautocomplete",
                            new { controller = "Catalog", action = "SearchTermAutoComplete" });

            //product tags
            routeBuilder.MapControllerRoute("ProductTagsAll",
                            pattern + "producttag/all/",
                            new { controller = "Catalog", action = "ProductTagsAll" });

            //manufacturers
            routeBuilder.MapControllerRoute("ManufacturerList",
                            pattern + "manufacturer/all/",
                            new { controller = "Catalog", action = "ManufacturerAll" });

            //vendors
            routeBuilder.MapControllerRoute("VendorList",
                            pattern + "vendor/all/",
                            new { controller = "Catalog", action = "VendorAll" });

            //product tags
            routeBuilder.MapControllerRoute("ProductsByTag",
                            pattern + "producttag/{productTagId}/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTag" });

            routeBuilder.MapControllerRoute("ProductsByTagName",
                            pattern + "producttag/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTagName" });

            //vendor reviews
            routeBuilder.MapControllerRoute("VendorReviews",
                            pattern + "vendoreviews/{vendorId}",
                            new { controller = "Catalog", action = "VendorReviews" });

            //set review helpfulness (AJAX link)
            routeBuilder.MapControllerRoute("SetVendorReviewHelpfulness",
                            pattern + "setvendorreviewhelpfulness",
                            new { controller = "Catalog", action = "SetVendorReviewHelpfulness" });

        }

        private void RegisterProductRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //recently viewed products
            routeBuilder.MapControllerRoute("RecentlyViewedProducts",
                            pattern + "recentlyviewedproducts/",
                            new { controller = "Product", action = "RecentlyViewedProducts" });

            //new products
            routeBuilder.MapControllerRoute("NewProducts",
                            pattern + "newproducts/",
                            new { controller = "Product", action = "NewProducts" });

            //compare products
            routeBuilder.MapControllerRoute("CompareProducts",
                            pattern + "compareproducts/",
                            new { controller = "Product", action = "CompareProducts" });

            //quick view product
            routeBuilder.MapControllerRoute("QuickView-Product",
                            pattern + "quickview/product/{productId}",
                            new { controller = "Product", action = "QuickView" },
                            new { productId = @"\w+" },
                            new[] { "Grand.Web.Controllers" });

            //product email a friend
            routeBuilder.MapControllerRoute("ProductEmailAFriend",
                            pattern + "productemailafriend/{productId}",
                            new { controller = "Product", action = "ProductEmailAFriend" });

            //product ask question
            routeBuilder.MapControllerRoute("AskQuestion",
                            pattern + "askquestion/{productId}",
                            new { controller = "Product", action = "AskQuestion" });

            //product ask question on product page
            routeBuilder.MapControllerRoute("AskQuestionOnProduct",
                            pattern + "askquestiononproduct",
                            new { controller = "Product", action = "AskQuestionOnProduct" });

            //reviews
            routeBuilder.MapControllerRoute("ProductReviews",
                            pattern + "productreviews/{productId}",
                            new { controller = "Product", action = "ProductReviews" });

            //comparing products
            routeBuilder.MapControllerRoute("AddProductToCompare",
                            pattern + "compareproducts/add/{productId}",
                            new { controller = "Product", action = "AddProductToCompareList" });

            //set review helpfulness (AJAX link)
            routeBuilder.MapControllerRoute("SetProductReviewHelpfulness",
                            pattern + "setproductreviewhelpfulness",
                            new { controller = "Product", action = "SetProductReviewHelpfulness" });

            //comparing products
            routeBuilder.MapControllerRoute("RemoveProductFromCompareList",
                            pattern + "compareproducts/remove/{productId}",
                            new { controller = "Product", action = "RemoveProductFromCompareList" });

            routeBuilder.MapControllerRoute("ClearCompareList",
                            pattern + "clearcomparelist/",
                            new { controller = "Product", action = "ClearCompareList" });

            //new RSS
            routeBuilder.MapControllerRoute("NewProductsRSS",
                            pattern + "newproducts/rss",
                            new { controller = "Product", action = "NewProductsRss" });

            //product attributes with "upload file" type
            routeBuilder.MapControllerRoute("UploadFileProductAttribute",
                            pattern + "uploadfileproductattribute/{attributeId}",
                            new { controller = "Product", action = "UploadFileProductAttribute" });
        }

        private void RegisterCommonRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {

            //contact us
            routeBuilder.MapControllerRoute("ContactUs",
                            pattern + "contactus",
                            new { controller = "Common", action = "ContactUs" });
            //sitemap
            routeBuilder.MapControllerRoute("Sitemap",
                            pattern + "sitemap",
                            new { controller = "Common", action = "Sitemap" });

            routeBuilder.MapControllerRoute("sitemap-indexed.xml",
                            pattern + "sitemap-{Id:min(0)}.xml",
                            new { controller = "Common", action = "SitemapXml" });

            //interactive form
            routeBuilder.MapControllerRoute("PopupInteractiveForm",
                            pattern + "popupinteractiveform",
                            new { controller = "Common", action = "PopupInteractiveForm" });

            //change currency (AJAX link)
            routeBuilder.MapControllerRoute("ChangeCurrency",
                            pattern + "changecurrency/{customercurrency}",
                            new { controller = "Common", action = "SetCurrency" });

            //change language (AJAX link)
            routeBuilder.MapControllerRoute("ChangeLanguage",
                            pattern + "changelanguage/{langid}",
                            new { controller = "Common", action = "SetLanguage" });

            //change tax (AJAX link)
            routeBuilder.MapControllerRoute("ChangeTaxType",
                            pattern + "changetaxtype/{customertaxtype}",
                            new { controller = "Common", action = "SetTaxType" });

            //change store (AJAX link)
            routeBuilder.MapControllerRoute("ChangeStore",
                            pattern + "changestore/{store}",
                            new { controller = "Common", action = "SetStore" });

            //get state list by country ID  (AJAX link)
            routeBuilder.MapControllerRoute("GetStatesByCountryId",
                            pattern + "country/getstatesbycountryid/",
                            new { controller = "Country", action = "GetStatesByCountryId" });

            //EU Cookie law accept button handler (AJAX link)
            routeBuilder.MapControllerRoute("EuCookieLawAccept",
                            pattern + "eucookielawaccept",
                            new { controller = "Common", action = "EuCookieLawAccept" });
            
            //Privacy Preference settings
            routeBuilder.MapControllerRoute("PrivacyPreference",
                pattern + "privacypreference",
                new { controller = "Common", action = "PrivacyPreference" });

            // contact attributes with "upload file" type
            routeBuilder.MapControllerRoute("UploadFileContactAttribute",
                            pattern + "uploadfilecontactattribute/{attributeId}",
                            new { controller = "Common", action = "UploadFileContactAttribute" });

            //robots.txt
            routeBuilder.MapControllerRoute("robots.txt",
                            "robots.txt",
                            new { controller = "Common", action = "RobotsTextFile" });

            //sitemap (XML)
            routeBuilder.MapControllerRoute("sitemap.xml",
                            pattern + "sitemap.xml",
                            new { controller = "Common", action = "SitemapXml" });

            //store closed
            routeBuilder.MapControllerRoute("StoreClosed",
                            pattern + "storeclosed",
                            new { controller = "Common", action = "StoreClosed" });

            //page not found
            routeBuilder.MapControllerRoute("PageNotFound",
                            pattern + "page-not-found",
                            new { controller = "Common", action = "PageNotFound" });

            //lets encrypt
            routeBuilder.MapControllerRoute("well-known",
                            ".well-known/pki-validation/{fileName}",
                            new { controller = "LetsEncrypt", action = "Index" });


        }
        private void RegisterCheckoutRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //checkout pages
            routeBuilder.MapControllerRoute("Checkout",
                            pattern + "checkout/",
                            new { controller = "Checkout", action = "Index" });

            routeBuilder.MapControllerRoute("CheckoutOnePage",
                            pattern + "onepagecheckout/",
                            new { controller = "Checkout", action = "OnePageCheckout" });

            routeBuilder.MapControllerRoute("CheckoutShippingAddress",
                            pattern + "checkout/shippingaddress",
                            new { controller = "Checkout", action = "ShippingAddress" });

            routeBuilder.MapControllerRoute("CheckoutSelectShippingAddress",
                            pattern + "checkout/selectshippingaddress",
                            new { controller = "Checkout", action = "SelectShippingAddress" });

            routeBuilder.MapControllerRoute("CheckoutBillingAddress",
                            pattern + "checkout/billingaddress",
                            new { controller = "Checkout", action = "BillingAddress" });

            routeBuilder.MapControllerRoute("CheckoutSelectBillingAddress",
                            pattern + "checkout/selectbillingaddress",
                            new { controller = "Checkout", action = "SelectBillingAddress" });

            routeBuilder.MapControllerRoute("CheckoutShippingMethod",
                            pattern + "checkout/shippingmethod",
                            new { controller = "Checkout", action = "ShippingMethod" });

            routeBuilder.MapControllerRoute("CheckoutShippingFormPartial",
                            pattern + "checkout/getshippingformpartialview/{shippingOption?}",
                            new { controller = "Checkout", action = "GetShippingFormPartialView" });

            routeBuilder.MapControllerRoute("CheckoutPaymentMethod",
                            pattern + "checkout/paymentmethod",
                            new { controller = "Checkout", action = "PaymentMethod" });

            routeBuilder.MapControllerRoute("CheckoutPaymentInfo",
                            pattern + "checkout/paymentinfo",
                            new { controller = "Checkout", action = "PaymentInfo" });

            routeBuilder.MapControllerRoute("CheckoutConfirm",
                            pattern + "checkout/confirm",
                            new { controller = "Checkout", action = "Confirm" });

            routeBuilder.MapControllerRoute("CheckoutCompleted",
                            pattern + "checkout/completed/{orderId?}",
                            new { controller = "Checkout", action = "Completed" });
        }

        private void RegisterAddToCartRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {

            //add product to cart (without any attributes and options). used on catalog pages.
            routeBuilder.MapControllerRoute("AddProductToCart-Catalog",
                            pattern + "addproducttocart/catalog/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddProductToCart_Catalog" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });

            //add product to cart (with attributes and options). used on the product details pages.
            routeBuilder.MapControllerRoute("AddProductToCart-Details",
                            pattern + "addproducttocart/details/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddProductToCart_Details" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });

            //add product to bid, use on the product details page
            routeBuilder.MapControllerRoute("AddBid",
                            pattern + "addbid/AddBid/{productId}/{shoppingCartTypeId}",
                            new { controller = "AddToCart", action = "AddBid" },
                            new { productId = @"\w+", shoppingCartTypeId = @"\d+" },
                            new[] { "Grand.Web.Controllers" });

        }

        private void RegisterCmsRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {

            //widgets
            routeBuilder.MapControllerRoute("WidgetsByZone",
                            $"{pattern}widgetsbyzone/",
                            new { controller = "Widget", action = "WidgetsByZone" });

            //knowledgebase
            routeBuilder.MapControllerRoute("Knowledgebase",
                            pattern + "knowledgebase",
                            new { controller = "Knowledgebase", action = "List" });

            routeBuilder.MapControllerRoute("KnowledgebaseSearch",
                            pattern + "knowledgebase/itemsbykeyword/{keyword?}",
                            new { controller = "Knowledgebase", action = "ItemsByKeyword" });

            //news
            routeBuilder.MapControllerRoute("NewsArchive",
                            pattern + "news",
                            new { controller = "News", action = "List" });

            //news RSS
            routeBuilder.MapControllerRoute("NewsRSS",
                            pattern + "news/rss/{languageId}",
                            new { controller = "News", action = "ListRss" });

            //poll vote AJAX link
            routeBuilder.MapControllerRoute("PollVote",
                            pattern + "poll/vote",
                            new { controller = "Poll", action = "Vote" });

        }

        private void RegisterPrivateMessagesRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {

            //private messages
            routeBuilder.MapControllerRoute("PrivateMessages",
                            pattern + "privatemessages/{tab?}",
                            new { controller = "PrivateMessages", action = "Index" });

            routeBuilder.MapControllerRoute("PrivateMessagesPaged",
                            pattern + "privatemessages/{tab}/page/{pageNumber}",
                            new { controller = "PrivateMessages", action = "Index" });

            routeBuilder.MapControllerRoute("PrivateMessagesInbox",
                            pattern + "inboxupdate",
                            new { controller = "PrivateMessages", action = "InboxUpdate" });

            routeBuilder.MapControllerRoute("PrivateMessagesSent",
                            pattern + "sentupdate",
                            new { controller = "PrivateMessages", action = "SentUpdate" });

            routeBuilder.MapControllerRoute("SendPM",
                            pattern + "sendpm/{toCustomerId}",
                            new { controller = "PrivateMessages", action = "SendPM" });

            routeBuilder.MapControllerRoute("SendPMReply",
                            pattern + "sendpm/{toCustomerId}/{replyToMessageId}",
                            new { controller = "PrivateMessages", action = "SendPM" });

            routeBuilder.MapControllerRoute("ViewPM",
                            pattern + "viewpm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "ViewPM" });

            routeBuilder.MapControllerRoute("DeletePM",
                            pattern + "deletepm/{privateMessageId}",
                            new { controller = "PrivateMessages", action = "DeletePM" });
        }
        
        private void RegisterBoardsRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //forum
            routeBuilder.MapControllerRoute("Boards",
                            pattern + "boards",
                            new { controller = "Boards", action = "Index" });

            routeBuilder.MapControllerRoute("CustomerForumSubscriptions",
                           pattern + "boards/forumsubscriptions",
                           new { controller = "Boards", action = "CustomerForumSubscriptions" });

            routeBuilder.MapControllerRoute("CustomerForumSubscriptionsPaged",
                            pattern + "boards/forumsubscriptions/{pageNumber}",
                            new { controller = "Boards", action = "CustomerForumSubscriptions" });

            //forums
            routeBuilder.MapControllerRoute("ActiveDiscussions",
                            pattern + "boards/activediscussions",
                            new { controller = "Boards", action = "ActiveDiscussions" });

            routeBuilder.MapControllerRoute("ActiveDiscussionsPaged",
                            pattern + "boards/activediscussions/page/{pageNumber}",
                            new { controller = "Boards", action = "ActiveDiscussions" });

            routeBuilder.MapControllerRoute("ActiveDiscussionsRSS",
                            pattern + "boards/activediscussionsrss",
                            new { controller = "Boards", action = "ActiveDiscussionsRSS" });

            routeBuilder.MapControllerRoute("PostEdit",
                            pattern + "boards/postedit/{id}",
                            new { controller = "Boards", action = "PostEdit" });

            routeBuilder.MapControllerRoute("PostDelete",
                            pattern + "boards/postdelete/{id}",
                            new { controller = "Boards", action = "PostDelete" });

            routeBuilder.MapControllerRoute("PostCreate",
                            pattern + "boards/postcreate/{id}",
                            new { controller = "Boards", action = "PostCreate" });

            routeBuilder.MapControllerRoute("PostCreateQuote",
                            pattern + "boards/postcreate/{id}/{quote}",
                            new { controller = "Boards", action = "PostCreate" });

            routeBuilder.MapControllerRoute("TopicEdit",
                            pattern + "boards/topicedit/{id}",
                            new { controller = "Boards", action = "TopicEdit" });

            routeBuilder.MapControllerRoute("TopicDelete",
                            pattern + "boards/topicdelete/{id}",
                            new { controller = "Boards", action = "TopicDelete" });

            routeBuilder.MapControllerRoute("TopicCreate",
                            pattern + "boards/topiccreate/{id}",
                            new { controller = "Boards", action = "TopicCreate" });

            routeBuilder.MapControllerRoute("TopicMove",
                            pattern + "boards/topicmove/{id}",
                            new { controller = "Boards", action = "TopicMove" });

            routeBuilder.MapControllerRoute("TopicWatch",
                            pattern + "boards/topicwatch/{id}",
                            new { controller = "Boards", action = "TopicWatch" });

            routeBuilder.MapControllerRoute("TopicSlug",
                            pattern + "boards/topic/{id}/{slug}",
                            new { controller = "Boards", action = "Topic" });

            routeBuilder.MapControllerRoute("TopicSlugPaged",
                            pattern + "boards/topic/{id}/{slug}/page/{pageNumber}",
                            new { controller = "Boards", action = "Topic" });

            routeBuilder.MapControllerRoute("ForumWatch",
                            pattern + "boards/forumwatch/{id}",
                            new { controller = "Boards", action = "ForumWatch" });

            routeBuilder.MapControllerRoute("ForumRSS",
                            pattern + "boards/forumrss/{id}",
                            new { controller = "Boards", action = "ForumRSS" });

            routeBuilder.MapControllerRoute("ForumSlug",
                            pattern + "boards/forum/{id}/{slug}",
                            new { controller = "Boards", action = "Forum" });

            routeBuilder.MapControllerRoute("ForumSlugPaged",
                            pattern + "boards/forum/{id}/{slug}/page/{pageNumber}",
                            new { controller = "Boards", action = "Forum" });

            routeBuilder.MapControllerRoute("ForumGroupSlug",
                            pattern + "boards/forumgroup/{id}/{slug}",
                            new { controller = "Boards", action = "ForumGroup" });

            routeBuilder.MapControllerRoute("Search",
                            pattern + "boards/search",
                            new { controller = "Boards", action = "Search" });

            //customer profile page
            routeBuilder.MapControllerRoute("CustomerProfile",
                            pattern + "profile/{id}",
                            new { controller = "Profile", action = "Index" });

            routeBuilder.MapControllerRoute("CustomerProfilePaged",
                            pattern + "profile/{id}/page/{pageNumber}",
                            new { controller = "Profile", action = "Index" });
        }

        private void RegisterBlogRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //blog
            routeBuilder.MapControllerRoute("Blog",
                            pattern + "blog",
                            new { controller = "Blog", action = "List" });

            //blog
            routeBuilder.MapControllerRoute("BlogByTag",
                            pattern + "blog/tag/{tag}",
                            new { controller = "Blog", action = "BlogByTag" });

            routeBuilder.MapControllerRoute("BlogByMonth",
                            pattern + "blog/month/{month}",
                            new { controller = "Blog", action = "BlogByMonth" });

            routeBuilder.MapControllerRoute("BlogByCategory",
                            pattern + "blog/category/{categorySeName}",
                            new { controller = "Blog", action = "BlogByCategory" });

            routeBuilder.MapControllerRoute("BlogByKeyword",
                            pattern + "blog/keyword/{searchKeyword?}",
                            new { controller = "Blog", action = "BlogByKeyword" });

            //blog RSS
            routeBuilder.MapControllerRoute("BlogRSS",
                            pattern + "blog/rss/{languageId}",
                            new { controller = "Blog", action = "ListRss" });
        }
        private void RegisterNewsletterRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //assign newsletters to categories
            routeBuilder.MapControllerRoute("SubscribeNewsletterCategory",
                            pattern + "newsletter/savecategories",
                            new { controller = "Newsletter", action = "SaveCategories" });

            //subscribe newsletters
            routeBuilder.MapControllerRoute("SubscribeNewsletter",
                            pattern + "subscribenewsletter",
                            new { controller = "Newsletter", action = "SubscribeNewsletter" });

            //activate newsletters
            routeBuilder.MapControllerRoute("NewsletterActivation",
                            pattern + "newsletter/subscriptionactivation/{token:guid}/{active}",
                            new { controller = "Newsletter", action = "SubscriptionActivation" });

        }
        private void RegisterCartRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
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

            routeBuilder.MapControllerRoute("ChangeTypeCartItem",
                pattern + "changetypecartitem/{id}",
                new { controller = "ShoppingCart", action = "ChangeTypeCartItem" });

            //estimate shipping
            routeBuilder.MapControllerRoute("EstimateShipping",
                            $"{pattern}cart/estimateshipping",
                            new { controller = "ShoppingCart", action = "GetEstimateShipping" });

            //wishlist
            routeBuilder.MapControllerRoute("Wishlist",
                            pattern + "wishlist/{customerGuid?}",
                            new { controller = "ShoppingCart", action = "Wishlist" });

            //email wishlist
            routeBuilder.MapControllerRoute("EmailWishlist",
                            pattern + "emailwishlist",
                            new { controller = "ShoppingCart", action = "EmailWishlist" });

            //checkout attributes with "upload file" type
            routeBuilder.MapControllerRoute("UploadFileCheckoutAttribute",
                            pattern + "uploadfilecheckoutattribute/{attributeId}",
                            new { controller = "ShoppingCart", action = "UploadFileCheckoutAttribute" });
        }

        private void RegisterOrderRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            routeBuilder.MapControllerRoute("CustomerOrders",
                            pattern + "order/history",
                            new { controller = "Order", action = "CustomerOrders" });

            routeBuilder.MapControllerRoute("CustomerRewardPoints",
                            pattern + "rewardpoints/history",
                            new { controller = "Order", action = "CustomerRewardPoints" });

            //orders
            routeBuilder.MapControllerRoute("OrderDetails",
                            pattern + "orderdetails/{orderId}",
                            new { controller = "Order", action = "Details" });

            routeBuilder.MapControllerRoute("ShipmentDetails",
                            pattern + "orderdetails/shipment/{shipmentId}",
                            new { controller = "Order", action = "ShipmentDetails" });


            routeBuilder.MapControllerRoute("ReOrder",
                           pattern + "reorder/{orderId}",
                           new { controller = "Order", action = "ReOrder" });

            routeBuilder.MapControllerRoute("GetOrderPdfInvoice",
                            pattern + "orderdetails/pdf/{orderId}",
                            new { controller = "Order", action = "GetPdfInvoice" });

            routeBuilder.MapControllerRoute("PrintOrderDetails",
                            pattern + "orderdetails/print/{orderId}",
                            new { controller = "Order", action = "PrintOrderDetails" });

            routeBuilder.MapControllerRoute("CancelOrder",
                            pattern + "orderdetails/cancel/{orderId}",
                            new { controller = "Order", action = "CancelOrder" });

            routeBuilder.MapControllerRoute("AddOrderNote",
                            "orderdetails/ordernote/{orderId}",
                            new { controller = "Order", action = "AddOrderNote" });

        }

        private void RegisterBackInStockSubscriptionRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //back in stock notifications
            routeBuilder.MapControllerRoute("BackInStockSubscribePopup",
                            pattern + "backinstocksubscribe/{productId}",
                            new { controller = "BackInStockSubscription", action = "SubscribePopup" });

            //back in stock notifications button text
            routeBuilder.MapControllerRoute("BackInStockSubscribeButton",
                            pattern + "backinstocksubscribebutton/{productId}",
                            new { controller = "BackInStockSubscription", action = "SubscribeButton" });

            routeBuilder.MapControllerRoute("CustomerBackInStockSubscriptions",
                           pattern + "backinstocksubscriptions/manage",
                           new { controller = "BackInStockSubscription", action = "CustomerSubscriptions" });

            routeBuilder.MapControllerRoute("CustomerBackInStockSubscriptionsPaged",
                            pattern + "backinstocksubscriptions/manage/{pageNumber}",
                            new { controller = "BackInStockSubscription", action = "CustomerSubscriptions" });
        }

        private void RegisterReturnRequestRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //customer account links
            routeBuilder.MapControllerRoute("CustomerReturnRequests",
                            pattern + "returnrequest/history",
                            new { controller = "ReturnRequest", action = "CustomerReturnRequests" });

            routeBuilder.MapControllerRoute("ReturnRequest",
                            pattern + "returnrequest/{orderId}",
                            new { controller = "ReturnRequest", action = "ReturnRequest" });

            routeBuilder.MapControllerRoute("ReturnRequestDetails",
                            pattern + "returnrequestdetails/{returnRequestId}",
                            new { controller = "ReturnRequest", action = "ReturnRequestDetails" });
        }

        private void RegisterDownloadRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //downloads
            routeBuilder.MapControllerRoute("GetSampleDownload",
                            pattern + "download/sample/{productid}",
                            new { controller = "Download", action = "Sample" });

            //order downloads
            routeBuilder.MapControllerRoute("GetDownload",
                            pattern + "download/getdownload/{orderItemId}/{agree?}",
                            new { controller = "Download", action = "GetDownload" });

            routeBuilder.MapControllerRoute("GetLicense",
                            pattern + "download/getlicense/{orderItemId}/",
                            new { controller = "Download", action = "GetLicense" });


            routeBuilder.MapControllerRoute("GetOrderNoteFile",
                            pattern + "download/ordernotefile/{ordernoteid}",
                            new { controller = "Download", action = "GetOrderNoteFile" });

            routeBuilder.MapControllerRoute("GetShipmentNoteFile",
                            pattern + "download/shipmentnotefile/{shipmentnoteid}",
                            new { controller = "Download", action = "GetShipmentNoteFile" });

            routeBuilder.MapControllerRoute("GetCustomerNoteFile",
                            pattern + "download/customernotefile/{customernoteid}",
                            new { controller = "Download", action = "GetCustomerNoteFile" });

            routeBuilder.MapControllerRoute("GetReturnRequestNoteFile",
                            pattern + "download/returnrequestnotefile/{returnrequestnoteid}",
                            new { controller = "Download", action = "GetReturnRequestNoteFile" });

            routeBuilder.MapControllerRoute("GetDocumentFile",
                            pattern + "download/documentfile/{documentid}",
                            new { controller = "Download", action = "GetDocumentFile" });

        }
        private void RegisterTopicRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //topics
            routeBuilder.MapControllerRoute("TopicPopup",
                            pattern + "t-popup/{SystemName}",
                            new { controller = "Topic", action = "TopicDetailsPopup" });

            //authenticate topic AJAX link
            routeBuilder.MapControllerRoute("TopicAuthenticate",
                            pattern + "topic/authenticate",
                            new { controller = "Topic", action = "Authenticate" });
        }

        private void RegisterInstallRoute(IEndpointRouteBuilder routeBuilder, string pattern)
        {
            //install
            routeBuilder.MapControllerRoute("Installation", "install",
                            new { controller = "Install", action = "Index" });
            //upgrade
            routeBuilder.MapControllerRoute("Upgrade", "upgrade",
                            new { controller = "Upgrade", action = "Index" });
        }
    }
}
