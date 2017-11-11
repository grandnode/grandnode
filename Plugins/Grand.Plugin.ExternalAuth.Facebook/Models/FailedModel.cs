using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.ExternalAuth.Facebook.Models
{
    public class FailedModel : BaseGrandModel
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
