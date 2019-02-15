using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Data;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;

namespace Grand.Api.Services
{
    public partial class ManufacturerApiService : IManufacturerApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IManufacturerService _manufacturerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        private readonly IMongoCollection<ManufacturerDto> _manufacturer;

        public ManufacturerApiService(IMongoDBContext mongoDBContext, IManufacturerService manufacturerService, IUrlRecordService urlRecordService, IPictureService pictureService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService)
        {
            _mongoDBContext = mongoDBContext;
            _manufacturerService = manufacturerService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;

            _manufacturer = _mongoDBContext.Database().GetCollection<ManufacturerDto>(typeof(Core.Domain.Catalog.Manufacturer).Name);
        }
        public virtual ManufacturerDto GetById(string id)
        {
            return _manufacturer.AsQueryable().FirstOrDefault(x => x.Id == id);
        }

        public virtual IMongoQueryable<ManufacturerDto> GetManufacturers()
        {
            return _manufacturer.AsQueryable();
        }

        public virtual ManufacturerDto InsertOrUpdateManufacturer(ManufacturerDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = InsertManufacturer(model);
            else
                model = UpdateManufacturer(model);

            return model;
        }
        public virtual ManufacturerDto InsertManufacturer(ManufacturerDto model)
        {
            var manufacturer = model.ToEntity();
            manufacturer.CreatedOnUtc = DateTime.UtcNow;
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            _manufacturerService.InsertManufacturer(manufacturer);
            model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
            manufacturer.SeName = model.SeName;
            _manufacturerService.UpdateManufacturer(manufacturer);
            _urlRecordService.SaveSlug(manufacturer, model.SeName, "");

            //activity log
            _customerActivityService.InsertActivity("AddNewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);

            return manufacturer.ToModel();
        }

        public virtual ManufacturerDto UpdateManufacturer(ManufacturerDto model)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(model.Id);
            string prevPictureId = manufacturer.PictureId;
            manufacturer = model.ToEntity(manufacturer);
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
            manufacturer.SeName = model.SeName;
            _manufacturerService.UpdateManufacturer(manufacturer);
            //search engine name
            _urlRecordService.SaveSlug(manufacturer, model.SeName, "");
            _manufacturerService.UpdateManufacturer(manufacturer);
            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != manufacturer.PictureId)
            {
                var prevPicture = _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            if (!string.IsNullOrEmpty(manufacturer.PictureId))
            {
                var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                if (picture != null)
                    _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(manufacturer.Name));
            }
            //activity log
            _customerActivityService.InsertActivity("EditManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.EditManufacturer"), manufacturer.Name);

            return manufacturer.ToModel();
        }

        public virtual void DeleteManufacturer(ManufacturerDto model)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer != null)
            {
                _manufacturerService.DeleteManufacturer(manufacturer);

                //activity log
                _customerActivityService.InsertActivity("DeleteManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);
            }
        }

       
    }
}
