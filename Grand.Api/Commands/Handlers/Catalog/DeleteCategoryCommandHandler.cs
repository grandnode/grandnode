using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public DeleteCategoryCommandHandler(
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryService.GetCategoryById(request.Model.Id);
            if (category != null)
            {
                await _categoryService.DeleteCategory(category);

                //activity log
                await _customerActivityService.InsertActivity("DeleteCategory", category.Id, _localizationService.GetResource("ActivityLog.DeleteCategory"), category.Name);
            }
            return true;
        }
    }
}
