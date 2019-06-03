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
    public class AddCategoryProductModelValidator : BaseGrandValidator<CategoryModel.AddCategoryProductModel>
    {
        public AddCategoryProductModelValidator(ILocalizationService localizationService, ICategoryService categoryService, IWorkContext workContext)
        {
            if (workContext.CurrentCustomer.IsStaff())
            {
                RuleFor(x => x).MustAsync(async (x, y, context) =>
                {
                    var category = await categoryService.GetCategoryById(x.CategoryId);
                    if (category != null)
                        if (!category.LimitedToStores || (category.Stores.Where(z => z != workContext.CurrentCustomer.StaffStoreId).Any() && category.LimitedToStores))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    return true;

                }).WithMessage("No permisions");
            }
        }
    }
}