using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Knowledgebase;

namespace Grand.Web.Areas.Admin.Validators.Knowledgebase
{
    public class KnowledgebaseArticleModelValidator : BaseGrandValidator<KnowledgebaseArticleModel>
    {
        public KnowledgebaseArticleModelValidator(ILocalizationService localizationService, IKnowledgebaseService knowledgebaseService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Name.Required"));
            RuleFor(x => x.ParentCategoryId).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.ParentCategoryId.Required"));
            RuleFor(x => x.ParentCategoryId).Must(x =>
            {
                var category = knowledgebaseService.GetKnowledgebaseCategory(x);
                if (category != null)
                {
                    return true;
                }

                return false;
            }).WithMessage(localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.ParentCategoryId.MustExist"));
        }
    }
}
