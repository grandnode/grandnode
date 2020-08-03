using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Blogs;
using System.Collections.Generic;

namespace Grand.Web.Validators.Blogs
{
    public class BlogPostValidator : BaseGrandValidator<BlogPostModel>
    {
        public BlogPostValidator(
            IEnumerable<IValidatorConsumer<BlogPostModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.AddNewComment.CommentText).NotEmpty().WithMessage(localizationService.GetResource("Blog.Comments.CommentText.Required")).When(x => x.AddNewComment != null);
        }}
}