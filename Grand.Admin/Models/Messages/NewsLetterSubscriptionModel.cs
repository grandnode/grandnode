using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Messages
{
    public partial class NewsLetterSubscriptionModel : BaseEntityModel
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
        public string CreatedOn { get; set; }
    }
}