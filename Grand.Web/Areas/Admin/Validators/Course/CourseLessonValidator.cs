using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Courses;

namespace Grand.Web.Areas.Admin.Validators.Courses
{
    public class CourseLessonValidator : BaseGrandValidator<CourseLessonModel>
    {
        public CourseLessonValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Course.Lesson.Fields.Name.Required"));
            RuleFor(x => x.CourseId).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Course.Lesson.Fields.CourseId.Required"));
            RuleFor(x => x.SubjectId).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Course.Lesson.Fields.SubjectId.Required"));
        }
    }
}