using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerReviewModel
    {
        public string CustomerId { get; set; }

        public ReviewModel Review { get; set; }
    }
}