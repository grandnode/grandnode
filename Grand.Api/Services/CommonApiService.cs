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

        public CommonApiService(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
            _languageDto = _mongoDBContext.Database().GetCollection<LanguageDto>(typeof(Core.Domain.Localization.Language).Name);
        }
        
        public virtual IMongoQueryable<LanguageDto> GetLanguages()
        {
            return _languageDto.AsQueryable();
        }
    }
}
