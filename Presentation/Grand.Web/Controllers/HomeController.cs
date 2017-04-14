using System.Web.Mvc;
using Grand.Web.Framework.Security;

namespace Grand.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        [GrandHttpsRequirement(SslRequirement.No)]
        public virtual ActionResult Index()
        {
            return View();
        }
    }
}
