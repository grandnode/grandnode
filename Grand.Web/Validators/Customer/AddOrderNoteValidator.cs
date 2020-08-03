using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Orders;
using System.Collections.Generic;

namespace Grand.Web.Validators.Customer
{
    public class AddOrderNoteValidator : BaseGrandValidator<AddOrderNoteModel>
    {
        public AddOrderNoteValidator(
            IEnumerable<IValidatorConsumer<AddOrderNoteModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Note).NotEmpty().WithMessage(localizationService.GetResource("OrderNote.Fields.Title.Required"));
        }
    }
}
