﻿using System.Web.Mvc;
using System.Web.Routing;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Payments
{
    public partial class PaymentMethodModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.FriendlyName")]
        [AllowHtml]
        public string FriendlyName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SystemName")]
        [AllowHtml]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.Logo")]
        public string LogoUrl { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportCapture")]
        public bool SupportCapture { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportPartiallyRefund")]
        public bool SupportPartiallyRefund { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportRefund")]
        public bool SupportRefund { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportVoid")]
        public bool SupportVoid { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.RecurringPaymentType")]
        public string RecurringPaymentType { get; set; }
        



        public string ConfigurationActionName { get; set; }
        public string ConfigurationControllerName { get; set; }
        public RouteValueDictionary ConfigurationRouteValues { get; set; }
    }
}