using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators;
using Grand.Web.Areas.Admin.Validators.Knowledgebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Models.Knowledgebase
{
    [Validator(typeof(KnowledgebaseCategoryModelValidator))]
    public class KnowledgebaseCategoryModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Fields.ParentCategoryId")]
        public string ParentCategoryId { get; set; }
    }
}
