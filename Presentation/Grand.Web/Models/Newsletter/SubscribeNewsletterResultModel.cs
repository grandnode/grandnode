using Grand.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Grand.Web.Models.Newsletter
{
    public partial class SubscribeNewsletterResultModel: BaseNopModel
    {
        public string Result { get; set; }
        public string ResultCategory { get; set; }
        public bool Success { get; set; }
        public bool ShowCategories { get; set; }
        public NewsletterCategoryModel NewsletterCategory { get; set; }
    }
}