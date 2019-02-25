using Grand.Core.Domain.Polls;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Polls;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Polls;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Polls)]
    public partial class PollController : BaseAdminController
	{
		#region Fields
        private readonly IPollService _pollService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Constructors

        public PollController(IPollService pollService, ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreService storeService,
            ICustomerService customerService)
        {
            this._pollService = pollService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._storeService = storeService;
            this._customerService = customerService;
        }

        #endregion

        #region Polls

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var polls = _pollService.GetPolls("", false, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = polls.Select(x =>
                {
                    var m = x.ToModel();
                    return m;
                }),
                Total = polls.TotalCount
            };

            return Json(gridModel);
        }

        public IActionResult Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new PollModel();
            //default values
            model.Published = true;
            model.ShowOnHomePage = true;
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            //locales
            AddLocales(_languageService, model.Locales);
            //ACL
            model.PrepareACLModel(null, false, _customerService);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(PollModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var poll = model.ToEntity();                
                _pollService.InsertPoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = poll.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);
            //locales
            AddLocales(_languageService, model.Locales);
            //ACL
            model.PrepareACLModel(null, true, _customerService);

            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var poll = _pollService.GetPollById(id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = poll.ToModel();
            //Store
            model.PrepareStoresMappingModel(poll, false, _storeService);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = poll.GetLocalized(x => x.Name, languageId, false, false);
            });
            //ACL
            model.PrepareACLModel(poll, false, _customerService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(PollModel model, bool continueEditing)
        {
            var poll = _pollService.GetPollById(model.Id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                poll = model.ToEntity(poll);
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

            //Store
            model.PrepareStoresMappingModel(poll, true, _storeService);
            //ACL
            model.PrepareACLModel(poll, true, _customerService);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = poll.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var poll = _pollService.GetPollById(id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                _pollService.DeletePoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = poll.Id });
        }

        #endregion

        #region Poll answer
        
        [HttpPost]
        public IActionResult PollAnswers(string pollId, DataSourceRequest command)
        {
            var poll = _pollService.GetPollById(pollId);
            if (poll == null)
                throw new ArgumentException("No poll found with the specified id", "pollId");

            var answers = poll.PollAnswers.OrderBy(x=>x.DisplayOrder).ToList();

            var gridModel = new DataSourceResult
            {
                Data = answers.Select(x=>x.ToModel()),
                Total = answers.Count
            };

            return Json(gridModel);
        }
        //create
        public IActionResult PollAnswerCreatePopup(string pollId)
        {
            var model = new PollAnswerModel();
            model.PollId = pollId;

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public IActionResult PollAnswerCreatePopup(PollAnswerModel model)
        {
            var poll = _pollService.GetPollById(model.PollId);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var pa = model.ToEntity();
                poll.PollAnswers.Add(pa);
                _pollService.UpdatePoll(poll);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult PollAnswerEditPopup(string id, string pollId)
        {
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
        public IActionResult PollAnswerEditPopup(PollAnswerModel model)
        {
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
                _pollService.UpdatePoll(poll);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult PollAnswerDelete(PollAnswer answer)
        {
            var pol = _pollService.GetPollById(answer.PollId);
            var pollAnswer = pol.PollAnswers.Where(x => x.Id == answer.Id).FirstOrDefault();
            if (pollAnswer == null)
                throw new ArgumentException("No poll answer found with the specified id", "id");
            if (ModelState.IsValid)
            {
                pol.PollAnswers.Remove(pollAnswer);
                _pollService.UpdatePoll(pol);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion
    }
}
