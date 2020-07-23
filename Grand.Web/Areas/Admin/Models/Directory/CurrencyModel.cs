using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Directory
{
    public partial class CurrencyModel : BaseGrandEntityModel, ILocalizedModel<CurrencyLocalizedModel>, IStoreMappingModel
    {
        public CurrencyModel()
        {
            Locales = new List<CurrencyLocalizedModel>();
            AvailableStores = new List<StoreModel>();
        }
        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.CurrencyCode")]

        public string CurrencyCode { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.DisplayLocale")]

        public string DisplayLocale { get; set; }

        [UIHint("DecimalN4")]
        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.Rate")]
        public decimal Rate { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.CustomFormatting")]

        public string CustomFormatting { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.Published")]
        public bool Published { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.IsPrimaryExchangeRateCurrency")]
        public bool IsPrimaryExchangeRateCurrency { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.IsPrimaryStoreCurrency")]
        public bool IsPrimaryStoreCurrency { get; set; }

        public IList<CurrencyLocalizedModel> Locales { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.RoundingType")]
        public int RoundingTypeId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.MidpointRound")]
        public int MidpointRoundId { get; set; }

        //Store mapping
        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }
    }

    public partial class CurrencyLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Currencies.Fields.Name")]

        public string Name { get; set; }
    }
}