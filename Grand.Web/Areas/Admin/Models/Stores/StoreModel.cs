using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Stores
{
    public partial class StoreModel : BaseGrandEntityModel, ILocalizedModel<StoreLocalizedModel>
    {
        public StoreModel()
        {
            Locales = new List<StoreLocalizedModel>();
            AvailableLanguages = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Shortcut")]
        public string Shortcut { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Url")]
        public string Url { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.SslEnabled")]
        public virtual bool SslEnabled { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.SecureUrl")]
        public virtual string SecureUrl { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Hosts")]
        public string Hosts { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyName")]
        public string CompanyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyAddress")]
        public string CompanyAddress { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyPhoneNumber")]
        public string CompanyPhoneNumber { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyVat")]
        public string CompanyVat { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyEmail")]
        public string CompanyEmail { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyHours")]
        public string CompanyHours { get; set; }

        public IList<StoreLocalizedModel> Locales { get; set; }
        //default language
        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultLanguage")]
        public string DefaultLanguageId { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }

        //default warehouse
        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultWarehouse")]
        public string DefaultWarehouseId { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }

        //default country
        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultCountry")]
        public string DefaultCountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

    }

    public partial class StoreLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Shortcut")]
        public string Shortcut { get; set; }
    }
}