using Grand.Domain.Catalog;
using Grand.Domain.Courses;
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
