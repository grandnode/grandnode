using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Knowledgebase;

namespace Grand.Web.Validators.Knowledgebase
{
    public class KnowledgebaseArticleValidator : BaseGrandValidator<KnowledgebaseArticleModel>
    {
        public KnowledgebaseArticleValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.AddNewComment.CommentText).NotEmpty().WithMessage(localizationService.GetResource("Grand.knowledgebase.addarticlecomment.result")).When(x => x.AddNewComment != null);
        }
    }
}