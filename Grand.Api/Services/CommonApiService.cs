using Grand.Api.DTOs.Common;
using Grand.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public partial class CommonApiService : ICommonApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IMongoCollection<LanguageDto> _languageDto;
        private readonly IMongoCollection<CurrencyDto> _currencyDto;
        private readonly IMongoCollection<StoreDto> _storeDto;
        private readonly IMongoCollection<CountryDto> _countryDto;
        private readonly IMongoCollection<StateProvinceDto> _stateProvinceDto;

        public CommonApiService(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
            _languageDto = _mongoDBContext.Database().GetCollection<LanguageDto>(typeof(Core.Domain.Localization.Language).Name);
            _currencyDto = _mongoDBContext.Database().GetCollection<CurrencyDto>(typeof(Core.Domain.Directory.Currency).Name);
            _storeDto = _mongoDBContext.Database().GetCollection<StoreDto>(typeof(Core.Domain.Stores.Store).Name);
            _countryDto = _mongoDBContext.Database().GetCollection<CountryDto>(typeof(Core.Domain.Directory.Country).Name);
            _stateProvinceDto = _mongoDBContext.Database().GetCollection<StateProvinceDto>(typeof(Core.Domain.Directory.StateProvince).Name);
        }
        public virtual IMongoQueryable<LanguageDto> GetLanguages()
        {
            return _languageDto.AsQueryable();
        }
        public virtual IMongoQueryable<CurrencyDto> GetCurrencies()
        {
            return _currencyDto.AsQueryable();
        }
        public virtual IMongoQueryable<StoreDto> GetStores()
        {
            return _storeDto.AsQueryable();
        }
        public virtual IMongoQueryable<CountryDto> GetCountries()
        {
            return _countryDto.AsQueryable();
        }
        public virtual IMongoQueryable<StateProvinceDto> GetStates()
        {
            return _stateProvinceDto.AsQueryable();
        }
    }
}
