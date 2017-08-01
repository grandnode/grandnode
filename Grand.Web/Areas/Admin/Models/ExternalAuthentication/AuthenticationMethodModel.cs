using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;

using Grand.Framework;
using Grand.Framework.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Areas.Admin.Models.ExternalAuthentication
{
    public partial class AuthenticationMethodModel : BaseGrandModel
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