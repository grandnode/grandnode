using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetPopularProductTags : IRequest<PopularProductTagsModel>
    {
        public Language Language { get; set; }
        public Store Store { get; set; }
    }
}
