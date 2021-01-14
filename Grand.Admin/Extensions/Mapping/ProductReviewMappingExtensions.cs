using Grand.Domain.Catalog;
using Grand.Admin.Models.Catalog;

namespace Grand.Admin.Extensions
{
    public static class ProductReviewMappingExtensions
    {
        public static ProductReview ToEntity(this ProductReviewModel model, ProductReview destination)
        {
            return model.MapTo(destination);
        }
    }
}