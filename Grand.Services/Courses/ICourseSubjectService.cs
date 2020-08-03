using Grand.Domain.Courses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Courses
{
    public interface ICourseSubjectService
    {
        Task<CourseSubject> GetById(string id);
        Task<IList<CourseSubject>> GetByCourseId(string courseId);
        Task<CourseSubject> Update(CourseSubject courseSubject);
        Task<CourseSubject> Insert(CourseSubject courseSubject);
        Task Delete(CourseSubject courseSubject);
    }
}
