using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Polls;
using Nop.Core.Domain.Polls;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Polls;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using MongoDB.Bson;

namespace Nop.Admin.Controllers
{
    public partial class PollController : BaseAdminController
	{
		#region Fields

        private readonly IPollService _pollService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

		#endregion

		#region Constructors

        public PollController(IPollService pollService, ILanguageService languageService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            this._pollService = pollService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
		}

		#endregion 
        
        #region Polls

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var polls = _pollService.GetPolls(0, false, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = polls.Select(x =>
                {
                    var m = x.ToModel();
                    var lang = _languageService.GetLanguageById(m.LanguageId);
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.LanguageName = lang.Name;
                    return m;
                }),
                Total = polls.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new PollModel();
            //default values
            model.Published = true;
            model.ShowOnHomePage = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(PollModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var poll = model.ToEntity();
                poll.StartDateUtc = model.StartDate;
                poll.EndDateUtc = model.EndDate;
                _pollService.InsertPoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = poll.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var poll = _pollService.GetPollById(id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = poll.ToModel();
            model.StartDate = poll.StartDateUtc;
            model.EndDate = poll.EndDateUtc;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(PollModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var poll = _pollService.GetPollById(model.Id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                poll = model.ToEntity(poll);
                poll.StartDateUtc = model.StartDate;
                poll.EndDateUtc = model.EndDate;
                _pollService.UpdatePoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = poll.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var poll = _pollService.GetPollById(id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");
            
            _pollService.DeletePoll(poll);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Poll answer

        [HttpPost]
        public ActionResult PollAnswers(int pollId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var poll = _pollService.GetPollById(pollId);
            if (poll == null)
                throw new ArgumentException("No poll found with the specified id", "pollId");

            var answers = poll.PollAnswers.OrderBy(x=>x.DisplayOrder).ToList();

            var gridModel = new DataSourceResult
            {
                Data = answers.Select(x =>  new PollAnswerModel
                {
                    Id = x.Id,
                    PollId = x.PollId,
                    Name = x.Name,
                    NumberOfVotes = x.NumberOfVotes,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = answers.Count
            };

            return Json(gridModel);
        }


        [HttpPost]
        public ActionResult PollAnswerUpdate(PollAnswerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();
            
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }
            var poll = _pollService.GetPollById(model.PollId);
            var pollAnswer = poll.PollAnswers.FirstOrDefault(x=>x.Id == model.Id);
            if (pollAnswer == null)
                throw new ArgumentException("No poll answer found with the specified id");
            pollAnswer._id = ObjectId.GenerateNewId().ToString();
            pollAnswer.Id = poll.PollAnswers.Count > 0 ? poll.PollAnswers.Max(x => x.Id) + 1 : 1;
            pollAnswer.Name = model.Name;
            pollAnswer.DisplayOrder = model.DisplayOrder;
            _pollService.UpdatePoll(poll);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult PollAnswerAdd(int pollId, [Bind(Exclude = "Id")] PollAnswerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();
           
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var poll = _pollService.GetPollById(pollId);
            if (poll == null)
                throw new ArgumentException("No poll found with the specified id", "pollId");

            poll.PollAnswers.Add(new PollAnswer 
            {
                _id = ObjectId.GenerateNewId().ToString(),
                Id = poll.PollAnswers.Count > 0 ? poll.PollAnswers.Max(x => x.Id) + 1 : 1,
                Name = model.Name,
                PollId = pollId,
                DisplayOrder = model.DisplayOrder
            });
            _pollService.UpdatePoll(poll);

            return new NullJsonResult();
        }


        [HttpPost]
        public ActionResult PollAnswerDelete(PollAnswer answer)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var pol = _pollService.GetPollById(answer.PollId);
            var pollAnswer = pol.PollAnswers.Where(x => x.Id == answer.Id).FirstOrDefault();
            if (pollAnswer == null)
                throw new ArgumentException("No poll answer found with the specified id", "id");

            pol.PollAnswers.Remove(pollAnswer);
            _pollService.UpdatePoll(pol);

            return new NullJsonResult();
        }

        #endregion
    }
}
