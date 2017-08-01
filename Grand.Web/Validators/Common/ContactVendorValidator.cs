using FluentValidation;
using Grand.Core.Domain.Common;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Web.Models.Common;

namespace Grand.Web.Validators.Common
{
    public class ContactVendorValidator : BaseGrandValidator<ContactVendorModel>
    {
        public ContactVendorValidator(ILocalizationService localizationService, CommonSettings commonSettings)
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