using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.Directory;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Directory
{
    [Validator(typeof(CountryValidator))]
    public partial class CountryModel : BaseGrandEntityModel, ILocalizedModel<CountryLocalizedModel>
    {
        public CountryModel()
        {
            Locales = new List<CountryLocalizedModel>();
        }
        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.AllowsBilling")]
        public bool AllowsBilling { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.AllowsShipping")]
        public bool AllowsShipping { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.TwoLetterIsoCode")]
        
        public string TwoLetterIsoCode { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.ThreeLetterIsoCode")]
        
        public string ThreeLetterIsoCode { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.NumericIsoCode")]
        public int NumericIsoCode { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.SubjectToVat")]
        public bool SubjectToVat { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }




        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.NumberOfStates")]
        public int NumberOfStates { get; set; }

        public IList<CountryLocalizedModel> Locales { get; set; }


        //Store mapping
        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }
    }

    public partial class CountryLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        
        public string Name { get; set; }
    }
}