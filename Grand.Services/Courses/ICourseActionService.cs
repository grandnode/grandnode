using Grand.Core.Domain.Courses;
using System.Threading.Tasks;

namespace Grand.Services.Courses
{
    public interface ICourseActionService
    {
        Task<CourseAction> GetById(string id);
        Task<CourseAction> Update(CourseAction courseAction);
        Task<CourseAction> InsertAsync(CourseAction courseAction);
    }
}
