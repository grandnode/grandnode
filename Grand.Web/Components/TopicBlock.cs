using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            _topicViewModelService = topicViewModelService;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(string systemName)
        {
            var model = await _topicViewModelService.TopicBlock(systemName);
            if (model == null)
                return Content("");

            return View(model);
        }

        #endregion

    }
}
