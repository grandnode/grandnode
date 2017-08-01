using FluentValidation;
using Grand.Web.Areas.Admin.Models.Shipping;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Shipping
{
    public class WarehouseValidator : BaseGrandValidator<WarehouseModel>
    {
        public WarehouseValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.Warehouses.Fields.Name.Required"));
        }
    }
}