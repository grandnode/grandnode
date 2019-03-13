using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class ReviewModel : BaseGrandEntityModel
    {
        /// <summary>
        /// Gets or sets the title
        /// </summary>
        [GrandResourceDisplayName("Admin.Customers.Customers.Reviews.Title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the review text
        /// </summary>
        [GrandResourceDisplayName("Admin.Customers.Customers.Reviews.Review")]
        public string ReviewText { get; set; }
    }
}