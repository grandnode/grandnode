using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class TopicBlockViewComponent : BaseViewComponent
    {
        #region Fields
        private readonly ITopicViewModelService _topicViewModelService;
        #endregion

        #region Constructors

        public TopicBlockViewComponent(
            ITopicViewModelService topicViewModelService
)
        {
            this._topicViewModelService = topicViewModelService;
        }

        #endregion

        #region Invoker

        public IViewComponentResult Invoke(string systemName)
        {
            var model = _topicViewModelService.TopicBlock(systemName);
            if (model == null)
                return Content("");
            return View(model);
        }

        #endregion

    }
}
