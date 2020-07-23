using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Courses
{
    public partial class CourseSubjectModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Courses.Course.Subject.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Course.Subject.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string CourseId { get; set; }

    }
}
