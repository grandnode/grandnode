using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Tasks;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Tasks
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