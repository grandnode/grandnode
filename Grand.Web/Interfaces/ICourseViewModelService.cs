using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Web.Models.Course;
using Grand.Web.Models.Customer;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICourseViewModelService
    {
        Task<Course> GetCourseById(string courseId);
        Task<CoursesModel> GetCoursesByCustomer(Customer customer, string storeId);
        Task<CourseLesson> GetLessonById(string lessonId);
        Task<bool> CheckOrder(Course course, Customer customer);
        Task<bool> IsApprovedCourse(Course course, Customer customer);
        Task<CourseModel> PrepareCourseModel(Course course);
        Task<LessonModel> PrepareLessonModel(Course course, CourseLesson lesson);
        Task Approved(Course course, CourseLesson lesson, Customer customer);
    }
}
