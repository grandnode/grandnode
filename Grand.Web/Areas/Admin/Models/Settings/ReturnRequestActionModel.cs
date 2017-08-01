using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Settings;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    [Validator(typeof(ReturnRequestActionValidator))]
    public partial class ReturnRequestActionModel : BaseGrandEntityModel, ILocalizedModel<ReturnRequestActionLocalizedModel>
    {
        public ReturnRequestActionModel()
        {
            Locales = new List<ReturnRequestActionLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestActions.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestActions.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<ReturnRequestActionLocalizedModel> Locales { get; set; }
    }

    public partial class ReturnRequestActionLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestActions.Name")]
        
        public string Name { get; set; }

    }
}