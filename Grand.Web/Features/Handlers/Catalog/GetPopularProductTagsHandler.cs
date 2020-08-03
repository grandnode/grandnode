using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetPopularProductTagsHandler : IRequestHandler<GetPopularProductTags, PopularProductTagsModel>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IProductTagService _productTagService;
        private readonly CatalogSettings _catalogSettings;

        public GetPopularProductTagsHandler(
            ICacheManager cacheManager, 
            IProductTagService productTagService, 
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _productTagService = productTagService;
            _catalogSettings = catalogSettings;
        }

        public async Task<PopularProductTagsModel> Handle(GetPopularProductTags request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.PRODUCTTAG_POPULAR_MODEL_KEY, request.Language.Id, request.Store.Id);
            var cacheModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new PopularProductTagsModel();

                //get all tags
                var allTags = (await _productTagService
                    .GetAllProductTags())
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var tags = allTags
                    .Take(_catalogSettings.NumberOfProductTags)
                    .ToList();
                //sorting
                tags = tags.OrderBy(x => x.Name).ToList();

                model.TotalTags = allTags.Count;

                foreach (var tag in tags)
                    model.Tags.Add(new ProductTagModel {
                        Id = tag.Id,
                        Name = tag.GetLocalized(y => y.Name, request.Language.Id),
                        SeName = tag.SeName,
                        ProductCount = tag.Count
                    });
                return model;
            });
            return cacheModel;
        }
    }
}
