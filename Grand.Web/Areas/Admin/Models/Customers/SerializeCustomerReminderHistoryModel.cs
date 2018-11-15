using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public class SerializeCustomerReminderHistoryModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public DateTime SendDate { get; set; }
        public int Level { get; set; }
        public bool OrderId { get; set; }
    }
}
