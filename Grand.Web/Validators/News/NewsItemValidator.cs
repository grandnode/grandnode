using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.News;
using System.Collections.Generic;

namespace Grand.Web.Validators.News
{
    public class NewsItemValidator : BaseGrandValidator<NewsItemModel>
    {
        public NewsItemValidator(
            IEnumerable<IValidatorConsumer<NewsItemModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.AddNewComment.CommentTitle).NotEmpty().WithMessage(localizationService.GetResource("News.Comments.CommentTitle.Required")).When(x => x.AddNewComment != null);
            RuleFor(x => x.AddNewComment.CommentTitle).Length(1, 200).WithMessage(string.Format(localizationService.GetResource("News.Comments.CommentTitle.MaxLengthValidation"), 200)).When(x => x.AddNewComment != null && !string.IsNullOrEmpty(x.AddNewComment.CommentTitle));
            RuleFor(x => x.AddNewComment.CommentText).NotEmpty().WithMessage(localizationService.GetResource("News.Comments.CommentText.Required")).When(x => x.AddNewComment != null);
        }}
}