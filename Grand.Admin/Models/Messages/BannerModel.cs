using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Messages
{
    public partial class BannerModel : BaseEntityModel, ILocalizedModel<BannerLocalizedModel>
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