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
    public partial class CategoryApiService : ICategoryApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        private readonly IMongoCollection<CategoryDto> _category;

        public CategoryApiService(IMongoDBContext mongoDBContext, ICategoryService categoryService, IUrlRecordService urlRecordService, IPictureService pictureService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService)
        {
            _mongoDBContext = mongoDBContext;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;

            _category = _mongoDBContext.Database().GetCollection<CategoryDto>(typeof(Core.Domain.Catalog.Category).Name);
        }
        public virtual CategoryDto GetById(string id)
        {
            return _category.AsQueryable().FirstOrDefault(x => x.Id == id);
        }

        public virtual IMongoQueryable<CategoryDto> GetCategories()
        {
            return _category.AsQueryable();
        }
        public virtual CategoryDto InsertOrUpdateCategory(CategoryDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = InsertCategory(model);
            else
                model = UpdateCategory(model);

            return model;
        }
        public virtual CategoryDto InsertCategory(CategoryDto model)
        {
            var category = model.ToEntity();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            _categoryService.InsertCategory(category);
            model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
            category.SeName = model.SeName;
            _categoryService.UpdateCategory(category);
            _urlRecordService.SaveSlug(category, model.SeName, "");

            //activity log
            _customerActivityService.InsertActivity("AddNewCategory", category.Id, _localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name);

            return category.ToModel();
        }

        public virtual CategoryDto UpdateCategory(CategoryDto model)
        {
            var category = _categoryService.GetCategoryById(model.Id);
            string prevPictureId = category.PictureId;
            category = model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = category.ValidateSeName(model.SeName, category.Name, true);
            category.SeName = model.SeName;
            _categoryService.UpdateCategory(category);
            //search engine name
            _urlRecordService.SaveSlug(category, model.SeName, "");
            _categoryService.UpdateCategory(category);
            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != category.PictureId)
            {
                var prevPicture = _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            if (!string.IsNullOrEmpty(category.PictureId))
            {
                var picture = _pictureService.GetPictureById(category.PictureId);
                if (picture != null)
                    _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(category.Name));
            }
            //activity log
            _customerActivityService.InsertActivity("EditCategory", category.Id, _localizationService.GetResource("ActivityLog.EditCategory"), category.Name);

            return category.ToModel();
        }

        public virtual void DeleteCategory(CategoryDto model)
        {
            var category = _categoryService.GetCategoryById(model.Id);
            if (category != null)
            {
                _categoryService.DeleteCategory(category);

                //activity log
                _customerActivityService.InsertActivity("DeleteCategory", category.Id, _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);
            }
        }

       
    }
}
