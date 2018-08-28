using Grand.Core;
using Grand.Framework.Components;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;

namespace Grand.Web.Areas.Admin.Components
{
    public class CommonLanguageSelectorViewComponent : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly IStoreContext _storeContext;

        public CommonLanguageSelectorViewComponent(
            IWorkContext workContext,
            ILanguageService languageService, 
            IStoreContext storeContext
            )
        {
            this._workContext = workContext;
            this._languageService = languageService;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()//original Action name: LanguageSelector
        {
            var model = new LanguageSelectorModel();
            model.CurrentLanguage = _workContext.WorkingLanguage.ToModel();
            model.AvailableLanguages = _languageService
                .GetAllLanguages(storeId: _storeContext.CurrentStore.Id)
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }
    }
}