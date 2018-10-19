using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Forums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ForumController : BaseAdminController
    {
        private readonly IForumService _forumService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public ForumController(IForumService forumService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            this._forumService = forumService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
        }

        #region List

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult ForumGroupList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroups = _forumService.GetAllForumGroups();
            var gridModel = new DataSourceResult
            {
                Data = forumGroups.Select(fg =>
                {
                    var model = fg.ToModel();
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(fg.CreatedOnUtc, DateTimeKind.Utc);
                    return model;
                }),
                Total = forumGroups.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ForumList(string forumGroupId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroup = _forumService.GetForumGroupById(forumGroupId);
            if (forumGroup == null)
                throw new Exception("Forum group cannot be loaded");

            var forums = _forumService.GetAllForumsByGroupId(forumGroup.Id);
            var gridModel = new DataSourceResult
            {
                Data = forums.Select(f =>
                {
                    var forumModel = f.ToModel();
                    forumModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(f.CreatedOnUtc, DateTimeKind.Utc);
                    return forumModel;
                }),
                Total = forums.Count
            };

            return Json(gridModel);
        }

        #endregion

        #region Create

        public IActionResult CreateForumGroup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            return View(new ForumGroupModel { DisplayOrder = 1 });
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateForumGroup(ForumGroupModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var forumGroup = model.ToEntity();
                forumGroup.CreatedOnUtc = DateTime.UtcNow;
                forumGroup.UpdatedOnUtc = DateTime.UtcNow;
                _forumService.InsertForumGroup(forumGroup);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Added"));
                return continueEditing ? RedirectToAction("EditForumGroup", new { forumGroup.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult CreateForum()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var model = new ForumModel();
            foreach (var forumGroup in _forumService.GetAllForumGroups())
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            model.DisplayOrder = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateForum(ForumModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var forum = model.ToEntity();
                forum.CreatedOnUtc = DateTime.UtcNow;
                forum.UpdatedOnUtc = DateTime.UtcNow;
                _forumService.InsertForum(forum);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Added"));
                return continueEditing ? RedirectToAction("EditForum", new { forum.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            foreach (var forumGroup in _forumService.GetAllForumGroups())
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        #endregion

        #region Edit

        public IActionResult EditForumGroup(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                //No forum group found with the specified id
                return RedirectToAction("List");

            var model = forumGroup.ToModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditForumGroup(ForumGroupModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroup = _forumService.GetForumGroupById(model.Id);
            if (forumGroup == null)
                //No forum group found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                forumGroup = model.ToEntity(forumGroup);
                forumGroup.UpdatedOnUtc = DateTime.UtcNow;
                _forumService.UpdateForumGroup(forumGroup);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Updated"));
                return continueEditing ? RedirectToAction("EditForumGroup", new { id = forumGroup.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult EditForum(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forum = _forumService.GetForumById(id);
            if (forum == null)
                //No forum found with the specified id
                return RedirectToAction("List");

            var model = forum.ToModel();
            foreach (var forumGroup in _forumService.GetAllForumGroups())
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditForum(ForumModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forum = _forumService.GetForumById(model.Id);
            if (forum == null)
                //No forum found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                forum = model.ToEntity(forum);
                forum.UpdatedOnUtc = DateTime.UtcNow;
                _forumService.UpdateForum(forum);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Updated"));
                return continueEditing ? RedirectToAction("EditForum", new { id = forum.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            foreach (var forumGroup in _forumService.GetAllForumGroups())
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        #endregion

        #region Delete

        [HttpPost]
        public IActionResult DeleteForumGroup(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                //No forum group found with the specified id
                return RedirectToAction("List");

            _forumService.DeleteForumGroup(forumGroup);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult DeleteForum(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forum = _forumService.GetForumById(id);
            if (forum == null)
                //No forum found with the specified id
                return RedirectToAction("List");

            _forumService.DeleteForum(forum);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Deleted"));
            return RedirectToAction("List");
        }

        #endregion
    }
}
