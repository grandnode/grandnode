﻿using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Models.Stores;
using Grand.Admin.Validators.Directory;
using Grand.Web.Framework;
using Grand.Web.Framework.Localization;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Directory
{
    [Validator(typeof(CountryValidator))]
    public partial class CountryModel : BaseNopEntityModel, ILocalizedModel<CountryLocalizedModel>
    {
        public CountryModel()
        {
            Locales = new List<CountryLocalizedModel>();
        }
        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.AllowsBilling")]
        public bool AllowsBilling { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.AllowsShipping")]
        public bool AllowsShipping { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.TwoLetterIsoCode")]
        [AllowHtml]
        public string TwoLetterIsoCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.ThreeLetterIsoCode")]
        [AllowHtml]
        public string ThreeLetterIsoCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.NumericIsoCode")]
        public int NumericIsoCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.SubjectToVat")]
        public bool SubjectToVat { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }




        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.NumberOfStates")]
        public int NumberOfStates { get; set; }

        public IList<CountryLocalizedModel> Locales { get; set; }


        //Store mapping
        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }
    }

    public partial class CountryLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}