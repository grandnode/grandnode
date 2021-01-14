using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Messages;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Messages
{
    public class ContactAttributeValidator : BaseGrandValidator<ContactAttributeModel>
    {
        public ContactAttributeValidator(
            IEnumerable<IValidatorConsumer<ContactAttributeModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.ContactAttributes.Fields.Name.Required"));
        }
    }
}