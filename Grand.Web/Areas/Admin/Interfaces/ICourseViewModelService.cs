using Grand.Core.Domain.Courses;
using Grand.Web.Areas.Admin.Models.Courses;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICourseViewModelService
    {
        Task<CourseModel> PrepareModel(CourseModel model = null);
        Task<Course> InsertCourseModel(CourseModel model);
        Task<Course> UpdateCourseModel(Course course, CourseModel model);
        Task DeleteCourse(Course course);
    }
}
