using Grand.Domain.Courses;
using System.Threading.Tasks;

namespace Grand.Services.Courses
{
    public interface ICourseActionService
    {
        Task<CourseAction> GetById(string id);
        Task<CourseAction> GetCourseAction(string customerId, string lessonId);
        Task<bool> CustomerLessonCompleted(string customerId, string lessonId);
        Task<CourseAction> Update(CourseAction courseAction);
        Task<CourseAction> InsertAsync(CourseAction courseAction);
    }
}
