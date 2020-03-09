﻿using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Common
{
    public class AddressValidator : BaseGrandValidator<AddressModel>
    {
        public AddressValidator(
            IEnumerable<IValidatorConsumer<AddressModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.FirstName.Required"))
                .When(x => x.FirstNameEnabled && x.FirstNameRequired);
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.LastName.Required"))
                .When(x => x.LastNameEnabled && x.LastNameRequired);
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.Email.Required"))
                .When(x => x.EmailEnabled && x.EmailRequired);
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"))
                .When(x => x.EmailEnabled && x.EmailRequired);
            RuleFor(x => x.Company)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.Company.Required"))
                .When(x => x.CompanyEnabled && x.CompanyRequired);
            RuleFor(x => x.VatNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.VatNumber.Required"))
                .When(x => x.VatNumberEnabled && x.VatNumberRequired);
            RuleFor(x => x.CountryId)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.Country.Required"))
                .When(x => x.CountryEnabled);
            RuleFor(x => x.CountryId)
                .NotEqual("")
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.Country.Required"))
                .When(x => x.CountryEnabled);
            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.City.Required"))
                .When(x => x.CityEnabled && x.CityRequired);
            RuleFor(x => x.Address1)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.Address1.Required"))
                .When(x => x.StreetAddressEnabled && x.StreetAddressRequired);
            RuleFor(x => x.Address2)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.Address2.Required"))
                .When(x => x.StreetAddress2Enabled && x.StreetAddress2Required);
            RuleFor(x => x.ZipPostalCode)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.ZipPostalCode.Required"))
                .When(x => x.ZipPostalCodeEnabled && x.ZipPostalCodeRequired);
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.PhoneNumber.Required"))
                .When(x => x.PhoneEnabled && x.PhoneRequired);
            RuleFor(x => x.FaxNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Address.Fields.FaxNumber.Required"))
                .When(x => x.FaxEnabled && x.FaxRequired);
        }
    }
}