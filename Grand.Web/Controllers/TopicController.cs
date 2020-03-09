using Grand.Core;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class TopicController : BasePublicController
    {
        #region Fields
        private readonly ITopicViewModelService _topicViewModelService;
        private readonly ITopicService _topicService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Constructors

        public TopicController(ITopicService topicService,
            ITopicViewModelService topicViewModelService,
            ILocalizationService localizationService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            _topicService = topicService;
            _topicViewModelService = topicViewModelService;
            _localizationService = localizationService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _permissionService = permissionService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> TopicDetails(string topicId)
        {
            var model = await _topicViewModelService.TopicDetails(topicId);
            if (model == null)
                return RedirectToRoute("HomePage");

            //hide topic if it`s set as no published
            if(!model.Published
                && !(await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel)) 
                && !(await _permissionService.Authorize(StandardPermissionProvider.ManageTopics))) 
                return RedirectToRoute("HomePage");

            //template
            var templateViewPath = await _topicViewModelService.PrepareTopicTemplateViewPath(model.TopicTemplateId);

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                DisplayEditLink(Url.Action("Edit", "Topic", new { id = model.Id, area = "Admin" }));

            return View(templateViewPath, model);
        }

        public virtual async Task<IActionResult> TopicDetailsPopup(string systemName)
        {
            var model = await _topicViewModelService.TopicDetailsPopup(systemName);
            if (model == null)
                return RedirectToRoute("HomePage");

            //template
            var templateViewPath = await _topicViewModelService.PrepareTopicTemplateViewPath(model.TopicTemplateId);

            ViewBag.IsPopup = true;
            return View(templateViewPath, model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Authenticate(string id, string password)
        {
            var authResult = false;
            var title = string.Empty;
            var body = string.Empty;
            var error = string.Empty;

            var topic = await _topicService.GetTopicById(id);

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
                    title = topic.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id);
                    body = topic.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id);
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
