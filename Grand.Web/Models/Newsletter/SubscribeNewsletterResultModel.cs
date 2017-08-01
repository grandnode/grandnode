using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Models.Newsletter
{
    public partial class SubscribeNewsletterResultModel: BaseGrandModel
    {
        public string Result { get; set; }
        public string ResultCategory { get; set; }
        public bool Success { get; set; }
        public bool ShowCategories { get; set; }
        public NewsletterCategoryModel NewsletterCategory { get; set; }
    }
}