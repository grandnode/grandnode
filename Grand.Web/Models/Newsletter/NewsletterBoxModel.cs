using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Newsletter
{
    public partial class NewsletterBoxModel : BaseGrandModel
    {
        public string NewsletterEmail { get; set; }
        public bool AllowToUnsubscribe { get; set; }
    }
}