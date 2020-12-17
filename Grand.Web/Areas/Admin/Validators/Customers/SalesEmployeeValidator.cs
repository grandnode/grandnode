using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Directory
{
    public class SalesEmployeeValidator : BaseGrandValidator<SalesEmployeeModel>
    {
        public SalesEmployeeValidator(
            IEnumerable<IValidatorConsumer<SalesEmployeeModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.SalesEmployee.Fields.Name.Required"));
        }
    }
}