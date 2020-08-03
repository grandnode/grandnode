using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Blogs;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Blogs
{
    public class BlogCategoryValidator : BaseGrandValidator<BlogCategoryModel>
    {
        public BlogCategoryValidator(IEnumerable<IValidatorConsumer<BlogCategoryModel>> validators, 
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Fields.Name.Required"));
        }
    }
}