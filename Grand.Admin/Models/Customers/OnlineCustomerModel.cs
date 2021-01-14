using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;

namespace Grand.Admin.Models.Customers
{
    public partial class OnlineCustomerModel : BaseEntityModel
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