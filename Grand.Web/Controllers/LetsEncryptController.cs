using Grand.Core;
using Grand.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Grand.Web.Controllers
{
    [AllowAnonymous]
    public partial class LetsEncryptController : Controller
    {
        private readonly CommonSettings _commonSettings;

        public LetsEncryptController(CommonSettings commonSettings)
        {
            _commonSettings = commonSettings;
        }
        public virtual IActionResult Index(string fileName)
        {
            if(!_commonSettings.AllowToReadLetsEncryptFile)
                return Content("");

            if (fileName == null)
                return Content("");

            var file = CommonHelper.MapPath("~/wwwroot/content/acme/") + Path.GetFileName(fileName);
            if (System.IO.File.Exists(file))
            {
                return new PhysicalFileResult(file, "text/plain");
            }
            return Content("");

        }

    }
}
