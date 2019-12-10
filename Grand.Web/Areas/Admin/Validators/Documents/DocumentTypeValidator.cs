using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Documents;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Documents
{
    public class DocumentTypeValidator : BaseGrandValidator<DocumentTypeModel>
    {
        public DocumentTypeValidator(
            IEnumerable<IValidatorConsumer<DocumentTypeModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Documents.Type.Fields.Name.Required"));
        }
    }
}
