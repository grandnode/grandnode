using Grand.Core;
using Grand.Framework.Components;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class FaviconViewComponent : BaseViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IWebHelper _webHelper;

        public FaviconViewComponent(IStoreContext storeContext, IWebHostEnvironment hostingEnvironment,
            IWebHelper webHelper)
        {
            _storeContext = storeContext;
            _hostingEnvironment = hostingEnvironment;
            _webHelper = webHelper;
        }

        public IViewComponentResult Invoke()
        {
            var model = PrepareFavicon();
            if (string.IsNullOrEmpty(model.FaviconUrl))
                return Content("");

            return View(model);
        }

        private FaviconModel PrepareFavicon()
        {
            var model = new FaviconModel();
            var faviconFileName = string.Format("favicon-{0}.ico", _storeContext.CurrentStore.Id);
            var localFaviconPath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, faviconFileName);
            if (!System.IO.File.Exists(localFaviconPath))
            {
                //try loading a generic favicon
                faviconFileName = "favicon.ico";
                localFaviconPath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, faviconFileName);
                if (!System.IO.File.Exists(localFaviconPath))
                {
                    return model;
                }
            }
            model.FaviconUrl = _webHelper.GetStoreLocation() + faviconFileName;
            return model;
        }

    }
}