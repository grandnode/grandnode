using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Settings;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    [Validator(typeof(SettingValidator))]
    public partial class SettingModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.Value")]
        
        public string Value { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.StoreName")]
        public string Store { get; set; }
        public string StoreId { get; set; }
    }
}