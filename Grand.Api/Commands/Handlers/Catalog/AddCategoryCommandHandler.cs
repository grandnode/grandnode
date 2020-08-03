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
    public class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, CategoryDto>
    {
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly SeoSettings _seoSettings;

        public AddCategoryCommandHandler(
            ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            SeoSettings seoSettings)
        {
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _seoSettings = seoSettings;
        }

        public async Task<CategoryDto> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = request.Model.ToEntity();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _categoryService.InsertCategory(category);
            request.Model.SeName = await category.ValidateSeName(request.Model.SeName,
                category.Name, true, _seoSettings, _urlRecordService, _languageService);
            category.SeName = request.Model.SeName;
            await _categoryService.UpdateCategory(category);
            await _urlRecordService.SaveSlug(category, request.Model.SeName, "");

            //activity log
            await _customerActivityService.InsertActivity("AddNewCategory", category.Id,
                _localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name);

            return category.ToModel();
        }
    }
}
