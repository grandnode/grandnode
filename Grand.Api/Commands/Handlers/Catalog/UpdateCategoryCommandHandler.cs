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
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
    {
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly SeoSettings _seoSettings;

        public UpdateCategoryCommandHandler(
            ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            SeoSettings seoSettings)
        {
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _seoSettings = seoSettings;
        }

        public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryService.GetCategoryById(request.Model.Id);
            string prevPictureId = category.PictureId;
            category = request.Model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            request.Model.SeName = await category.ValidateSeName(request.Model.SeName, category.Name, true, _seoSettings, _urlRecordService, _languageService);
            category.SeName = request.Model.SeName;
            await _categoryService.UpdateCategory(category);
            //search engine name
            await _urlRecordService.SaveSlug(category, request.Model.SeName, "");
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
    }
}
