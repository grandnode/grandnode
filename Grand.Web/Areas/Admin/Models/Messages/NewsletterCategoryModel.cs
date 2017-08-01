using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Models.Stores;
using Grand.Web.Areas.Admin.Validators.Messages;
using Grand.Framework;
using Grand.Framework.Localization;
using Grand.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;



namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(NewsletterCategoryValidator))]
    public partial class NewsletterCategoryModel: BaseGrandEntityModel, ILocalizedModel<NewsletterCategoryLocalizedModel>
    {
        public NewsletterCategoryModel()
        {
            Locales = new List<NewsletterCategoryLocalizedModel>();
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