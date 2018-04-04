using FluentValidation.Attributes;
using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Knowledgebase;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Models.Knowledgebase
{
    [Validator(typeof(KnowledgebaseArticleModelValidator))]
    public class KnowledgebaseArticleModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.Content")]
        public string Content { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.ParentCategoryId")]
        public string ParentCategoryId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public List<KnowledgebaseCategory> Categories { get; set; }
    }
}
