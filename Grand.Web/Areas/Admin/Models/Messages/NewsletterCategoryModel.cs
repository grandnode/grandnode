using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class NewsletterCategoryModel : BaseGrandEntityModel, ILocalizedModel<NewsletterCategoryLocalizedModel>, IStoreMappingModel
    {
        public NewsletterCategoryModel()
        {
            Locales = new List<NewsletterCategoryLocalizedModel>();
            AvailableStores = new List<StoreModel>();
        }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.Description")]

        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.Selected")]
        public bool Selected { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        public string[] SelectedStoreIds { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        public IList<NewsletterCategoryLocalizedModel> Locales { get; set; }
    }

    public partial class NewsletterCategoryLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsletterCategory.Fields.Description")]

        public string Description { get; set; }

    }
}