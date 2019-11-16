using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Courses;

namespace Grand.Web.Areas.Admin.Validators.Courses
{
    public class CourseSubjectValidator : BaseGrandValidator<CourseSubjectModel>
    {
        public CourseSubjectValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Course.Subject.Fields.Name.Required"));
            RuleFor(x => x.CourseId).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Course.Subject.Fields.CourseId.Required"));
        }
    }
}