using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Security;
using Grand.Framework.Mvc.Filters;

namespace Grand.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult Index()
        {
            return View();
        }
    }
}
