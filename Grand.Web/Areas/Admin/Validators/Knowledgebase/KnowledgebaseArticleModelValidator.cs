using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Validators.Knowledgebase
{
    public class KnowledgebaseArticleModelValidator : BaseGrandValidator<KnowledgebaseArticleModel>
    {
        public KnowledgebaseArticleModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Title.Required"));
        }
    }
}
