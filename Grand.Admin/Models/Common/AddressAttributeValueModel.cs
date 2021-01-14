using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Common
{
    public partial class AddressAttributeValueModel : BaseEntityModel, ILocalizedModel<AddressAttributeValueLocalizedModel>
    {
        public AddressAttributeValueModel()
        {
            Locales = new List<AddressAttributeValueLocalizedModel>();
        }

        public string AddressAttributeId { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<AddressAttributeValueLocalizedModel> Locales { get; set; }

    }

    public partial class AddressAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Address.AddressAttributes.Values.Fields.Name")]

        public string Name { get; set; }
    }
}