using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Knowledgebase;

namespace Grand.Web.Areas.Admin.Validators.Knowledgebase
{
    public class KnowledgebaseCategoryModelValidator : BaseGrandValidator<KnowledgebaseCategoryModel>
    {
        public KnowledgebaseCategoryModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Name.Required"));
        }
    }
}
