using FluentValidation;
using Grand.Domain.Common;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Validators.Common
{
    public class ContactVendorValidator : BaseGrandValidator<ContactVendorModel>
    {
        public ContactVendorValidator(
            IEnumerable<IValidatorConsumer<ContactVendorModel>> validators,
            ILocalizationService localizationService, CommonSettings commonSettings)
            : base(validators)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("ContactVendor.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.FullName).NotEmpty().WithMessage(localizationService.GetResource("ContactVendor.FullName.Required"));
            if (commonSettings.SubjectFieldOnContactUsForm)
            {
                RuleFor(x => x.Subject).NotEmpty().WithMessage(localizationService.GetResource("ContactVendor.Subject.Required"));
            }
            RuleFor(x => x.Enquiry).NotEmpty().WithMessage(localizationService.GetResource("ContactVendor.Enquiry.Required"));
        }}
}