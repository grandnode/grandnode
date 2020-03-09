using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Messages;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Messages
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