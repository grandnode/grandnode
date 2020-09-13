using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Tasks;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Tasks
{
    public class ScheduleTaskValidator : BaseGrandValidator<ScheduleTaskModel>
    {
        public ScheduleTaskValidator(
            IEnumerable<IValidatorConsumer<ScheduleTaskModel>> validators)
            : base(validators)
        {
            RuleFor(x => x.TimeInterval).GreaterThan(0).WithMessage("Time interval must be greater than zero");
        }
    }
}