using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class ProductReviewMappingExtensions
    {
        public static ProductReview ToEntity(this ProductReviewModel model, ProductReview destination)
        {
            return model.MapTo(destination);
        }
    }
}