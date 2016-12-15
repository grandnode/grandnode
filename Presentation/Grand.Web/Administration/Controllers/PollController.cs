using System;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Polls;
using Grand.Core.Domain.Polls;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Polls;
using Grand.Services.Security;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using MongoDB.Bson;
using Grand.Core.Domain.Localization;
using System.Collections.Generic;
using Grand.Services.Stores;

namespace Grand.Admin.Controllers
{
    public partial class PollController : BaseAdminController
	{
		#region Fields

        private readonly IPollService _pollService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        #endregion

        #region Constructors

        public PollController(IPollService pollService, ILanguageService languageService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService, IStoreService storeService)
        {
            this._pollService = pollService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._storeService = storeService;

        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Poll poll, PollModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name
                    });

            }
            return localized;
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateOptionLocales(PollAnswer pollanswer, PollAnswerModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });
            }
            return localized;
        }


        [NonAction]
        protected virtual void PrepareStoresMappingModel(PollModel model, Poll poll, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (poll != null)
                {
                    model.SelectedStoreIds = poll.Stores.ToArray();
                }
            }
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

            var polls = _pollService.GetPolls("", false, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = polls.Select(x =>
                {
                    var m = x.ToModel();
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
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
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //locales
            AddLocales(_languageService, model.Locales);

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
                poll.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                poll.Locales = UpdateLocales(poll, model);
                _pollService.InsertPoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = poll.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            return View(model);
        }

        public ActionResult Edit(string id)
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

            //Store
            PrepareStoresMappingModel(model, poll, false);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = poll.GetLocalized(x => x.Name, languageId, false, false);
            });


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
                poll.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                poll.Locales = UpdateLocales(poll, model);
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
        public ActionResult Delete(string id)
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
        public ActionResult PollAnswers(string pollId, DataSourceRequest command)
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

        /*
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
            pollAnswer.Name = model.Name;
            pollAnswer.DisplayOrder = model.DisplayOrder;
            _pollService.UpdatePoll(poll);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult PollAnswerAdd(string pollId, [Bind(Exclude = "Id")] PollAnswerModel model)
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
                Name = model.Name,
                PollId = pollId,
                DisplayOrder = model.DisplayOrder
            });
            _pollService.UpdatePoll(poll);

            return new NullJsonResult();
        }
        */


        //create
        public ActionResult PollAnswerCreatePopup(string pollId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var model = new PollAnswerModel();
            model.PollId = pollId;

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public ActionResult PollAnswerCreatePopup(string btnId, string formId, PollAnswerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var poll = _pollService.GetPollById(model.PollId);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var pa = model.ToEntity();
                pa.Locales = UpdateOptionLocales(pa, model);
                poll.PollAnswers.Add(pa);
                _pollService.UpdatePoll(poll);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult PollAnswerEditPopup(string id, string pollId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var pollAnswer = _pollService.GetPollById(pollId).PollAnswers.Where(x => x.Id == id).FirstOrDefault();
            if (pollAnswer == null)
                //No poll answer found with the specified id
                return RedirectToAction("List");

            var model = pollAnswer.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pollAnswer.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult PollAnswerEditPopup(string btnId, string formId, PollAnswerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePolls))
                return AccessDeniedView();

            var poll = _pollService.GetPollById(model.PollId);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            var pollAnswer = poll.PollAnswers.Where(x => x.Id == model.Id).FirstOrDefault();
            if (pollAnswer == null)
                //No poll answer found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                pollAnswer = model.ToEntity(pollAnswer);
                pollAnswer.Locales = UpdateOptionLocales(pollAnswer, model);

                _pollService.UpdatePoll(poll);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
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
