using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Blogs;

namespace Grand.Web.Areas.Admin.Validators.Blogs
{
    public class BlogCategoryValidator : BaseGrandValidator<BlogCategoryModel>
    {
        public BlogCategoryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Fields.Name.Required"));
        }
    }
}