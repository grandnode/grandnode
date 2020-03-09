﻿using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Common
{
    public class AddressAttributeValueValidator : BaseGrandValidator<AddressAttributeValueModel>
    {
        public AddressAttributeValueValidator(
            IEnumerable<IValidatorConsumer<AddressAttributeValueModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Address.AddressAttributes.Values.Fields.Name.Required"));
        }
    }
}