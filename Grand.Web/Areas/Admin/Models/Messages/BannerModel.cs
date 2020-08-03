using Grand.Framework.Localization;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class BannerModel : BaseGrandEntityModel, ILocalizedModel<BannerLocalizedModel>
    {
        public BannerModel()
        {
            Locales = new List<BannerLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Promotions.Banners.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Banners.Fields.Body")]

        public string Body { get; set; }

        public IList<BannerLocalizedModel> Locales { get; set; }

    }

    public partial class BannerLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Banners.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Banners.Fields.Body")]

        public string Body { get; set; }

    }

}