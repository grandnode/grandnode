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

        protected bool ValidRequest(string name, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                error = "Component name is empty";
                return false;
            }
            var component = _viewComponentSelector.SelectComponent(name);
            if (component == null)
            {
                error = "Component not exists";
                return false;
            }
            var attribute = component.TypeInfo.GetCustomAttribute<BaseViewComponentAttribute>();
            if (attribute == null || attribute.AdminAccess)
            {
                error = "Component - Attribute admin access limited";
                return false;
            }
            return true;
        }

        public IActionResult Index(string name, [FromBody] Dictionary<string, object> arguments)
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

            if (!ValidRequest(name, out var error))
                return Content(error);

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
                return ViewComponent(name, args);
            }
            return ViewComponent(name);
        }

        public IActionResult Form(string name)
        {

            /*
                Sample request:
                var form = new FormData();
                form.append('key', 'value');
                //To return Json use header: X-Response-View = Json
                    $.ajax({
                            cache: false,
                            type: "POST",
                            url: 'Component/Form?Name=HomePageProducts',
                            processData: false,
                            contentType: false,
                            data: form
                        }).done(function (data) {
                            console.log(data)
                    });
             */

            if (!ValidRequest(name, out var error))
                return Content(error);

            return ViewComponent(name);
        }
    }
}