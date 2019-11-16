using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Courses;

namespace Grand.Web.Areas.Admin.Validators.Courses
{
    public class CourseLevelValidator : BaseGrandValidator<CourseLevelModel>
    {
        public CourseLevelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Level.Fields.Name.Required"));
        }
    }
}