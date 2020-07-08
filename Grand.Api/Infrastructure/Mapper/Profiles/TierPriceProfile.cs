using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class TierPriceProfile : Profile, IMapperProfile
    {
        public TierPriceProfile()
        {
            CreateMap<ProductTierPriceDto, TierPrice>();

            CreateMap<TierPrice, ProductTierPriceDto>();
        }

        public int Order => 1;
    }
}