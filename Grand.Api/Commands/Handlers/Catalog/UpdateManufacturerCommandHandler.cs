using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Domain.Seo;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateManufacturerCommandHandler : IRequestHandler<UpdateManufacturerCommand, ManufacturerDto>
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly SeoSettings _seoSettings;

        public UpdateManufacturerCommandHandler(
            IManufacturerService manufacturerService,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            SeoSettings seoSettings)
        {
            _manufacturerService = manufacturerService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _seoSettings = seoSettings;
        }

        public async Task<ManufacturerDto> Handle(UpdateManufacturerCommand request, CancellationToken cancellationToken)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(request.Model.Id);
            var prevPictureId = manufacturer.PictureId;
            manufacturer = request.Model.ToEntity(manufacturer);
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            request.Model.SeName = await manufacturer.ValidateSeName(request.Model.SeName, manufacturer.Name, true, _seoSettings, _urlRecordService, _languageService);
            manufacturer.SeName = request.Model.SeName;
            await _manufacturerService.UpdateManufacturer(manufacturer);
            //search engine name
            await _urlRecordService.SaveSlug(manufacturer, request.Model.SeName, "");
            await _manufacturerService.UpdateManufacturer(manufacturer);
            //delete an old picture (if deleted or updated)
            if (!string.IsNullOrEmpty(prevPictureId) && prevPictureId != manufacturer.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            if (!string.IsNullOrEmpty(manufacturer.PictureId))
            {
                var picture = await _pictureService.GetPictureById(manufacturer.PictureId);
                if (picture != null)
                    await _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(manufacturer.Name));
            }
            //activity log
            await _customerActivityService.InsertActivity("EditManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.EditManufacturer"), manufacturer.Name);

            return manufacturer.ToModel();
        }
    }
}
