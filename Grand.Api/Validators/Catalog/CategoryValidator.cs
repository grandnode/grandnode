using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Extensions;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Media;
using System.Collections.Generic;

namespace Grand.Api.Validators.Catalog
{
    public class CategoryValidator : BaseGrandValidator<CategoryDto>
    {
        public CategoryValidator(IEnumerable<IValidatorConsumer<CategoryDto>> validators,
            ILocalizationService localizationService, IPictureService pictureService, ICategoryService categoryService, ICategoryTemplateService 
            categoryTemplateService) : base(validators)
        {
            
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Api.Catalog.Category.Fields.Name.Required"));
            RuleFor(x => x.PageSizeOptions).Must(FluentValidationUtilities.PageSizeOptionsValidator).WithMessage(localizationService.GetResource("Api.Catalog.Category.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.PictureId))
                {
                    var picture = await pictureService.GetPictureById(x.PictureId);
                    if (picture == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Category.Fields.PictureId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.ParentCategoryId))
                {
                    var category = await categoryService.GetCategoryById(x.ParentCategoryId);
                    if (category == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Category.Fields.ParentCategoryId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.CategoryTemplateId))
                {
                    var template = await categoryTemplateService.GetCategoryTemplateById(x.CategoryTemplateId);
                    if (template == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Category.Fields.CategoryTemplateId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var category = await categoryService.GetCategoryById(x.Id);
                    if (category == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.Category.Fields.Id.NotExists"));
        }
    }
}
