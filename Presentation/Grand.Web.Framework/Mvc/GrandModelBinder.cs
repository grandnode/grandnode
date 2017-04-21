using System.Web.Mvc;

namespace Grand.Web.Framework.Mvc
{
    public class GrandModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = base.BindModel(controllerContext, bindingContext);
            if (model is BaseGrandModel)
            {
                ((BaseGrandModel)model).BindModel(controllerContext, bindingContext);
            }
            return model;
        }
    }
}
