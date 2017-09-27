using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class VendorReviewListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.VendorReviews.List.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.List.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.List.SearchText")]
        public string SearchText { get; set; }

        [GrandResourceDisplayName("Admin.VendorReviews.List.SearchVendor")]
        public string SearchVendorId { get; set; }

    }
}