using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Courses
{
    public partial class CourseLevelModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.Courses.Level.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Courses.Level.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

    }
}
