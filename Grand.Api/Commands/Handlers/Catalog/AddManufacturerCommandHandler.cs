using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Domain.Seo;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Seo;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddManufacturerCommandHandler : IRequestHandler<AddManufacturerCommand, ManufacturerDto>
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly SeoSettings _seoSettings;

        public AddManufacturerCommandHandler(
            IManufacturerService manufacturerService,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            SeoSettings seoSettings)
        {
            _manufacturerService = manufacturerService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _seoSettings = seoSettings;
        }

        public async Task<ManufacturerDto> Handle(AddManufacturerCommand request, CancellationToken cancellationToken)
        {
            var manufacturer = request.Model.ToEntity();
            manufacturer.CreatedOnUtc = DateTime.UtcNow;
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            await _manufacturerService.InsertManufacturer(manufacturer);
            request.Model.SeName = await manufacturer.ValidateSeName(request.Model.SeName, manufacturer.Name, true, _seoSettings, _urlRecordService, _languageService);
            manufacturer.SeName = request.Model.SeName;
            await _manufacturerService.UpdateManufacturer(manufacturer);
            await _urlRecordService.SaveSlug(manufacturer, request.Model.SeName, "");

            //activity log
            await _customerActivityService.InsertActivity("AddNewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);

            return manufacturer.ToModel();
        }
    }
}
