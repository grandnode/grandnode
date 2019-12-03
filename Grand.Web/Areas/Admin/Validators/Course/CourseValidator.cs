using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Courses;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Courses
{
    public class CourseValidator : BaseGrandValidator<CourseModel>
    {
        public CourseValidator(
            IEnumerable<IValidatorConsumer<CourseModel>> validators,
            ILocalizationService localizationService, IProductCourseService productCourseService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Courses.Course.Fields.Name.Required"));
            RuleFor(x => x.ProductId).MustAsync(async (x, y, context) =>
            {
                if(!string.IsNullOrEmpty(x.ProductId) && !string.IsNullOrEmpty(x.Id))
                {
                    var course = await productCourseService.GetCourseByProductId(x.ProductId);
                    if (course != null && course.Id != x.Id)
                        return false;
                }
                if (!string.IsNullOrEmpty(x.ProductId) && string.IsNullOrEmpty(x.Id))
                {
                    var course = await productCourseService.GetCourseByProductId(x.ProductId);
                    if (course != null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Admin.Courses.Course.Fields.ProductId.Assigned"));
        }
    }
}