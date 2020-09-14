﻿using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Plugins
{
    public partial class PluginModel : BaseModel, ILocalizedModel<PluginLocalizedModel>, IStoreMappingModel
    {
        public PluginModel()
        {
            Locales = new List<PluginLocalizedModel>();
            AvailableStores = new List<StoreModel>();
        }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.Group")]

        public string Group { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]

        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.SystemName")]

        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.Version")]

        public string Version { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.Author")]

        public string Author { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.Configure")]
        public string ConfigurationUrl { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.Installed")]
        public bool Installed { get; set; }

        public bool CanChangeEnabled { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.IsEnabled")]
        public bool IsEnabled { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.Logo")]
        public string LogoUrl { get; set; }

        public IList<PluginLocalizedModel> Locales { get; set; }


        //Store mapping
        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }
    }
    public partial class PluginLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]

        public string FriendlyName { get; set; }
    }
}