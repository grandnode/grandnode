using Grand.Api.DTOs.Common;
using MongoDB.Driver.Linq;

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
        PictureDto InsertPicture(PictureDto pictureDto);
        void DeletePicture(PictureDto pictureDto);
        IMongoQueryable<MessageTemplateDto> GetCategoryMessageTemplate();
        IMongoQueryable<MessageTemplateDto> GetManufacturerMessageTemplate();
        IMongoQueryable<MessageTemplateDto> GetProductMessageTemplate();
    }
}
