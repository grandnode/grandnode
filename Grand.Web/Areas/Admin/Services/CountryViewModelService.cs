using Grand.Core.Domain.Directory;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Directory;

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
            _countryService.InsertCountry(country);
            return country;
        }

        public virtual Country UpdateCountryModel(Country country, CountryModel model)
        {
            country = model.ToEntity(country);
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
            _stateProvinceService.InsertStateProvince(sp);
            return sp;
        }
        public virtual StateProvince UpdateStateProvinceModel(StateProvince sp, StateProvinceModel model)
        {
            sp = model.ToEntity(sp);
            _stateProvinceService.UpdateStateProvince(sp);
            return sp;
        }
    }
}
