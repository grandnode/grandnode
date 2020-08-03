using Grand.Core.Caching;
using Grand.Domain.Media;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetHomepageManufacturersHandler : IRequestHandler<GetHomepageManufacturers, IList<ManufacturerModel>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly MediaSettings _mediaSettings;

        public GetHomepageManufacturersHandler(
            ICacheManager cacheManager, 
            IManufacturerService manufacturerService, 
            IPictureService pictureService, 
            ILocalizationService localizationService, 
            MediaSettings mediaSettings)
        {
            _cacheManager = cacheManager;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
        }

        public async Task<IList<ManufacturerModel>> Handle(GetHomepageManufacturers request, CancellationToken cancellationToken)
        {
            string manufacturersCacheKey = string.Format(ModelCacheEventConst.MANUFACTURER_HOMEPAGE_KEY, request.Store.Id, request.Language.Id);

            var model = await _cacheManager.GetAsync(manufacturersCacheKey, async () =>
            {
                var modelManuf = new List<ManufacturerModel>();
                var manuf = await _manufacturerService.GetAllManufacturers(storeId: request.Store.Id);
                foreach (var x in manuf.Where(x => x.ShowOnHomePage))
                {
                    var manModel = x.ToModel(request.Language);
                    //prepare picture model
                    manModel.PictureModel = new PictureModel {
                        Id = x.PictureId,
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(x.PictureId),
                        ImageUrl = await _pictureService.GetPictureUrl(x.PictureId, _mediaSettings.CategoryThumbPictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), manModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), manModel.Name)
                    };
                    modelManuf.Add(manModel);
                }
                return modelManuf;
            });
            return model;

        }
    }
}
