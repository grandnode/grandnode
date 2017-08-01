using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class OnlineCustomerModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Customers.OnlineCustomers.Fields.CustomerInfo")]
        public string CustomerInfo { get; set; }

        [GrandResourceDisplayName("Admin.Customers.OnlineCustomers.Fields.IPAddress")]
        public string LastIpAddress { get; set; }

        [GrandResourceDisplayName("Admin.Customers.OnlineCustomers.Fields.Location")]
        public string Location { get; set; }

        [GrandResourceDisplayName("Admin.Customers.OnlineCustomers.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }
        
        [GrandResourceDisplayName("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage")]
        public string LastVisitedPage { get; set; }
    }
}