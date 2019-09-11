using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Courses;

namespace Grand.Web.Areas.Admin.Models.Courses
{
    [Validator(typeof(CourseLevelValidator))]
    public partial class CourseLevelModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Courses.Level.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Level.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

    }
}
