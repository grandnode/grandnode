using Grand.Core;
using Grand.Domain.Localization;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class LanguageViewModelService : ILanguageViewModelService
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Constructors

        public LanguageViewModelService(ILanguageService languageService,
            ILocalizationService localizationService,
            ICurrencyService currencyService)
        {
            _localizationService = localizationService;
            _languageService = languageService;
            _currencyService = currencyService;
        }

        #endregion

        public virtual void PrepareFlagsModel(LanguageModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.FlagFileNames = Directory
                .EnumerateFiles(CommonHelper.WebMapPath("/content/images/flags/"), "*.png", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .ToList();
        }

        public virtual async Task PrepareCurrenciesModel(LanguageModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //templates
            model.AvailableCurrencies.Add(new SelectListItem
            {
                Text = "---",
                Value = ""
            });
            var currencies = await _currencyService.GetAllCurrencies(true);
            foreach (var currency in currencies)
            {
                model.AvailableCurrencies.Add(new SelectListItem
                {
                    Text = currency.Name,
                    Value = currency.Id.ToString()
                });
            }
        }

        public virtual async Task<Language> InsertLanguageModel(LanguageModel model)
        {
            var language = model.ToEntity();
            await _languageService.InsertLanguage(language);
            return language;
        }
        public virtual async Task<Language> UpdateLanguageModel(Language language, LanguageModel model)
        {
            //update
            language = model.ToEntity(language);
            await _languageService.UpdateLanguage(language);
            return language;
        }
        public virtual async Task<(bool error, string message)> InsertLanguageResourceModel(LanguageResourceModel model)
        {
            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            var res = await _localizationService.GetLocaleStringResourceByName(model.Name, model.LanguageId, false);
            if (res == null)
            {
                var resource = new LocaleStringResource { LanguageId = model.LanguageId };
                resource.ResourceName = model.Name;
                resource.ResourceValue = model.Value;
                await _localizationService.InsertLocaleStringResource(resource);
            }
            else
            {
                return (error: true, message: string.Format(_localizationService.GetResource("Admin.Configuration.Languages.Resources.NameAlreadyExists"), model.Name));
            }
            return (false, string.Empty);
        }
        public virtual async Task<(bool error, string message)> UpdateLanguageResourceModel(LanguageResourceModel model)
        {
            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();
            var resource = await _localizationService.GetLocaleStringResourceById(model.Id);
            // if the resourceName changed, ensure it isn't being used by another resource
            if (!resource.ResourceName.Equals(model.Name, StringComparison.OrdinalIgnoreCase))
            {
                var res = await _localizationService.GetLocaleStringResourceByName(model.Name, model.LanguageId, false);
                if (res != null && res.Id != resource.Id)
                {
                    return (error: true, message: string.Format(_localizationService.GetResource("Admin.Configuration.Languages.Resources.NameAlreadyExists"), res.ResourceName));
                }
            }
            resource.ResourceName = model.Name;
            resource.ResourceValue = model.Value;
            await _localizationService.UpdateLocaleStringResource(resource);
            return (false, string.Empty);
        }
        public virtual async Task<(IEnumerable<LanguageResourceModel> languageResourceModels, int totalCount)> PrepareLanguageResourceModel(LanguageResourceFilterModel model, string languageId, int pageIndex, int pageSize)
        {
            var language = await _languageService.GetLanguageById(languageId);

            var resources = _localizationService
                .GetAllResources(languageId)
                .OrderBy(x => x.ResourceName)
                .Select(x => new LanguageResourceModel
                {
                    LanguageId = languageId,
                    Id = x.Id,
                    Name = x.ResourceName,
                    Value = x.ResourceValue,
                });

            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.ResourceName))
                    resources = resources.Where(x => x.Name.ToLowerInvariant().Contains(model.ResourceName.ToLowerInvariant()));
                if (!string.IsNullOrEmpty(model.ResourceValue))
                    resources = resources.Where(x => x.Value.ToLowerInvariant().Contains(model.ResourceValue.ToLowerInvariant()));
            }
            resources = resources.AsQueryable();
            return (resources.Skip((pageIndex - 1) * pageSize).Take(pageSize), resources.Count());
        }
    }
}
