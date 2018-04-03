using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Knowledgebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Models.Knowledgebase
{
    [Validator(typeof(KnowledgebaseArticleModelValidator))]
    public class KnowledgebaseArticleModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.Article.Fields.Titles")]
        public string Title { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.Article.Fields.Content")]
        public string Content { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.Article.Fields.ParentCategoryId")]
        public string ParentCategoryId { get; set; }
    }
}
