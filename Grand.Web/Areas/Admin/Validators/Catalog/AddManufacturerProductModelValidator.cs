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
    public class AddManufacturerProductModelValidator : BaseGrandValidator<ManufacturerModel.AddManufacturerProductModel>
    {
        public AddManufacturerProductModelValidator(ILocalizationService localizationService, IManufacturerService manufacturerService, IWorkContext workContext)
        {
            if (workContext.CurrentCustomer.IsStaff())
            {
                RuleFor(x => x).MustAsync(async (x, y, context) =>
                {
                    var manufacturer = await manufacturerService.GetManufacturerById(x.ManufacturerId);
                    if (manufacturer != null)
                        if (!manufacturer.LimitedToStores || (manufacturer.Stores.Where(z => z != workContext.CurrentCustomer.StaffStoreId).Any() && manufacturer.LimitedToStores))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    return true;

                }).WithMessage(localizationService.GetResource("Admin.Catalog.Manufacturers.Permisions"));
            }
        }
    }
}