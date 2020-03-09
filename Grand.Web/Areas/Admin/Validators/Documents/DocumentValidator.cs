using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Documents;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Documents
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
