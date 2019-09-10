using Grand.Core.Domain.Courses;
using Grand.Web.Areas.Admin.Models.Courses;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICourseViewModelService
    {
        Task<CourseModel> PrepareCourseModel(CourseModel model = null);
        Task<Course> InsertCourseModel(CourseModel model);
        Task<Course> UpdateCourseModel(Course course, CourseModel model);
        Task DeleteCourse(Course course);
        Task<CourseLessonModel> PrepareCourseLessonModel(string courseId, CourseLessonModel model = null);
        Task<CourseLesson> InsertCourseLessonModel(CourseLessonModel model);
        Task<CourseLesson> UpdateCourseLessonModel(CourseLesson lesson, CourseLessonModel model);
        Task DeleteCourseLesson(CourseLesson lesson);
    }
}
