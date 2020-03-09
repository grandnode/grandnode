using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Courses;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Courses
{
    public class CourseLevelValidator : BaseGrandValidator<CourseLevelModel>
    {
        public CourseLevelValidator(
            IEnumerable<IValidatorConsumer<CourseLevelModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Level.Fields.Name.Required"));
        }
    }
}