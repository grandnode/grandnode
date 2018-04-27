using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using System.Linq;

namespace Grand.Web.Areas.Admin.Validators.Knowledgebase
{
    public class KnowledgebaseCategoryModelValidator : BaseGrandValidator<KnowledgebaseCategoryModel>
    {
        public KnowledgebaseCategoryModelValidator(ILocalizationService localizationService, IKnowledgebaseService knowledgebaseService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Name.Required"));
            RuleFor(x => x.ParentCategoryId).Must(x =>
            {
                var category = knowledgebaseService.GetKnowledgebaseCategory(x);
                if (category != null || string.IsNullOrEmpty(x))
                {
                    return true;
                }
                else
                {
                    var categories = knowledgebaseService.GetKnowledgebaseCategories();
                    if (!categories.Any())
                    {
                        return true;
                    }
                }

                return false;
            }).WithMessage(localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.ParentCategoryId.MustExist"));
        }
    }
}
