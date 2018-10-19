using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Messages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(InteractiveFormAttributeValidator))]
    public partial class InteractiveFormAttributeModel : BaseGrandEntityModel, ILocalizedModel<InteractiveFormAttributeLocalizedModel>
    {
        public InteractiveFormAttributeModel()
        {
            Locales = new List<InteractiveFormAttributeLocalizedModel>();
        }
        public string FormId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.SystemName")]
        public string SystemName { get; set; }

        
        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.RegexValidation")]
        public string RegexValidation { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.FormControlTypeId")]
        public int FormControlTypeId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.ValidationMinLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMinLength { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.ValidationMaxLength")]
        [UIHint("Int32Nullable")]
        public int? ValidationMaxLength { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.DefaultValue")]
        public string DefaultValue { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.Style")]
        public string Style { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.Class")]
        public string Class { get; set; }

        public IList<InteractiveFormAttributeLocalizedModel> Locales { get; set; }

    }

    public partial class InteractiveFormAttributeLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.InteractiveForms.Attribute.Fields.Name")]
        public string Name { get; set; }

    }

}