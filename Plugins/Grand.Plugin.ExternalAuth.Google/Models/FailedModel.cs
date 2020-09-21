using Grand.Core.Models;

namespace Grand.Plugin.ExternalAuth.Google.Models
{
    public class FailedModel : BaseModel
    {
        public string ErrorMessage { get; set; }
    }
}
