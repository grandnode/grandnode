using Grand.Core.Caching;
using Grand.Domain.Media;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetHomepageCategoryHandler : IRequestHandler<GetHomepageCategory, IList<CategoryModel>>
    {
        private readonly ICategoryService _categoryService;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;

        public GetHomepageCategoryHandler(
            ICategoryService categoryService,
            ICacheManager cacheManager,
            IPictureService pictureService,
            ILocalizationService localizationService,
            MediaSettings mediaSettings)
        {
            _categoryService = categoryService;
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
        }

        public async Task<IList<CategoryModel>> Handle(GetHomepageCategory request, CancellationToken cancellationToken)
        {
            string categoriesCacheKey = string.Format(ModelCacheEventConst.CATEGORY_HOMEPAGE_KEY,
                            string.Join(",", request.Customer.GetCustomerRoleIds()),
                            request.Store.Id,
                            request.Language.Id);

            var model = await _cacheManager.GetAsync(categoriesCacheKey, async () =>
            {
                var cat = new List<CategoryModel>();
                foreach (var x in (await _categoryService.GetAllCategoriesDisplayedOnHomePage()))
                {
                    var catModel = x.ToModel(request.Language);
                    //prepare picture model
                    catModel.PictureModel = new PictureModel {
                        Id = x.PictureId,
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(x.PictureId),
                        ImageUrl = await _pictureService.GetPictureUrl(x.PictureId, _mediaSettings.CategoryThumbPictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                    };
                    cat.Add(catModel);
                }
                return cat;
            });

            return model;
        }
    }
}
