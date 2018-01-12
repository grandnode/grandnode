using FluentValidation;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class GenerateCalendarValidator : BaseGrandValidator<ProductModel.GenerateCalendarModel>
    {
        public GenerateCalendarValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.StartTime).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.StartTime.Required"));
            RuleFor(x => x.EndTime).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.EndTime.Required"));
            RuleFor(x => x.StartDateUtc).LessThanOrEqualTo(x => x.EndDateUtc).WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.Days.Finaldatemustbegreaterthanstartdate"));
            RuleFor(x => x.Interval).GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.Interval.Intervalmustbegreaterthanzero"));

            RuleFor(x => x.StartTime).Must((x, context) =>
            {
                if ((IntervalUnit)x.IntervalUnit == IntervalUnit.Day)
                    return true;
                else
                {
                    if ((x.StartTime != default(DateTime) || x.StartTime == null))
                        return true;
                }
                return false;
            }).WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.StartTime.Required"));


            RuleFor(x => x.Quantity).Must((x, context) =>
            {
                if((IntervalUnit)x.IntervalUnit == IntervalUnit.Day)
                    return true;
                else
                {
                    if (x.Quantity > 0)
                        return true;
                }
                return false;
            }).WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.Quantity.Quantitymustbegreaterthanzero"));

            RuleFor(x => x.Resource).Must((x, context) =>
            {
                if ((IntervalUnit)x.IntervalUnit != IntervalUnit.Day)
                    return true;
                else
                {
                    if (!string.IsNullOrEmpty(x.Resource))
                        return true;
                }
                return false;
            }).WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.Resource.Required"));
        }
    }
}
