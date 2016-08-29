using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Core.Domain.Messages
{
    public partial class CampaignCustomerSubscription
    {
        public string CustomerId { get; set; }
        public string Email { get; set; }
    }
}
