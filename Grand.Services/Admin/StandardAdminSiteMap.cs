using Grand.Domain.Admin;
using System.Collections.Generic;

namespace Grand.Services.Admin
{
    public static class StandardAdminSiteMap
    {
        public static readonly List<AdminSiteMap> SiteMap =
            new List<AdminSiteMap>() {
                new AdminSiteMap {
                    SystemName = "Dashboard",
                    ResourceName = "Admin.Dashboard",
                    ControllerName = "Home",
                    ActionName = "Index",
                    IconClass = "icon-home",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Dashboard",
                            ResourceName = "Admin.Dashboard",
                            ControllerName = "Home",
                            ActionName = "Index",
                            IconClass = "icon-bar-chart"
                        },
                        new AdminSiteMap {
                            SystemName = "Dashboard",
                            ResourceName = "Admin.Dashboard.Statistics",
                            ControllerName = "Home",
                            ActionName = "Statistics",
                            PermissionNames = new List<string> { "ManageReports" },
                            IconClass = "icon-bulb"
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Catalog",
                    ResourceName = "Admin.Catalog",
                    PermissionNames = new List<string> { "ManageProducts", "ManageCategories", "ManageManufacturers", "ManageProducts", "ManageProductReviews", "ManageProductTags", "ManageAttributes" },
                    IconClass = "fa fa-sitemap",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Products",
                            ResourceName = "Admin.Catalog.Products.Manage",
                            PermissionNames = new List<string> { "ManageProducts" },
                            ControllerName = "Product",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Categories",
                            ResourceName = "Admin.Catalog.Categories",
                            ControllerName = "Category",
                            PermissionNames = new List<string> { "ManageCategories" },
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Manufacturers",
                            ResourceName = "Admin.Catalog.Manufacturers",
                            PermissionNames = new List<string> { "ManageManufacturers" },
                            ControllerName = "Manufacturer",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Bulk edit products",
                            ResourceName = "Admin.Catalog.BulkEdit",
                            PermissionNames = new List<string> { "ManageProducts" },
                            ControllerName = "Product",
                            ActionName = "BulkEdit",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Product reviews",
                            ResourceName = "Admin.Catalog.ProductReviews",
                            PermissionNames = new List<string> { "ManageProductReviews" },
                            ControllerName = "ProductReview",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Product tags",
                            ResourceName = "Admin.Catalog.ProductTags",
                            PermissionNames = new List<string> { "ManageProductTags" },
                            ControllerName = "ProductTags",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Attributes",
                            ResourceName = "Admin.Catalog.Attributes",
                            PermissionNames = new List<string> { "ManageProductAttribute", "ManageSpecificationAttributes", "ManageCheckoutAttributes", "ManageContactAttributes" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Products attributes",
                                    ResourceName = "Admin.Catalog.Attributes.ProductAttributes",
                                    ControllerName = "ProductAttribute",
                                    ActionName = "List",
                                    PermissionNames = new List<string> { "ManageProductAttributes" },
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Specification attributes",
                                    ResourceName = "Admin.Catalog.Attributes.SpecificationAttributes",
                                    ControllerName = "SpecificationAttribute",
                                    ActionName = "List",
                                    PermissionNames = new List<string> { "ManageSpecificationAttributes" },
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Checkout attributes",
                                    ResourceName = "Admin.Catalog.Attributes.CheckoutAttributes",
                                    ControllerName = "CheckoutAttribute",
                                    ActionName = "List",
                                    PermissionNames = new List<string> { "ManageCheckoutAttributes" },
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Contact attributes",
                                    ResourceName = "Admin.Catalog.Attributes.ContactAttributes",
                                    ControllerName = "ContactAttribute",
                                    ActionName = "List",
                                    PermissionNames = new List<string> { "ManageContactAttributes" },
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Sales",
                    ResourceName = "Admin.Sales",
                    PermissionNames = new List<string> { "ManageOrders", "ManageShipments", "ManageRecurringPayments", "ManageReturnRequests", "ManageGiftCards", "ManageCurrentCarts", "ManageOrderTags" },
                    IconClass = "icon-basket",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Orders",
                            ResourceName = "Admin.Orders",
                            PermissionNames = new List<string> { "ManageOrders" },
                            ControllerName = "Order",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Shipments",
                            ResourceName = "Admin.Orders.Shipments.List",
                            PermissionNames = new List<string> { "ManageShipments" },
                            ControllerName = "Shipment",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Return requests",
                            ResourceName = "Admin.ReturnRequests",
                            PermissionNames = new List<string> { "ManageReturnRequests" },
                            ControllerName = "ReturnRequest",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Gift cards",
                            ResourceName = "Admin.GiftCards",
                            PermissionNames = new List<string> { "ManageGiftCards" },
                            ControllerName = "GiftCard",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Shopping carts and wishlists",
                            ResourceName = "Admin.CurrentCartWishlists",
                            PermissionNames = new List<string> { "ManageCurrentCarts" },
                            ControllerName = "ShoppingCart",
                            ActionName = "CurrentCarts",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Recurring payments",
                            ResourceName = "Admin.RecurringPayments",
                            PermissionNames = new List<string> { "ManageRecurringPayments" },
                            ControllerName = "RecurringPayment",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "OrderTags",
                            ResourceName = "Admin.Orders.OrderTags",
                            PermissionNames = new List<string> { "ManageOrderTags" },
                            ControllerName = "OrderTags",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Customers",
                    ResourceName = "Admin.Customers",
                    PermissionNames = new List<string> { "ManageCustomers", "ManageVendors", "ManageVendorReviews", "ManageActivityLog", "ManageCustomerTags", "ManageCustomerRoles", "ManageSalesEmployees", "ManageDocuments", "ManageAffiliates" },
                    IconClass = "icon-users",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Customers",
                            ResourceName = "Admin.Customers.Customers",
                            PermissionNames = new List<string> { "ManageCustomers" },
                            ControllerName = "Customer",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Customer roles",
                            ResourceName = "Admin.Customers.CustomerRoles",
                            PermissionNames = new List<string> { "ManageCustomerRoles" },
                            ControllerName = "CustomerRole",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Customer tags",
                            ResourceName = "Admin.Customers.CustomerTags",
                            PermissionNames = new List<string> { "ManageCustomerTags" },
                            ControllerName = "CustomerTag",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Sales employee",
                            ResourceName = "Admin.Customers.SalesEmployees",
                            PermissionNames = new List<string> { "ManageSalesEmployees" },
                            ControllerName = "SalesEmployee",
                            ActionName = "Index",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Online customers",
                            ResourceName = "Admin.Customers.OnlineCustomers",
                            PermissionNames = new List<string> { "ManageCustomers" },
                            ControllerName = "OnlineCustomer",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Vendors",
                            ResourceName = "Admin.Vendors",
                            PermissionNames = new List<string> { "ManageVendors" },
                            ControllerName = "Vendor",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Vendor reviews",
                            ResourceName = "Admin.VendorReviews",
                            PermissionNames = new List<string> { "ManageVendorReviews" },
                            ControllerName = "VendorReview",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Affiliates",
                            ResourceName = "Admin.Affiliates",
                            PermissionNames = new List<string> { "ManageAffiliates" },
                            ControllerName = "Affiliate",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Documents",
                            ResourceName = "Admin.Customers.Documents",
                            PermissionNames = new List<string> { "ManageDocuments" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Document types",
                                    ResourceName = "Admin.Customers.Document.Type",
                                    ControllerName = "Document",
                                    ActionName = "Types",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Document list",
                                    ResourceName = "Admin.Customers.Document.List",
                                    ControllerName = "Document",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Activity Log",
                            ResourceName = "Admin.Configuration.ActivityLog",
                            PermissionNames = new List<string> { "ManageActivityLog" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Activity Types",
                                    ResourceName = "Admin.Configuration.ActivityLog.ActivityLogType",
                                    ControllerName = "ActivityLog",
                                    ActionName = "ListTypes",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Activity Log",
                                    ResourceName = "Admin.Configuration.ActivityLog.ActivityLog",
                                    ControllerName = "ActivityLog",
                                    ActionName = "ListLogs",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Activity Stats",
                                    ResourceName = "Admin.Configuration.ActivityLog.ActivityStats",
                                    ControllerName = "ActivityLog",
                                    ActionName = "ListStats",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Promotions",
                    ResourceName = "Admin.Promotions",
                    PermissionNames = new List<string> { "ManageAffiliates", "ManageNewsletterCategories", "ManageNewsletterSubscribers", "ManageCampaigns", "ManageDiscounts", "ManageActions", "ManageReminders", "ManagePushNotifications", "ManageBanners", "ManageInteractiveForms" },
                    IconClass = "icon-bulb",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Newsletter",
                            ResourceName = "Admin.Promotions.Newsletter",
                            PermissionNames = new List<string> { "ManageCampaigns", "ManageNewsletterSubscribers" },
                            IconClass = "fa fa-dot-circle-o",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Campaigns",
                                    ResourceName = "Admin.Promotions.Campaigns",
                                    PermissionNames = new List<string> { "ManageCampaigns" },
                                    ControllerName = "Campaign",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Newsletter categories",
                                    ResourceName = "Admin.Promotions.NewsletterCategory",
                                    PermissionNames = new List<string> { "ManageNewsletterCategories" },
                                    ControllerName = "NewsletterCategory",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Newsletter subscriptions",
                                    ResourceName = "Admin.Promotions.NewsletterSubscriptions",
                                    PermissionNames = new List<string> { "ManageNewsletterSubscribers" },
                                    ControllerName = "NewsLetterSubscription",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Discounts",
                            ResourceName = "Admin.Promotions.Discounts",
                            PermissionNames = new List<string> { "ManageDiscounts" },
                            ControllerName = "Discount",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Customer actions",
                            ResourceName = "Admin.Customers.CustomerActions",
                            PermissionNames = new List<string> { "ManageActions", "ManageBanners", "ManageInteractiveForms" },
                            IconClass = "fa fa-dot-circle-o",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Customer action type",
                                    ResourceName = "Admin.Customers.Actiontype",
                                    PermissionNames = new List<string> { "ManageActions" },
                                    ControllerName = "CustomerActionType",
                                    ActionName = "ListTypes",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Customer actions",
                                    ResourceName = "Admin.Customers.CustomerActions",
                                    PermissionNames = new List<string> { "ManageActions" },
                                    ControllerName = "CustomerAction",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Banners",
                                    ResourceName = "Admin.Promotions.Banners",
                                    PermissionNames = new List<string> { "ManageBanners" },
                                    ControllerName = "Banner",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "InteractiveForms",
                                    ResourceName = "Admin.Promotions.InteractiveForms",
                                    PermissionNames = new List<string> { "ManageInteractiveForms" },
                                    ControllerName = "InteractiveForm",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Customer reminders",
                            ResourceName = "Admin.Customers.CustomerReminders",
                            PermissionNames = new List<string> { "ManageReminders" },
                            ControllerName = "CustomerReminder",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "PushNotifications",
                            ResourceName = "Admin.PushNotifications",
                            PermissionNames = new List<string> { "ManagePushNotifications" },
                            ControllerName = "PushNotifications",
                            IconClass = "fa fa-dot-circle-o",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "PushNotifications",
                                    ResourceName = "Admin.PushNotifications.Send",
                                    PermissionNames = new List<string> { "ManagePushNotifications" },
                                    ControllerName = "PushNotifications",
                                    ActionName = "Send",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "PushNotifications",
                                    ResourceName = "Admin.PushNotifications.Messages",
                                    PermissionNames = new List<string> { "ManagePushNotifications" },
                                    ControllerName = "PushNotifications",
                                    ActionName = "Messages",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "PushNotifications",
                                    ResourceName = "Admin.PushNotifications.Receivers",
                                    PermissionNames = new List<string> { "ManagePushNotifications" },
                                    ControllerName = "PushNotifications",
                                    ActionName = "Receivers",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Content Management",
                    ResourceName = "Admin.ContentManagement",
                    PermissionNames = new List<string> { "ManagePolls", "ManageNews", "ManageBlog", "ManageTopics", "ManageForums", "ManageMessageTemplates", "ManageKnowledgebase", "ManageCourses" },
                    IconClass = "icon-layers",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Topics",
                            ResourceName = "Admin.ContentManagement.Topics",
                            PermissionNames = new List<string> { "ManageTopics" },
                            ControllerName = "Topic",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Message templates",
                            ResourceName = "Admin.ContentManagement.MessageTemplates",
                            PermissionNames = new List<string> { "ManageMessageTemplates" },
                            ControllerName = "MessageTemplate",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Polls",
                            ResourceName = "Admin.ContentManagement.Polls",
                            PermissionNames = new List<string> { "ManagePolls" },
                            ControllerName = "Poll",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "News",
                            ResourceName = "Admin.ContentManagement.News",
                            PermissionNames = new List<string> { "ManageNews" },
                            ControllerName = "News",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Blog",
                            ResourceName = "Admin.ContentManagement.Blog",
                            PermissionNames = new List<string> { "ManageBlog" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Blog categories",
                                    ResourceName = "Admin.ContentManagement.Blog.BlogCategories",
                                    ControllerName = "Blog",
                                    ActionName = "CategoryList",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Blog posts",
                                    ResourceName = "Admin.ContentManagement.Blog.BlogPosts",
                                    ControllerName = "Blog",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Blog comments",
                                    ResourceName = "Admin.ContentManagement.Blog.Comments",
                                    ControllerName = "Blog",
                                    ActionName = "Comments",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Manage forums",
                            ResourceName = "Admin.ContentManagement.Forums",
                            PermissionNames = new List<string> { "ManageForums" },
                            ControllerName = "Forum",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Knowledgebase",
                            ResourceName = "Admin.ContentManagement.Knowledgebase",
                            PermissionNames = new List<string> { "ManageKnowledgebase" },
                            ControllerName = "Knowledgebase",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Course",
                            ResourceName = "Admin.ContentManagement.Course",
                            PermissionNames = new List<string> { "ManageCourses" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Course level",
                                    ResourceName = "Admin.ContentManagement.Course.Level",
                                    ControllerName = "Course",
                                    ActionName = "Level",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Manage courses",
                                    ResourceName = "Admin.ContentManagement.Course.Manage",
                                    ControllerName = "Course",
                                    ActionName = "List",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Reports",
                    ResourceName = "Admin.Reports",
                    PermissionNames = new List<string> { "ManageReports" },
                    IconClass = "icon-bar-chart",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Low stock report",
                            ResourceName = "Admin.Reports.LowStockReport",
                            PermissionNames = new List<string> { "ManageReports" },
                            ControllerName = "Reports",
                            ActionName = "LowStockReport",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Bestsellers",
                            ResourceName = "Admin.Reports.Bestsellers",
                            PermissionNames = new List<string> { "ManageReports" },
                            ControllerName = "Reports",
                            ActionName = "BestsellersReport",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Products never purchased",
                            ResourceName = "Admin.Reports.NeverSold",
                            PermissionNames = new List<string> { "ManageReports" },
                            ControllerName = "Reports",
                            ActionName = "NeverSoldReport",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Country report",
                            ResourceName = "Admin.Reports.Country",
                            PermissionNames = new List<string> { "ManageReports" },
                            AllPermissions = true,
                            ControllerName = "Reports",
                            ActionName = "CountryReport",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Customer reports",
                            ResourceName = "Admin.Reports.Customers",
                            PermissionNames = new List<string> { "ManageReports" },
                            AllPermissions = true,
                            ControllerName = "Reports",
                            ActionName = "Customer",
                            IconClass = "fa fa-dot-circle-o"
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Configuration",
                    ResourceName = "Admin.Configuration",
                    PermissionNames = new List<string> { "ManageCountries", "ManageLanguages", "ManageSettings", "ManagePaymentMethods", "ManageExternalAuthenticationMethods", "ManageTaxSettings", "ManageShippingSettings", "ManageCurrencies", "ManageMeasures", "ManageActivityLog", "ManageACL", "ManageEmailAccounts", "ManagePlugins", "ManageWidgets", "ManageStores" },
                    IconClass = "icon-wrench",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Settings",
                            ResourceName = "Admin.Configuration.Settings",
                            PermissionNames = new List<string> { "ManageSettings" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "General and common settings",
                                    ResourceName = "Admin.Configuration.Settings.GeneralCommon",
                                    ControllerName = "Setting",
                                    ActionName = "GeneralCommon",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Catalog settings",
                                    ResourceName = "Admin.Configuration.Settings.Catalog",
                                    ControllerName = "Setting",
                                    ActionName = "Catalog",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Customer and user settings",
                                    ResourceName = "Admin.Configuration.Settings.CustomerUser",
                                    ControllerName = "Setting",
                                    ActionName = "CustomerUser",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Shopping cart settings",
                                    ResourceName = "Admin.Configuration.Settings.ShoppingCart",
                                    ControllerName = "Setting",
                                    ActionName = "ShoppingCart",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Order settings",
                                    ResourceName = "Admin.Configuration.Settings.Order",
                                    ControllerName = "Setting",
                                    ActionName = "Order",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Media settings",
                                    ResourceName = "Admin.Configuration.Settings.Media",
                                    ControllerName = "Setting",
                                    ActionName = "Media",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Tax settings",
                                    ResourceName = "Admin.Configuration.Settings.Tax",
                                    ControllerName = "Setting",
                                    ActionName = "Tax",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Shipping settings",
                                    ResourceName = "Admin.Configuration.Settings.Shipping",
                                    ControllerName = "Setting",
                                    ActionName = "Shipping",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Reward points",
                                    ResourceName = "Admin.Configuration.Settings.RewardPoints",
                                    ControllerName = "Setting",
                                    ActionName = "RewardPoints",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Blog settings",
                                    ResourceName = "Admin.Configuration.Settings.Blog",
                                    ControllerName = "Setting",
                                    ActionName = "Blog",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "News settings",
                                    ResourceName = "Admin.Configuration.Settings.News",
                                    ControllerName = "Setting",
                                    ActionName = "News",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Forums settings",
                                    ResourceName = "Admin.Configuration.Settings.Forums",
                                    ControllerName = "Setting",
                                    ActionName = "Forum",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Vendor settings",
                                    ResourceName = "Admin.Configuration.Settings.Vendor",
                                    ControllerName = "Setting",
                                    ActionName = "Vendor",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Push notifications settings",
                                    ResourceName = "Admin.Configuration.Settings.PushNotifications",
                                    ControllerName = "Setting",
                                    ActionName = "PushNotifications",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Admin search settings",
                                    ResourceName = "Admin.Configuration.Settings.AdminSearch",
                                    ControllerName = "Setting",
                                    ActionName = "AdminSearch",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "All settings (settings)",
                                    ResourceName = "Admin.Configuration.Settings.AllSettings",
                                    ControllerName = "Setting",
                                    ActionName = "AllSettings",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Stores",
                            ResourceName = "Admin.Configuration.Stores",
                            PermissionNames = new List<string> { "ManageStores" },
                            ControllerName = "Store",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Countries",
                            ResourceName = "Admin.Configuration.Countries",
                            PermissionNames = new List<string> { "ManageCountries" },
                            ControllerName = "Country",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Languages",
                            ResourceName = "Admin.Configuration.Languages",
                            PermissionNames = new List<string> { "ManageLanguages" },
                            ControllerName = "Language",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Currencies",
                            ResourceName = "Admin.Configuration.Currencies",
                            PermissionNames = new List<string> { "ManageCurrencies" },
                            ControllerName = "Currency",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Measures",
                            ResourceName = "Admin.Configuration.Measures",
                            PermissionNames = new List<string> { "ManageMeasures" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Units",
                                    ResourceName = "Admin.Configuration.Measures.Units",
                                    ControllerName = "Measure",
                                    ActionName = "Units",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Weights",
                                    ResourceName = "Admin.Configuration.Measures.Weights",
                                    ControllerName = "Measure",
                                    ActionName = "Weights",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Dimensions",
                                    ResourceName = "Admin.Configuration.Measures.Dimensions",
                                    ControllerName = "Measure",
                                    ActionName = "Dimensions",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "EmailAccounts",
                            ResourceName = "Admin.Configuration.EmailAccounts",
                            PermissionNames = new List<string> { "ManageEmailAccounts" },
                            ControllerName = "EmailAccount",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Payment",
                            ResourceName = "Admin.Configuration.Payment",
                            PermissionNames = new List<string> { "ManagePaymentMethods" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Payment methods",
                                    ResourceName = "Admin.Configuration.Payment.Methods",
                                    PermissionNames = new List<string> { "ManagePaymentMethods" },
                                    ControllerName = "Payment",
                                    ActionName = "Methods",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Payment method restrictions",
                                    ResourceName = "Admin.Configuration.Payment.MethodRestrictions",
                                    PermissionNames = new List<string> { "ManagePaymentMethods" },
                                    ControllerName = "Payment",
                                    ActionName = "MethodRestrictions",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Tax",
                            ResourceName = "Admin.Configuration.Tax",
                            PermissionNames = new List<string> { "ManageTaxSettings" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Tax providers",
                                    ResourceName = "Admin.Configuration.Tax.Providers",
                                    ControllerName = "Tax",
                                    ActionName = "Providers",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Tax categories",
                                    ResourceName = "Admin.Configuration.Tax.Categories",
                                    ControllerName = "Tax",
                                    ActionName = "Categories",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Shipping",
                            ResourceName = "Admin.Configuration.Shipping",
                            PermissionNames = new List<string> { "ManageShippingSettings" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Shipping providers",
                                    ResourceName = "Admin.Configuration.Shipping.Providers",
                                    ControllerName = "Shipping",
                                    ActionName = "Providers",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Shipping methods",
                                    ResourceName = "Admin.Configuration.Shipping.Methods",
                                    ControllerName = "Shipping",
                                    ActionName = "Methods",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Shipping method restrictions",
                                    ResourceName = "Admin.Configuration.Shipping.Restrictions",
                                    ControllerName = "Shipping",
                                    ActionName = "Restrictions",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Delivery dates",
                                    ResourceName = "Admin.Configuration.Shipping.DeliveryDates",
                                    ControllerName = "Shipping",
                                    ActionName = "DeliveryDates",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Warehouses",
                                    ResourceName = "Admin.Configuration.Shipping.Warehouses",
                                    ControllerName = "Shipping",
                                    ActionName = "Warehouses",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "PickupPoints",
                                    ResourceName = "Admin.Configuration.Shipping.PickupPoints",
                                    ControllerName = "Shipping",
                                    ActionName = "PickupPoints",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Access control list",
                            ResourceName = "Admin.Configuration.ACL",
                            PermissionNames = new List<string> { "ManageACL" },
                            ControllerName = "Security",
                            ActionName = "Permissions",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "External authentication methods",
                            ResourceName = "Admin.Configuration.ExternalAuthenticationMethods",
                            PermissionNames = new List<string> { "ManageExternalAuthenticationMethods" },
                            ControllerName = "ExternalAuthentication",
                            ActionName = "Methods",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Widgets",
                            ResourceName = "Admin.ContentManagement.Widgets",
                            PermissionNames = new List<string> { "ManageWidgets" },
                            ControllerName = "Widget",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Local plugins",
                            ResourceName = "Admin.Configuration.Plugins.Local",
                            PermissionNames = new List<string> { "ManagePlugins" },
                            ControllerName = "Plugin",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "System",
                    ResourceName = "Admin.System",
                    PermissionNames = new List<string> { "ManageSystemLog", "ManageMessageQueue", "ManageMessageContactForm", "ManageMaintenance", "ManageScheduleTasks" },
                    IconClass = "icon-settings",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Log",
                            ResourceName = "Admin.System.Log",
                            PermissionNames = new List<string> { "ManageSystemLog" },
                            ControllerName = "Log",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Queued emails",
                            ResourceName = "Admin.System.QueuedEmails",
                            PermissionNames = new List<string> { "ManageMessageQueue" },
                            ControllerName = "QueuedEmail",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Contact Us form",
                            ResourceName = "Admin.System.ContactForm",
                            PermissionNames = new List<string> { "ManageMessageContactForm" },
                            ControllerName = "ContactForm",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Maintenance",
                            ResourceName = "Admin.System.Maintenance",
                            PermissionNames = new List<string> { "ManageMaintenance" },
                            ControllerName = "Common",
                            ActionName = "Maintenance",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Schedule tasks",
                            ResourceName = "Admin.System.ScheduleTasks",
                            PermissionNames = new List<string> { "ManageScheduleTasks" },
                            ControllerName = "ScheduleTask",
                            ActionName = "List",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Warnings",
                            ResourceName = "Admin.System.Warnings",
                            PermissionNames = new List<string> { "ManageMaintenance" },
                            ControllerName = "Common",
                            ActionName = "Warnings",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "System information",
                            ResourceName = "Admin.System.SystemInfo",
                            PermissionNames = new List<string> { "ManageMaintenance" },
                            ControllerName = "Common",
                            ActionName = "SystemInfo",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Search engine friendly names",
                            ResourceName = "Admin.System.SeNames",
                            PermissionNames = new List<string> { "ManageMaintenance" },
                            ControllerName = "Common",
                            ActionName = "SeNames",
                            IconClass = "fa fa-dot-circle-o"
                        },
                        new AdminSiteMap {
                            SystemName = "Developer tools",
                            ResourceName = "Admin.System.DeveloperTools",
                            PermissionNames = new List<string> { "ManageMaintenance" },
                            IconClass = "fa fa-dot-circle-o",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Manage API Users",
                                    ResourceName = "Admin.System.APIUsers",
                                    PermissionNames = new List<string> { "ManageMaintenance" },
                                    ControllerName = "ApiUser",
                                    ActionName = "Index",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Roslyn compiler",
                                    ResourceName = "Admin.System.Roslyn",
                                    PermissionNames = new List<string> { "ManageMaintenance" },
                                    ControllerName = "Common",
                                    ActionName = "Roslyn",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Query editor",
                                    ResourceName = "Admin.System.QueryEditor",
                                    PermissionNames = new List<string> { "ManageMaintenance" },
                                    ControllerName = "Common",
                                    ActionName = "QueryEditor",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Custom css",
                                    ResourceName = "Admin.System.CustomCss",
                                    PermissionNames = new List<string> { "ManageMaintenance" },
                                    ControllerName = "Common",
                                    ActionName = "CustomCss",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Custom JS",
                                    ResourceName = "Admin.System.CustomJs",
                                    PermissionNames = new List<string> { "ManageMaintenance" },
                                    ControllerName = "Common",
                                    ActionName = "CustomJs",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Robot.txt",
                                    ResourceName = "Admin.System.AdditionsRobotsTxt",
                                    PermissionNames = new List<string> { "ManageMaintenance" },
                                    ControllerName = "Common",
                                    ActionName = "AdditionsRobotsTxt",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        },
                        new AdminSiteMap {
                            SystemName = "Templates",
                            ResourceName = "Admin.System.Templates",
                            PermissionNames = new List<string> { "ManageMaintenance" },
                            IconClass = "fa fa-arrow-circle-o-right",
                            ChildNodes = new List<AdminSiteMap>() {
                                new AdminSiteMap {
                                    SystemName = "Category templates",
                                    ResourceName = "Admin.System.Templates.Category",
                                    ControllerName = "Template",
                                    ActionName = "CategoryTemplates",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Manufacturer templates",
                                    ResourceName = "Admin.System.Templates.Manufacturer",
                                    ControllerName = "Template",
                                    ActionName = "ManufacturerTemplates",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Product templates",
                                    ResourceName = "Admin.System.Templates.Product",
                                    ControllerName = "Template",
                                    ActionName = "ProductTemplates",
                                    IconClass = "fa fa-dot-circle-o"
                                },
                                new AdminSiteMap {
                                    SystemName = "Topic templates",
                                    ResourceName = "Admin.System.Templates.Topic",
                                    ControllerName = "Template",
                                    ActionName = "TopicTemplates",
                                    IconClass = "fa fa-dot-circle-o"
                                }
                            }
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Help",
                    ResourceName = "Admin.Help",
                    IconClass = "icon-question",
                    ChildNodes = new List<AdminSiteMap>() {
                        new AdminSiteMap {
                            SystemName = "Community forums",
                            ResourceName = "Admin.Help.Forums",
                            Url = "https://grandnode.com/boards?utm_source=web&utm_medium=admin&utm_term=web&utm_campaign=Community",
                            IconClass = "fa fa-dot-circle-o",
                            OpenUrlInNewTab = true
                        },
                        new AdminSiteMap {
                            SystemName = "Premium support services",
                            ResourceName = "Admin.Help.SupportServices",
                            Url = "https://grandnode.com/support-service?utm_source=web&utm_medium=admin&utm_term=web&utm_campaign=Support",
                            IconClass = "fa fa-dot-circle-o",
                            OpenUrlInNewTab = true
                        }
                    }
                },
                new AdminSiteMap {
                    SystemName = "Third party plugins",
                    ResourceName = "Admin.Plugins"
                }

            };
    };
}