using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class ReturnRequestReasonModel : BaseEntityModel, ILocalizedModel<ReturnRequestReasonLocalizedModel>
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