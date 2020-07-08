using Grand.Core;
using Grand.Domain.Media;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller used by jbimages (JustBoil.me) plugin (TimyMCE)
    /// </summary>
    //do not validate request token (XSRF)
    [IgnoreAntiforgeryToken]
    public partial class JbimagesController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly MediaSettings _mediaSettings;

        public JbimagesController(IPermissionService permissionService, MediaSettings mediaSettings)
        {
            _permissionService = permissionService;
            _mediaSettings = mediaSettings;
        }

        [NonAction]
        protected virtual IList<string> GetAllowedFileTypes()
        {
            if (string.IsNullOrEmpty(_mediaSettings.AllowedFileTypes))
                return new List<string> { ".gif", ".jpg", ".jpeg", ".png", ".bmp", ".webp" };
            else
                return _mediaSettings.AllowedFileTypes.Split(',');
        }

        [HttpPost]
        public virtual async Task<IActionResult> Upload()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.HtmlEditorManagePictures))
            {
                ViewData["resultCode"] = "failed";
                ViewData["result"] = "No access to this functionality";
                return View();
            }
            var form = await HttpContext.Request.ReadFormAsync();
            if (form.Files.Count == 0)
                throw new Exception("No file uploaded");

            var uploadFile = form.Files.FirstOrDefault();
            if (uploadFile == null)
            {
                ViewData["resultCode"] = "failed";
                ViewData["result"] = "No file name provided";
                return View();
            }

            var fileName = Path.GetFileName(uploadFile.FileName);
            if (String.IsNullOrEmpty(fileName))
            {
                ViewData["resultCode"] = "failed";
                ViewData["result"] = "No file name provided";
                return View();
            }

            var directory = "~/wwwroot/content/images/uploaded/";
            var filePath = Path.Combine(CommonHelper.MapPath(directory), fileName);

            var fileExtension = Path.GetExtension(filePath);
            if (!GetAllowedFileTypes().Contains(fileExtension))
            {
                ViewData["resultCode"] = "failed";
                ViewData["result"] = string.Format("Files with {0} extension cannot be uploaded", fileExtension);
                return View();
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                uploadFile.CopyTo(fileStream);
            }

            ViewData["resultCode"] = "success";
            ViewData["result"] = "success";
            ViewData["filename"] = this.Url.Content(string.Format("{0}{1}", directory, fileName));
            return View();
        }
    }
}