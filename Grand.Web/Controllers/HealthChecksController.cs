using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Controllers
{
    public class HealthChecksController: Controller
    {
        public virtual IActionResult Index()
        {
            return Content("Ok");
        }
    }
}
