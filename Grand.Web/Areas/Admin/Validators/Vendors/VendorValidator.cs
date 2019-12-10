using FluentValidation;
using Grand.Framework.Extensions;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Vendors;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Vendors
{
    public class VendorValidator : BaseGrandValidator<VendorModel>
    {
        public VendorValidator(
            IEnumerable<IValidatorConsumer<VendorModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Vendors.Fields.Name.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Admin.Vendors.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            RuleFor(x => x.PageSizeOptions)
                .Must(FluentValidationUtilities.PageSizeOptionsValidator)
                .WithMessage(localizationService.GetResource("Admin.Vendors.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
            RuleFor(x => x.Commission)
                .Must(IsCommissionValid)
                .WithMessage(localizationService.GetResource("Admin.Vendors.Fields.Commission.IsCommissionValid"));
                
        }
        
        private bool IsCommissionValid(decimal commission)
        {
            if (commission < 0 || commission > 100)
                return false;

            return true;
        }
    }
}