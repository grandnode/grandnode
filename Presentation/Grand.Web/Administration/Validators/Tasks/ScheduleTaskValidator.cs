using FluentValidation;
using Grand.Admin.Models.Tasks;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Tasks
{
    public class ScheduleTaskValidator : BaseNopValidator<ScheduleTaskModel>
    {
        public ScheduleTaskValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.TimeInterval).ExclusiveBetween(0, 3601).WithMessage("alloved range: 1-3600");
            RuleFor(x => x.MinuteOfHour).ExclusiveBetween(0, 60).WithMessage("alloved range:  1-59");
            RuleFor(x => x.HourOfDay).ExclusiveBetween(0, 24).WithMessage("alloved range:  1-23");
            RuleFor(x => x.DayOfWeek).ExclusiveBetween(0, 8).WithMessage("alloved range:  1-7");
            RuleFor(x => x.DayOfMonth).ExclusiveBetween(0, 32).WithMessage("alloved range:  1-31");
        }
    }
}