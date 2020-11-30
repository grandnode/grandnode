using Grand.Framework.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Collections.Generic;
using System.Reflection;

namespace Grand.Web.Controllers
{
    public class ComponentController : BasePublicController
    {
        private readonly IViewComponentSelector _viewComponentSelector;

        public ComponentController(IViewComponentSelector viewComponentSelector)
        {
            _viewComponentSelector = viewComponentSelector;
        }

        public IActionResult Index([FromQuery] string name, [FromBody] Dictionary<string, object> arguments)
        {

            /*
                Sample request:
                //To return Json use header: X-Response-View = Json
                var data = { productThumbPictureSize: 10};
                    $.ajax({
                            cache: false,
                            type: "POST",
                            url: 'Component/Index?Name=HomePageProducts',
                            contentType: "application/json",
                            data: JSON.stringify(data)
                        }).done(function (data) {
                            console.log(data)
                    });
             */

            if (string.IsNullOrEmpty(name))
                return Content("");

            var component = _viewComponentSelector.SelectComponent(name);
            if (component == null)
                return Content("Component not exists");

            var attribute = component.TypeInfo.GetCustomAttribute<BaseViewComponentAttribute>();
            if (attribute == null || attribute.AdminAccess)
                return Content("Component - Attribute admin access limited");

            string content;

            if (arguments != null)
            {
                var args = new Dictionary<string, object>();
                foreach (var arg in arguments)
                {
                    var key = arg.Key;
                    var value = arg.Value;
                    if (arg.Value is long)
                    {
                        int.TryParse(arg.Value.ToString(), out var parsevalue);
                        args.Add(key, parsevalue);
                    }
                    else
                        args.Add(key, value);
                }
                content = RenderViewComponentToString(name, args);
            }
            else
                content = RenderViewComponentToString(name);

            var viewComponentJsonModel = TempData["ViewComponentJsonModel"];
            if (viewComponentJsonModel != null)
                return Json(viewComponentJsonModel);

            return Content(content);
        }

    }
}
