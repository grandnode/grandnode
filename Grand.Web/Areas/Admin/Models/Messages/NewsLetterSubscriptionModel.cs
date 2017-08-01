using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Messages;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(NewsLetterSubscriptionValidator))]
    public partial class NewsLetterSubscriptionModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Email")]
        
        public string Email { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Active")]
        public bool Active { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Store")]
        public string StoreName { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Categories")]
        public string Categories { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}