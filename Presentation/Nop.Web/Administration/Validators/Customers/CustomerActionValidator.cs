﻿using FluentValidation;
using Nop.Admin.Models.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Customers
{
    public class CustomerActionValidator : BaseNopValidator<CustomerActionModel>
    {
        public CustomerActionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerAction.Fields.Name.Required"));
        }
    }
}