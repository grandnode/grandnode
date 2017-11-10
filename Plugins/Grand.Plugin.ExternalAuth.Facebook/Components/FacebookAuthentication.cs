using Microsoft.AspNetCore.Mvc;

namespace Grand.Plugin.ExternalAuth.Facebook.Components
{
    [ViewComponent(Name = "FacebookAuthentication")]
    public class FacebookAuthenticationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/ExternalAuth.Facebook/Views/PublicInfo.cshtml");
        }
    }
}