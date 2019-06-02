using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.ExternalAuth.Google.Models
{
    public class FailedModel : BaseGrandModel
    {
        public string ErrorMessage { get; set; }
    }
}
