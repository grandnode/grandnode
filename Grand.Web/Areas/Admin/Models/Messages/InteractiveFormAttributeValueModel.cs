using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Messages;
using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Framework.Localization;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(InteractiveFormAttributeValueValidator))]
    public partial class InteractiveFormAttributeValueModel : BaseGrandEntityModel, ILocalizedModel<InteractiveFormAttributeValueLocalizedModel>
    {
        public InteractiveFormAttributeValueModel()
        {
            Locales = new List<InteractiveFormAttributeValueLocalizedModel>();
        }
        public string FormId { get; set; }
        public string AttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Values.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Values.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        public IList<InteractiveFormAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class InteractiveFormAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Values.Fields.Name")]
        public string Name { get; set; }

    }

}