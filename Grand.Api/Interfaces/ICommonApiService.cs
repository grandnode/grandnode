using Grand.Api.DTOs.Common;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface ICommonApiService
    {
        IMongoQueryable<LanguageDto> GetLanguages();
        IMongoQueryable<CurrencyDto> GetCurrencies();
        IMongoQueryable<StoreDto> GetStores();
        IMongoQueryable<CountryDto> GetCountries();
        IMongoQueryable<StateProvinceDto> GetStates();
        IMongoQueryable<PictureDto> GetPictures();
        Task<PictureDto> InsertPicture(PictureDto pictureDto);
        Task DeletePicture(PictureDto pictureDto);
        IMongoQueryable<MessageTemplateDto> GetCategoryMessageTemplate();
        IMongoQueryable<MessageTemplateDto> GetManufacturerMessageTemplate();
        IMongoQueryable<MessageTemplateDto> GetProductMessageTemplate();
    }
}
