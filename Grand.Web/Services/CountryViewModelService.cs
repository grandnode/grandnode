using Grand.Core;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class CountryViewModelService : ICountryViewModelService
    {
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        public CountryViewModelService(ICountryService countryService,
           IStateProvinceService stateProvinceService,
           ILocalizationService localizationService,
           IWorkContext workContext)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        public virtual async Task<List<StateProvinceModel>> PrepareModel(string countryId, bool addSelectStateItem)
        {
            var country = await _countryService.GetCountryById(countryId);
            var states = await _stateProvinceService.GetStateProvincesByCountryId(country != null ? country.Id : "", _workContext.WorkingLanguage.Id);
            var model = (from s in states select new StateProvinceModel { id = s.Id, name = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) }).ToList();
            if (country == null)
            {
                //country is not selected ("choose country" item)
                if (addSelectStateItem)
                {
                    model.Insert(0, new StateProvinceModel { id = "", name = _localizationService.GetResource("Address.SelectState") });
                }
                else
                {
                    model.Insert(0, new StateProvinceModel { id = "", name = _localizationService.GetResource("Address.OtherNonUS") });
                }
            }
            else
            {
                //some country is selected
                if (!model.Any())
                {
                    //country does not have states
                    model.Insert(0, new StateProvinceModel { id = "", name = _localizationService.GetResource("Address.OtherNonUS") });
                }
                else
                {
                    //country has some states
                    if (addSelectStateItem)
                    {
                        model.Insert(0, new StateProvinceModel { id = "", name = _localizationService.GetResource("Address.SelectState") });
                    }
                }
            }
            return model;
        }

    }
}