using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Courses;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    public interface IProductCourseService
    {
        Task<Product> GetProductByCourseId(string courseId);
        Task<Course> GetCourseByProductId(string productId);
        Task UpdateCourseOnProduct(string productId, string courseId);
    }
}
