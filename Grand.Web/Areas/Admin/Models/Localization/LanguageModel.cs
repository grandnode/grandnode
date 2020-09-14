﻿using Grand.Framework.Mapping;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Localization
{
    public partial class LanguageModel : BaseEntityModel, IStoreMappingModel
    {
        public LanguageModel()
        {
            FlagFileNames = new List<string>();
            AvailableCurrencies = new List<SelectListItem>();
            Search = new LanguageResourceFilterModel();
            AvailableStores = new List<StoreModel>();
        }
        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.LanguageCulture")]

        public string LanguageCulture { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.UniqueSeoCode")]

        public string UniqueSeoCode { get; set; }

        //flags
        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.FlagImageFileName")]

        public string FlagImageFileName { get; set; }
        public IList<string> FlagFileNames { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.Rtl")]
        public bool Rtl { get; set; }

        //default currency
        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.DefaultCurrency")]

        public string DefaultCurrencyId { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }


        //Store mapping
        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Languages.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        public LanguageResourceFilterModel Search { get; set; }
    }
}