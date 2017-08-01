using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Catalog;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    [Validator(typeof(SpecificationAttributeValidator))]
    public partial class SpecificationAttributeModel : BaseGrandEntityModel, ILocalizedModel<SpecificationAttributeLocalizedModel>
    {
        public SpecificationAttributeModel()
        {
            Locales = new List<SpecificationAttributeLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}


        public IList<SpecificationAttributeLocalizedModel> Locales { get; set; }

    }

    public partial class SpecificationAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        
        public string Name { get; set; }
    }
}