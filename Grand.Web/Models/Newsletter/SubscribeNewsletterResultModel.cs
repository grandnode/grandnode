using Grand.Core.Models;

namespace Grand.Web.Models.Newsletter
{
    public partial class SubscribeNewsletterResultModel: BaseModel
    {
        public string Result { get; set; }
        public string ResultCategory { get; set; }
        public bool Success { get; set; }
        public bool ShowCategories { get; set; }
        public NewsletterCategoryModel NewsletterCategory { get; set; }
    }
}