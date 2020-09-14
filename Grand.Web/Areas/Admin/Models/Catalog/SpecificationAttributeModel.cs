﻿using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class SpecificationAttributeModel : BaseEntityModel, ILocalizedModel<SpecificationAttributeLocalizedModel>
    {
        public SpecificationAttributeModel()
        {
            Locales = new List<SpecificationAttributeLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.SeName")]
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }


        public IList<SpecificationAttributeLocalizedModel> Locales { get; set; }

    }

    public partial class SpecificationAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        public string Name { get; set; }
    }
}