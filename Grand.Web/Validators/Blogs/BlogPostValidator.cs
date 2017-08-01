using FluentValidation;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Web.Models.Blogs;

namespace Grand.Web.Validators.Blogs
{
    public class BlogPostValidator : BaseGrandValidator<BlogPostModel>
    {
        public BlogPostValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.AddNewComment.CommentText).NotEmpty().WithMessage(localizationService.GetResource("Blog.Comments.CommentText.Required")).When(x => x.AddNewComment != null);
        }}
}