using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class RecurringPaymentModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.ID")]
        public override string Id { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.CycleLength")]
        public int CycleLength { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.CyclePeriod")]
        public int CyclePeriodId { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.CyclePeriod")]
        public string CyclePeriodStr { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.TotalCycles")]
        public int TotalCycles { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.StartDate")]
        public string StartDate { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.IsActive")]
        public bool IsActive { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.NextPaymentDate")]
        public string NextPaymentDate { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.CyclesRemaining")]
        public int CyclesRemaining { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.InitialOrder")]
        public string InitialOrderId { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.Customer")]
        public string CustomerEmail { get; set; }

        [GrandResourceDisplayName("Admin.RecurringPayments.Fields.PaymentType")]
        public string PaymentType { get; set; }
        
        public bool CanCancelRecurringPayment { get; set; }

        #region Nested classes


        public partial class RecurringPaymentHistoryModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.RecurringPayments.History.Order")]
            public string OrderId { get; set; }
            public int OrderNumber { get; set; }

            public string RecurringPaymentId { get; set; }

            [GrandResourceDisplayName("Admin.RecurringPayments.History.OrderStatus")]
            public string OrderStatus { get; set; }

            [GrandResourceDisplayName("Admin.RecurringPayments.History.PaymentStatus")]
            public string PaymentStatus { get; set; }

            [GrandResourceDisplayName("Admin.RecurringPayments.History.ShippingStatus")]
            public string ShippingStatus { get; set; }

            [GrandResourceDisplayName("Admin.RecurringPayments.History.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}