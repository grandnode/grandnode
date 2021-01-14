using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.ExternalAuthentication
{
    public partial class AuthenticationMethodModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.FriendlyName")]
        
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.SystemName")]
        
        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.IsActive")]
        public bool IsActive { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.Configure")]
        public string ConfigurationUrl { get; set; }
    }
}