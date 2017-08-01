using FluentValidation;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Orders
{
    public class CheckoutAttributeValidator : BaseGrandValidator<CheckoutAttributeModel>
    {
        public CheckoutAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name.Required"));
        }
    }
}