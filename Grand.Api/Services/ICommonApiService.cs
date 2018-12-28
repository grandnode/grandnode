using Grand.Api.DTOs.Common;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface ICommonApiService
    {
        IMongoQueryable<LanguageDto> GetLanguages();
        IMongoQueryable<CurrencyDto> GetCurrencies();
        IMongoQueryable<StoreDto> GetStores();
    }
}
