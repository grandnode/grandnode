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
    public partial class CategoryApiService : ICategoryApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMongoCollection<CategoryDto> _category;
        private readonly SeoSettings _seoSettings;

        public CategoryApiService(IMongoDBContext mongoDBContext, ICategoryService categoryService, IUrlRecordService urlRecordService, ILanguageService languageService, IPictureService pictureService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService, SeoSettings seoSettings)
        {
            _mongoDBContext = mongoDBContext;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _pictureService = pictureService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _seoSettings = seoSettings;
            _category = _mongoDBContext.Database().GetCollection<CategoryDto>(typeof(Core.Domain.Catalog.Category).Name);
        }
        public virtual Task<CategoryDto> GetById(string id)
        {
            return _category.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public virtual IMongoQueryable<CategoryDto> GetCategories()
        {
            return _category.AsQueryable();
        }
        public virtual async Task<CategoryDto> InsertOrUpdateCategory(CategoryDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = await InsertCategory(model);
            else
                model = await UpdateCategory(model);

            return model;
        }
        public virtual async Task<CategoryDto> InsertCategory(CategoryDto model)
        {
            var category = model.ToEntity();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _categoryService.InsertCategory(category);
            model.SeName = await category.ValidateSeName(model.SeName, category.Name, true, _seoSettings, _urlRecordService, _languageService );
            category.SeName = model.SeName;
            await _categoryService.UpdateCategory(category);
            await _urlRecordService.SaveSlug(category, model.SeName, "");

            //activity log
            await _customerActivityService.InsertActivity("AddNewCategory", category.Id, _localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name);

            return category.ToModel();
        }

        public virtual async Task<CategoryDto> UpdateCategory(CategoryDto model)
        {
            var category = await _categoryService.GetCategoryById(model.Id);
            string prevPictureId = category.PictureId;
            category = model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            model.SeName = await category.ValidateSeName(model.SeName, category.Name, true, _seoSettings, _urlRecordService, _languageService);
            category.SeName = model.SeName;
            await _categoryService.UpdateCategory(category);
            //search engine name
            await _urlRecordService.SaveSlug(category, model.SeName, "");
            await _categoryService.UpdateCategory(category);
            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != category.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            if (!string.IsNullOrEmpty(category.PictureId))
            {
                var picture = await _pictureService.GetPictureById(category.PictureId);
                if (picture != null)
                    await _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(category.Name));
            }
            //activity log
            await _customerActivityService.InsertActivity("EditCategory", category.Id, _localizationService.GetResource("ActivityLog.EditCategory"), category.Name);
            return category.ToModel();
        }

        public virtual async Task DeleteCategory(CategoryDto model)
        {
            var category = await _categoryService.GetCategoryById(model.Id);
            if (category != null)
            {
                await _categoryService.DeleteCategory(category);

                //activity log
                await _customerActivityService.InsertActivity("DeleteCategory", category.Id, _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);
            }
        }

       
    }
}
