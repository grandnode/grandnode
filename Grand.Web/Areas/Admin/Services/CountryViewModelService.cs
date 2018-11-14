using Grand.Core.Domain.Directory;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Directory;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CountryViewModelService : ICountryViewModelService
    {
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILanguageService _languageService;

        public CountryViewModelService(ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILanguageService languageService)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _languageService = languageService;
        }

        public virtual CountryModel PrepareCountryModel()
        {
            var model = new CountryModel();
            //default values
            model.Published = true;
            model.AllowsBilling = true;
            model.AllowsShipping = true;
            return model;
        }

        public virtual Country InsertCountryModel(CountryModel model)
        {
            var country = model.ToEntity();
            country.Locales = model.Locales.ToLocalizedProperty();
            country.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
            _countryService.InsertCountry(country);
            return country;
        }

        public virtual Country UpdateCountryModel(Country country, CountryModel model)
        {
            country = model.ToEntity(country);
            country.Locales = model.Locales.ToLocalizedProperty();
            country.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
            _countryService.UpdateCountry(country);
            return country;
        }

        public virtual StateProvinceModel PrepareStateProvinceModel(string countryId)
        {
            var model = new StateProvinceModel();
            model.CountryId = countryId;
            //default value
            model.Published = true;
            return model;
        }

        public virtual StateProvince InsertStateProvinceModel(StateProvinceModel model)
        {
            var sp = model.ToEntity();
            sp.Locales = model.Locales.ToLocalizedProperty();
            _stateProvinceService.InsertStateProvince(sp);
            return sp;
        }
        public virtual StateProvince UpdateStateProvinceModel(StateProvince sp, StateProvinceModel model)
        {
            sp = model.ToEntity(sp);
            sp.Locales = model.Locales.ToLocalizedProperty();
            _stateProvinceService.UpdateStateProvince(sp);
            return sp;
        }
    }
}
