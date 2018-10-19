using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Directory;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Directory
{
    [Validator(typeof(StateProvinceValidator))]
    public partial class StateProvinceModel : BaseGrandEntityModel, ILocalizedModel<StateProvinceLocalizedModel>
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