using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Core.Domain.Seo;
using Grand.Core.Data;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public partial class ManufacturerApiService : IManufacturerApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IManufacturerService _manufacturerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        private readonly IMongoCollection<ManufacturerDto> _manufacturer;
        private readonly SeoSettings _seoSettings;

        public ManufacturerApiService(IMongoDBContext mongoDBContext, IManufacturerService manufacturerService, IUrlRecordService urlRecordService, ILanguageService languageService, IPictureService pictureService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService, SeoSettings seoSettings)
        {
            _mongoDBContext = mongoDBContext;
            _manufacturerService = manufacturerService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _pictureService = pictureService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _seoSettings = seoSettings;
            _manufacturer = _mongoDBContext.Database().GetCollection<ManufacturerDto>(typeof(Core.Domain.Catalog.Manufacturer).Name);
        }
        public virtual Task<ManufacturerDto> GetById(string id)
        {
            return _manufacturer.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public virtual IMongoQueryable<ManufacturerDto> GetManufacturers()
        {
            return _manufacturer.AsQueryable();
        }

        public virtual async Task<ManufacturerDto> InsertOrUpdateManufacturer(ManufacturerDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = await InsertManufacturer(model);
            else
                model = await UpdateManufacturer(model);

            return model;
        }
        public virtual async Task<ManufacturerDto> InsertManufacturer(ManufacturerDto model)
        {
            var manufacturer = model.ToEntity();
            manufacturer.CreatedOnUtc = DateTime.UtcNow;
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            await _manufacturerService.InsertManufacturer(manufacturer);
            model.SeName = await manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true, _seoSettings, _urlRecordService, _languageService);
            manufacturer.SeName = model.SeName;
            await _manufacturerService.UpdateManufacturer(manufacturer);
            await _urlRecordService.SaveSlug(manufacturer, model.SeName, "");

            //activity log
            await _customerActivityService.InsertActivity("AddNewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);

            return manufacturer.ToModel();
        }

        public virtual async Task<ManufacturerDto> UpdateManufacturer(ManufacturerDto model)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(model.Id);
            string prevPictureId = manufacturer.PictureId;
            manufacturer = model.ToEntity(manufacturer);
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = await manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true, _seoSettings, _urlRecordService, _languageService);
            manufacturer.SeName = model.SeName;
            await _manufacturerService.UpdateManufacturer(manufacturer);
            //search engine name
            await _urlRecordService.SaveSlug(manufacturer, model.SeName, "");
            await _manufacturerService.UpdateManufacturer(manufacturer);
            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != manufacturer.PictureId)
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

        public virtual async Task DeleteManufacturer(ManufacturerDto model)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer != null)
            {
                await _manufacturerService.DeleteManufacturer(manufacturer);

                //activity log
                await _customerActivityService.InsertActivity("DeleteManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);
            }
        }

       
    }
}
