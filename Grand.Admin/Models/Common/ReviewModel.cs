using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Common
{
    public partial class ReviewModel : BaseEntityModel
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