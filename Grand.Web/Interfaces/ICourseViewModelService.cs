using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Web.Models.Course;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICourseViewModelService
    {
        Task<Course> GetCourseById(string courseId);

        Task<bool> CheckOrder(Course course, Customer customer);

        Task<CourseModel> PrepareCourseModel(Course course);
    }
}
