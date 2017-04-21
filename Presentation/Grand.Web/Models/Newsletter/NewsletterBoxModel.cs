using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Newsletter
{
    public partial class NewsletterBoxModel : BaseGrandModel
    {
        public string NewsletterEmail { get; set; }
        public bool AllowToUnsubscribe { get; set; }
    }
}