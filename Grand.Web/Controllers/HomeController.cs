using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        public virtual IActionResult Index()
        {
            return View();
        }
    }
}
