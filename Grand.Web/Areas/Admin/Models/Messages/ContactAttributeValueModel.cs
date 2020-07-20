using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class ContactAttributeValueModel : BaseGrandEntityModel, ILocalizedModel<ContactAttributeValueLocalizedModel>
    {
        public ContactAttributeValueModel()
        {
            Locales = new List<ContactAttributeValueLocalizedModel>();
        }

        public string ContactAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Values.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Values.Fields.ColorSquaresRgb")]
        public string ColorSquaresRgb { get; set; }
        public bool DisplayColorSquaresRgb { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<ContactAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class ContactAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Attributes.ContactAttributes.Values.Fields.Name")]
        public string Name { get; set; }
    }
}