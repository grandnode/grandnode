using Microsoft.AspNetCore.Mvc;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Services;

namespace Grand.Web.Controllers
{
    public partial class TopicController : BasePublicController
    {
        #region Fields
        private readonly ITopicWebService _topicWebService;
        private readonly ITopicService _topicService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public TopicController(ITopicService topicService,
            ITopicWebService topicWebService,
            ILocalizationService localizationService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IPermissionService permissionService)
        {
            this._topicService = topicService;
            this._topicWebService = topicWebService;
            this._localizationService = localizationService;
            this._storeMappingService = storeMappingService;
            this._aclService = aclService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Methods

        public virtual IActionResult TopicDetails(string topicId)
        {
            var model = _topicWebService.TopicDetails(topicId);
            if (model == null)
                return RedirectToRoute("HomePage");

            //template
            var templateViewPath = _topicWebService.PrepareTopicTemplateViewPath(model.TopicTemplateId);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                DisplayEditLink(Url.Action("Edit", "Topic", new { id = model.Id, area = "Admin" }));

            return View(templateViewPath, model);
        }

        public virtual IActionResult TopicDetailsPopup(string systemName)
        {
            var model = _topicWebService.TopicDetailsPopup(systemName);
            if (model == null)
                return RedirectToRoute("HomePage");

            //template
            var templateViewPath = _topicWebService.PrepareTopicTemplateViewPath(model.TopicTemplateId);

            ViewBag.IsPopup = true;
            return View(templateViewPath, model);
        }

        [HttpPost]
        public virtual IActionResult Authenticate(string id, string password)
        {
            var authResult = false;
            var title = string.Empty;
            var body = string.Empty;
            var error = string.Empty;

            var topic = _topicService.GetTopicById(id);

            if (topic != null &&
                //password protected?
                topic.IsPasswordProtected &&
                //store mapping
                _storeMappingService.Authorize(topic) &&
                //ACL (access control list)
                _aclService.Authorize(topic))
            {
                if (topic.Password != null && topic.Password.Equals(password))
                {
                    authResult = true;
                    title = topic.GetLocalized(x => x.Title);
                    body = topic.GetLocalized(x => x.Body);
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
