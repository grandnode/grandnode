using Grand.Core;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Features.Models.Topics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class TopicController : BasePublicController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IMediator _mediator;

        #endregion

        #region Constructors

        public TopicController(
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IMediator mediator)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _workContext = workContext;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> TopicDetails(string topicId)
        {
            if (string.IsNullOrEmpty(topicId))
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetTopicBlock() { TopicId = topicId });
            if (model == null)
                return RedirectToRoute("HomePage");

            //hide topic if it`s set as no published
            if (!model.Published
                && !(await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                && !(await _permissionService.Authorize(StandardPermissionProvider.ManageTopics)))
                return RedirectToRoute("HomePage");

            //template
            var templateViewPath = await _mediator.Send(new GetTopicTemplateViewPath() { TemplateId = model.TopicTemplateId });

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                DisplayEditLink(Url.Action("Edit", "Topic", new { id = model.Id, area = "Admin" }));

            return View(templateViewPath, model);
        }

        public virtual async Task<IActionResult> TopicDetailsPopup(string systemName)
        {
            var model = await _mediator.Send(new GetTopicBlock() { SystemName = systemName });
            if (model == null)
                return RedirectToRoute("HomePage");

            //template
            var templateViewPath = await _mediator.Send(new GetTopicTemplateViewPath() { TemplateId = model.TopicTemplateId });

            ViewBag.IsPopup = true;
            return View(templateViewPath, model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Authenticate(string id, string password)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { Authenticated = false, Error = "Empty id" });

            var authResult = false;
            var title = string.Empty;
            var body = string.Empty;
            var error = string.Empty;

            var topic = await _mediator.Send(new GetTopicBlock() { TopicId = id });

            if (topic != null &&
                //password protected?
                topic.IsPasswordProtected)
            {
                if (topic.Password != null && topic.Password.Equals(password))
                {
                    authResult = true;
                    title = topic.Title;
                    body = topic.Body;
                }
                else
                {
                    error = _localizationService.GetResource("Topic.WrongPassword");
                }
            }
            return Json(new { Authenticated = authResult, Title = title, Body = body, Error = error });
        }

        #endregion
    }
}
