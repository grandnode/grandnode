using Grand.Core;
using Grand.Framework.Components;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

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
            _workContext = workContext;
            _languageService = languageService;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new LanguageSelectorModel();
            model.CurrentLanguage = _workContext.WorkingLanguage.ToModel();
            model.AvailableLanguages = (await _languageService
                .GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }
    }
}