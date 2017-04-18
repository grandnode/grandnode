using System.Web.Mvc;
using Grand.Web.Services;

namespace Grand.Web.Controllers
{
    public partial class WidgetController : BasePublicController
    {
        #region Fields

        private readonly IWidgetWebService _widgetWebService;

        #endregion

        #region Constructors

        public WidgetController(IWidgetWebService widgetWebService)
        {
            this._widgetWebService = widgetWebService;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public virtual ActionResult WidgetsByZone(string widgetZone, object additionalData = null)
        {
            var model = _widgetWebService.PrepareRenderWidget(widgetZone, additionalData);
            if (model == null)
                return Content("");

            return PartialView(model);
        }

        #endregion
    }
}
