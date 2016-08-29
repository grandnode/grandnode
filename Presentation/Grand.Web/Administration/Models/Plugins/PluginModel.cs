﻿using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Models.Stores;
using Grand.Admin.Validators.Plugins;
using Grand.Web.Framework;
using Grand.Web.Framework.Localization;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Plugins
{
    [Validator(typeof(PluginValidator))]
    public partial class PluginModel : BaseNopModel, ILocalizedModel<PluginLocalizedModel>
    {
        public PluginModel()
        {
            Locales = new List<PluginLocalizedModel>();
        }
        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.Group")]
        [AllowHtml]
        public string Group { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
        [AllowHtml]
        public string FriendlyName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.SystemName")]
        [AllowHtml]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.Version")]
        [AllowHtml]
        public string Version { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.Author")]
        [AllowHtml]
        public string Author { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.Configure")]
        public string ConfigurationUrl { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.Installed")]
        public bool Installed { get; set; }

        public bool CanChangeEnabled { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.IsEnabled")]
        public bool IsEnabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.Logo")]
        public string LogoUrl { get; set; }

        public IList<PluginLocalizedModel> Locales { get; set; }


        //Store mapping
        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }
    }
    public partial class PluginLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
        [AllowHtml]
        public string FriendlyName { get; set; }
    }
}