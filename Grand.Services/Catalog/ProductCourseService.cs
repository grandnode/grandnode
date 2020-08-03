using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Courses;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    public class ProductCourseService : IProductCourseService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Course> _courseRepository;

        public ProductCourseService(IRepository<Product> productRepository, IRepository<Course> courseRepository)
        {
            _productRepository = productRepository;
            _courseRepository = courseRepository;
        }

        public virtual async Task<Course> GetCourseByProductId(string productId)
        {
            var builder = Builders<Course>.Filter;
            var filter = FilterDefinition<Course>.Empty;

            filter = filter & builder.Where(p => p.ProductId == productId);

            return await _courseRepository.Collection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task<Product> GetProductByCourseId(string courseId)
        {
            var builder = Builders<Product>.Filter;
            var filter = FilterDefinition<Product>.Empty;

            filter = filter & builder.Where(p => p.CourseId == courseId);

            return await _productRepository.Collection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task UpdateCourseOnProduct(string productId, string courseId)
        {
            if (string.IsNullOrEmpty(productId))
                throw new ArgumentNullException("product");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productId);
            var update = Builders<Product>.Update
                .Set(x => x.CourseId, courseId)
                .CurrentDate("UpdatedOnUtc");

            await _productRepository.Collection.UpdateOneAsync(filter, update);
        }
    }
}
