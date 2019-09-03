using Grand.Core.Domain.Courses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Courses
{
    public interface ICourseService
    {
        Task<Course> GetById(string id);
        Task<IList<Course>> GetAll();
        Task<Course> Update(Course course);
        Task<Course> Insert(Course course);
        Task Delete(Course course);
    }
}
