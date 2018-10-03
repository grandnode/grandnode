using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Settings;
using Grand.Framework.Localization;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    [Validator(typeof(ReturnRequestReasonValidator))]
    public partial class ReturnRequestReasonModel : BaseGrandEntityModel, ILocalizedModel<ReturnRequestReasonLocalizedModel>
    {
        public ReturnRequestReasonModel()
        {
            Locales = new List<ReturnRequestReasonLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestReasons.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestReasons.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<ReturnRequestReasonLocalizedModel> Locales { get; set; }
    }

    public partial class ReturnRequestReasonLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestReasons.Name")]
        
        public string Name { get; set; }

    }
}