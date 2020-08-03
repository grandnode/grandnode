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
    public class GetManufacturerAllHandler : IRequestHandler<GetManufacturerAll, IList<ManufacturerModel>>
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ICacheManager _cacheManager;
        private readonly MediaSettings _mediaSettings;

        public GetManufacturerAllHandler(IManufacturerService manufacturerService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            ICacheManager cacheManager,
            MediaSettings mediaSettings)
        {
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _cacheManager = cacheManager;
            _mediaSettings = mediaSettings;
        }

        public async Task<IList<ManufacturerModel>> Handle(GetManufacturerAll request, CancellationToken cancellationToken)
        {
            string cacheKey = string.Format(ModelCacheEventConst.MANUFACTURER_ALL_MODEL_KEY,
                request.Language.Id,
                string.Join(",", request.Customer.GetCustomerRoleIds()),
                request.Store.Id);
            return await _cacheManager.GetAsync(cacheKey, () => PrepareManufacturerAll(request));            
        }

        private async Task<List<ManufacturerModel>> PrepareManufacturerAll(GetManufacturerAll request)
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = await _manufacturerService.GetAllManufacturers(storeId: request.Store.Id);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = manufacturer.ToModel(request.Language);

                //prepare picture model
                modelMan.PictureModel = new PictureModel {
                    Id = manufacturer.PictureId,
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(manufacturer.PictureId),
                    ImageUrl = await _pictureService.GetPictureUrl(manufacturer.PictureId, _mediaSettings.ManufacturerThumbPictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                };
                model.Add(modelMan);
            }
            return model;
        }
    }
}
