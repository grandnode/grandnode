using Grand.Domain.Polls;
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
using System.Threading.Tasks;

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
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Constructors

        public PollController(IPollService pollService, ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreService storeService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper)
        {
            _pollService = pollService;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeService = storeService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Polls

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var polls = await _pollService.GetPolls("", false, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = polls.Select(x =>
                {
                    var m = x.ToModel(_dateTimeHelper);
                    return m;
                }),
                Total = polls.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = new PollModel
            {
                //default values
                Published = true,
                ShowOnHomePage = true
            };
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false);
            //locales
            await AddLocales(_languageService, model.Locales);
            //ACL
            await model.PrepareACLModel(null, false, _customerService);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(PollModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var poll = model.ToEntity(_dateTimeHelper);
                await _pollService.InsertPoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = poll.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true);
            //locales
            await AddLocales(_languageService, model.Locales);
            //ACL
            await model.PrepareACLModel(null, true, _customerService);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var poll = await _pollService.GetPollById(id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = poll.ToModel(_dateTimeHelper);
            //Store
            await model.PrepareStoresMappingModel(poll, _storeService, false);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = poll.GetLocalized(x => x.Name, languageId, false, false);
            });
            //ACL
            await model.PrepareACLModel(poll, false, _customerService);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(PollModel model, bool continueEditing)
        {
            var poll = await _pollService.GetPollById(model.Id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                poll = model.ToEntity(poll, _dateTimeHelper);
                await _pollService.UpdatePoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = poll.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            //Store
            await model.PrepareStoresMappingModel(poll, _storeService, true);
            //ACL
            await model.PrepareACLModel(poll, true, _customerService);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = poll.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var poll = await _pollService.GetPollById(id);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                await _pollService.DeletePoll(poll);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Polls.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = poll.Id });
        }

        #endregion

        #region Poll answer

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> PollAnswers(string pollId, DataSourceRequest command)
        {
            var poll = await _pollService.GetPollById(pollId);
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
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> PollAnswerCreatePopup(string pollId)
        {
            var model = new PollAnswerModel
            {
                PollId = pollId
            };

            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> PollAnswerCreatePopup(PollAnswerModel model)
        {
            var poll = await _pollService.GetPollById(model.PollId);
            if (poll == null)
                //No poll found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var pa = model.ToEntity();
                poll.PollAnswers.Add(pa);
                await _pollService.UpdatePoll(poll);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> PollAnswerEditPopup(string id, string pollId)
        {
            var pollAnswer = (await _pollService.GetPollById(pollId)).PollAnswers.Where(x => x.Id == id).FirstOrDefault();
            if (pollAnswer == null)
                //No poll answer found with the specified id
                return RedirectToAction("List");

            var model = pollAnswer.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pollAnswer.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> PollAnswerEditPopup(PollAnswerModel model)
        {
            var poll = await _pollService.GetPollById(model.PollId);
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
                await _pollService.UpdatePoll(poll);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> PollAnswerDelete(PollAnswer answer)
        {
            var pol = (await _pollService.GetPolls()).FirstOrDefault(x => x.PollAnswers.Any(y => y.Id == answer.Id));
            var pollAnswer = pol.PollAnswers.Where(x => x.Id == answer.Id).FirstOrDefault();
            if (pollAnswer == null)
                throw new ArgumentException("No poll answer found with the specified id", "id");
            if (ModelState.IsValid)
            {
                pol.PollAnswers.Remove(pollAnswer);
                await _pollService.UpdatePoll(pol);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion
    }
}
