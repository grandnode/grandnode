using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Common;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Common
{
    public class AddressAttributeValidator : BaseGrandValidator<AddressAttributeModel>
    {
        public AddressAttributeValidator(
            IEnumerable<IValidatorConsumer<AddressAttributeModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Address.AddressAttributes.Fields.Name.Required"));
        }
    }
}