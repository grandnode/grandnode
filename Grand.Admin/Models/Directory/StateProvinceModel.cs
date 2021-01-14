using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Directory
{
    public partial class StateProvinceModel : BaseEntityModel, ILocalizedModel<StateProvinceLocalizedModel>
    {
        public StateProvinceModel()
        {
            Locales = new List<StateProvinceLocalizedModel>();
        }
        public string CountryId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.States.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.States.Fields.Abbreviation")]

        public string Abbreviation { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.States.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.States.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<StateProvinceLocalizedModel> Locales { get; set; }
    }

    public partial class StateProvinceLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.States.Fields.Name")]

        public string Name { get; set; }
    }
}