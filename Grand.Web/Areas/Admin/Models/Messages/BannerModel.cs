using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Messages;
using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Framework.Localization;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(BannerValidator))]
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