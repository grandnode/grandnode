using FluentValidation.Attributes;
using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Stores;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Stores
{
    [Validator(typeof(StoreValidator))]
    public partial class StoreModel : BaseGrandEntityModel, ILocalizedModel<StoreLocalizedModel>
    {
        public StoreModel()
        {
            Locales = new List<StoreLocalizedModel>();
            AvailableLanguages = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        
        public string Name { get; set; }

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


        public IList<StoreLocalizedModel> Locales { get; set; }
        //default language
        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultLanguage")]
        
        public string DefaultLanguageId { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }

        //default warehouse
        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultWarehouse")]
        
        public string DefaultWarehouseId { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }

    }

    public partial class StoreLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        
        public string Name { get; set; }
    }
}