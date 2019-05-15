using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class CountryViewModelService: ICountryViewModelService
    {
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;

        public CountryViewModelService(ICountryService countryService,
           IStateProvinceService stateProvinceService,
           ILocalizationService localizationService,
           IWorkContext workContext,
           ICacheManager cacheManager)
        {
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
        }

        public virtual async Task<dynamic> PrepareModel(string countryId, bool addSelectStateItem)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.STATEPROVINCES_BY_COUNTRY_MODEL_KEY, countryId, addSelectStateItem, _workContext.WorkingLanguage.Id);
            var cacheModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var country = await _countryService.GetCountryById(countryId);
                var states = await _stateProvinceService.GetStateProvincesByCountryId(country != null ? country.Id : "", _workContext.WorkingLanguage.Id);
                var result = (from s in states
                              select new { id = s.Id, name = s.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) })
                              .ToList();


                if (country == null)
                {
                    //country is not selected ("choose country" item)
                    if (addSelectStateItem)
                    {
                        result.Insert(0, new { id = "", name = _localizationService.GetResource("Address.SelectState") });
                    }
                    else
                    {
                        result.Insert(0, new { id = "", name = _localizationService.GetResource("Address.OtherNonUS") });
                    }
                }
                else
                {
                    //some country is selected
                    if (!result.Any())
                    {
                        //country does not have states
                        result.Insert(0, new { id = "", name = _localizationService.GetResource("Address.OtherNonUS") });
                    }
                    else
                    {
                        //country has some states
                        if (addSelectStateItem)
                        {
                            result.Insert(0, new { id = "", name = _localizationService.GetResource("Address.SelectState") });
                        }
                    }
                }

                return result;
            });

            return cacheModel;

        }

    }
}