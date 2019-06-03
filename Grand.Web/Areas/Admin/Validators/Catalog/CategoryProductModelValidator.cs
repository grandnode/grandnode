using FluentValidation;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Linq;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class CategoryProductModelValidator : BaseGrandValidator<CategoryModel.CategoryProductModel>
    {
        public CategoryProductModelValidator(ILocalizationService localizationService, ICategoryService categoryService, IWorkContext workContext)
        {
            if (workContext.CurrentCustomer.IsStaff())
            {
                RuleFor(x => x.CategoryId).MustAsync(async (x, y, context) =>
                {
                    var category = await categoryService.GetCategoryById(x.CategoryId);
                    if (!category.LimitedToStores || (category.Stores.Where(z => z != workContext.CurrentCustomer.StaffStoreId).Any() && category.LimitedToStores))
                        return false;
                    return true;
                }).WithMessage(localizationService.GetResource("Admin.Catalog.Categories.Permisions"));
            }
        }
    }
}