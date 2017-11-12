using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class TopicBlockViewComponent : ViewComponent
    {
        #region Fields
        private readonly ITopicWebService _topicWebService;
        #endregion

        #region Constructors

        public TopicBlockViewComponent(
            ITopicWebService topicWebService
)
        {
            this._topicWebService = topicWebService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(string systemName)
        {
            var model = _topicWebService.TopicBlock(systemName);
            if (model == null)
                return Content("");
            return View(model);
        }

        #endregion

    }
}
