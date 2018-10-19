using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Order;

namespace Grand.Web.Validators.Customer
{
    public class AddOrderNoteValidator : BaseGrandValidator<AddOrderNoteModel>
    {
        public AddOrderNoteValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Note).NotEmpty().WithMessage(localizationService.GetResource("OrderNote.Fields.Title.Required"));
        }
    }
}
