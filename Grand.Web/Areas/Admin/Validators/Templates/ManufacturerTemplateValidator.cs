﻿using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Templates;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Templates
{
    public class ManufacturerTemplateValidator : BaseGrandValidator<ManufacturerTemplateModel>
    {
        public ManufacturerTemplateValidator(
            IEnumerable<IValidatorConsumer<ManufacturerTemplateModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Manufacturer.Name.Required"));
            RuleFor(x => x.ViewPath).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Manufacturer.ViewPath.Required"));
        }
    }
}