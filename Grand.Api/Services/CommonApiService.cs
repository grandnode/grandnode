using Grand.Api.DTOs.Common;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Data;
using Grand.Services.Media;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public partial class CommonApiService : ICommonApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly IPictureService _pictureService;
        private readonly IMongoCollection<LanguageDto> _languageDto;
        private readonly IMongoCollection<CurrencyDto> _currencyDto;
        private readonly IMongoCollection<StoreDto> _storeDto;
        private readonly IMongoCollection<CountryDto> _countryDto;
        private readonly IMongoCollection<StateProvinceDto> _stateProvinceDto;
        private readonly IMongoCollection<PictureDto> _pictureDto;

        public CommonApiService(IMongoDBContext mongoDBContext, IPictureService pictureService)
        {
            _mongoDBContext = mongoDBContext;
            _pictureService = pictureService;
            _languageDto = _mongoDBContext.Database().GetCollection<LanguageDto>(typeof(Core.Domain.Localization.Language).Name);
            _currencyDto = _mongoDBContext.Database().GetCollection<CurrencyDto>(typeof(Core.Domain.Directory.Currency).Name);
            _storeDto = _mongoDBContext.Database().GetCollection<StoreDto>(typeof(Core.Domain.Stores.Store).Name);
            _countryDto = _mongoDBContext.Database().GetCollection<CountryDto>(typeof(Core.Domain.Directory.Country).Name);
            _stateProvinceDto = _mongoDBContext.Database().GetCollection<StateProvinceDto>(typeof(Core.Domain.Directory.StateProvince).Name);
            _pictureDto = _mongoDBContext.Database().GetCollection<PictureDto>(typeof(Core.Domain.Media.Picture).Name);
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
        public virtual IMongoQueryable<PictureDto> GetPictures()
        {
            return _pictureDto.AsQueryable();
        }
        public virtual PictureDto InsertPicture(PictureDto pictureDto)
        {
            var picture = _pictureService.InsertPicture(pictureDto.PictureBinary, pictureDto.MimeType, pictureDto.SeoFilename, pictureDto.AltAttribute, pictureDto.TitleAttribute, pictureDto.IsNew);
            return picture.ToModel();
        }
        public virtual void DeletePicture(PictureDto pictureDto)
        {
            var picture = _pictureService.GetPictureById(pictureDto.Id);
            if (picture != null)
            {
                _pictureService.DeletePicture(picture);
            }
        }
        public virtual IMongoQueryable<MessageTemplateDto> GetCategoryMessageTemplate()
        {
            return _mongoDBContext.Database().GetCollection<MessageTemplateDto>(typeof(Core.Domain.Catalog.CategoryTemplate).Name).AsQueryable();
        }
        public virtual IMongoQueryable<MessageTemplateDto> GetManufacturerMessageTemplate()
        {
            return _mongoDBContext.Database().GetCollection<MessageTemplateDto>(typeof(Core.Domain.Catalog.ManufacturerTemplate).Name).AsQueryable();
        }
        public virtual IMongoQueryable<MessageTemplateDto> GetProductMessageTemplate()
        {
            return _mongoDBContext.Database().GetCollection<MessageTemplateDto>(typeof(Core.Domain.Catalog.ProductTemplate).Name).AsQueryable();
        }

    }
}
