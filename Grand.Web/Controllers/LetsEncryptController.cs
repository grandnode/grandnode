using Grand.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Grand.Web.Controllers
{
    [AllowAnonymous]
    public partial class LetsEncryptController : Controller
    {
        public virtual IActionResult Index(string fileName)
        {
            if (fileName == null)
                return Content("");

            var file = Path.Combine(CommonHelper.MapPath("~/wwwroot/content/acme/"), fileName);
            if (System.IO.File.Exists(file))
            {
                return new PhysicalFileResult(file, "text/plain");
            }
            return Content("");

        }

    }
}
