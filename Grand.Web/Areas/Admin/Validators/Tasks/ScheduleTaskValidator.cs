using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Tasks;

namespace Grand.Web.Areas.Admin.Validators.Tasks
{
    public class ScheduleTaskValidator : BaseGrandValidator<ScheduleTaskModel>
    {
        public ScheduleTaskValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.TimeInterval).GreaterThan(0).WithMessage("Time interval must be greater than zero");
        }
    }
}