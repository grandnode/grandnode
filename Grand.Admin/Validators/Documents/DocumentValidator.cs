using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Documents;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Documents
{
    public class DocumentValidator : BaseGrandValidator<DocumentModel>
    {
        public DocumentValidator(
            IEnumerable<IValidatorConsumer<DocumentModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Documents.Document.Fields.Name.Required"));

            RuleFor(x => x.Number)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Documents.Document.Fields.Number.Required"));

        }
    }
}
