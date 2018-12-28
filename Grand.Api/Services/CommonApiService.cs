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

        public CommonApiService(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
            _languageDto = _mongoDBContext.Database().GetCollection<LanguageDto>(typeof(Core.Domain.Localization.Language).Name);
            _currencyDto = _mongoDBContext.Database().GetCollection<CurrencyDto>(typeof(Core.Domain.Directory.Currency).Name);
        }
        public virtual IMongoQueryable<LanguageDto> GetLanguages()
        {
            return _languageDto.AsQueryable();
        }
        public virtual IMongoQueryable<CurrencyDto> GetCurrencies()
        {
            return _currencyDto.AsQueryable();
        }
    }
}
