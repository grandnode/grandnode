using FluentValidation;
using Grand.Core.Domain.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class GenerateCalendarValidator : BaseGrandValidator<ProductModel.GenerateCalendarModel>
    {
        public GenerateCalendarValidator(ILocalizationService localizationService, IProductService productService)
        {
            RuleFor(x => x.StartTime).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.StartTime.Required"));
            RuleFor(x => x.EndTime).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.EndTime.Required"));
            RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate).WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.Days.Finaldatemustbegreaterthanstartdate"));
            RuleFor(x => x.Interval).MustAsync(async (x, y, context) =>
            {
                var product = await productService.GetProductById(x.ProductId);
                if(product.ProductType == ProductType.Reservation)
                {
                    if (x.Interval > 0)
                        return true;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Admin.Catalog.ProductReservations.Fields.Interval.Intervalmustbegreaterthanzero"));

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
