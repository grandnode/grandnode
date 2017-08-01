using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;

        public FooterViewComponent(ICommonWebService commonWebService)
        {
            this._commonWebService = commonWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonWebService.PrepareFooter();
            return View(model);

        }
    }
}