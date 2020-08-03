using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddProductCategoryCommandHandler : IRequestHandler<AddProductCategoryCommand, bool>
    {
        private readonly ICategoryService _categoryService;

        public AddProductCategoryCommandHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<bool> Handle(AddProductCategoryCommand request, CancellationToken cancellationToken)
        {
            var productCategory = new ProductCategory {
                ProductId = request.Product.Id,
                CategoryId = request.Model.CategoryId,
                IsFeaturedProduct = request.Model.IsFeaturedProduct,
            };
            await _categoryService.InsertProductCategory(productCategory);

            return true;
        }
    }
}
