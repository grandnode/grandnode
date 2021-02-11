using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Core.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class TierPriceProfile : Profile, IAutoMapperProfile
    {
        public TierPriceProfile()
        {
            CreateMap<ProductTierPriceDto, TierPrice>();

            CreateMap<TierPrice, ProductTierPriceDto>();
        }

        public int Order => 1;
    }
}