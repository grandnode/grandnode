using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Courses;

namespace Grand.Web.Areas.Admin.Models.Courses
{
    [Validator(typeof(CourseSubjectValidator))]
    public partial class CourseSubjectModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Courses.Course.Subject.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Subject.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string CourseId { get; set; }

    }
}
